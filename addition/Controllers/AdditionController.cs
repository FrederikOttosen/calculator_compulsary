using System.Net;
using System.Text;
using addition.Models;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using Polly;
using Polly.Retry;
using System.Text.Json;
using OpenTelemetry.Trace;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace addition.Controllers
{
    [ApiController]
    [Route("api/addition")]
    public class AdditionController : ControllerBase
    {
        private const string BaseUrl = "http://storage-handler/";
        private HttpClient _httpClient = new HttpClient();
        private AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly Tracer _tracer;
        
        public AdditionController(Tracer tracer, ILogger<AdditionController> logger)
        {
            _tracer = tracer;
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _retryPolicy = Policy.HandleResult<HttpResponseMessage>(response =>
                    !response.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(3));
        }

        [HttpPost]
        public async Task<ActionResult<decimal>> Add([FromBody] AdditionRequest? request)
        {
            using var startSpan = _tracer.StartActiveSpan("Addition_Started");
            if (request == null)
            {
                return BadRequest("Invalid input data");
            }
    
            using var calculationSpan = _tracer.StartActiveSpan("Addition_Performing");
            decimal result = request.Number1 + request.Number2;

            List<CalculationEntity>? history = null;
            try
            {
                history = await StoreCalculationAndFetchHistory($"{request.Number1} + {request.Number2}", result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in storage/history retrieval: {ex.Message}. StackTrace: {ex.StackTrace}");
            }
    
            using var returnSpan = _tracer.StartActiveSpan("Addition_Completed");
            return Ok(new ResponseDto
            {
                Response = result,
                History = history
            });
        }
        
        
        private async Task<List<CalculationEntity>?> StoreCalculationAndFetchHistory(string expression, decimal result)
        {
            var calculationEntity = new CalculationEntity
            {
                Expression = expression,
                Result = result
            };

            var saveCalculationRequest = new RestRequest("storage", Method.Post);
            saveCalculationRequest.AddJsonBody(calculationEntity);

            var serializedCalculationEntity = JsonSerializer.Serialize(calculationEntity);
            var content = new StringContent(serializedCalculationEntity, Encoding.UTF8, "application/json");

            // Execute Save Request with Polly
            HttpResponseMessage saveResponse = await _retryPolicy.ExecuteAsync(() => _httpClient.PostAsync("storage", content));
         
            if (!saveResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed to store calculation: " + saveResponse.StatusCode);
                throw new Exception("Failed to store calculation");
            }
            var typedResult = await saveResponse.Content.ReadFromJsonAsync<List<CalculationEntity>>();
            return typedResult;
        }

    }
}
