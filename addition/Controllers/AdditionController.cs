using System.Text;
using addition.Models;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Retry;
using OpenTelemetry.Trace;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace addition.Controllers
{
    [ApiController]
    [Route("api/addition")]
    public class AdditionController : ControllerBase
    {
        private const string BaseUrl = "http://storage-handler/api/";
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
        public Task<ActionResult<decimal>> Add([FromBody] AdditionRequest? request)
        {
            using var startSpan = _tracer.StartActiveSpan("Addition_Started");
            if (request == null)
            {
                return Task.FromResult<ActionResult<decimal>>(BadRequest("Invalid input data"));
            }
        
            using var calculationSpan = _tracer.StartActiveSpan("Addition_Performing");
            decimal result = request.Number1 + request.Number2;

            var _ = StoreCalculationAndFetchHistory($"{request.Number1} + {request.Number2}", result); 

            using var returnSpan = _tracer.StartActiveSpan("Addition_Completed");
            Console.WriteLine("Here we exit the Api");
            return Task.FromResult<ActionResult<decimal>>(Ok(new ResponseDto
            {
                Response = result
            }));
        }
        
        private async Task StoreCalculationAndFetchHistory(string expression, decimal result)
        {
            var calculationEntity = new CalculationEntity
            {
                Expression = expression,
                Result = result
            };
        
            var serializedCalculationEntity = JsonSerializer.Serialize(calculationEntity);
            var content = new StringContent(serializedCalculationEntity, Encoding.UTF8, "application/json");
        
            // Execute Save Request with Polly will retry 3 times with waiting in between
            await _retryPolicy.ExecuteAsync(() =>  _httpClient.PostAsync("storage", content));
        }

    }
}
