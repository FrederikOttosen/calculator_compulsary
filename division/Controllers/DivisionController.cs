using System.Text;
using System.Text.Json;
using addition.Models;
using Microsoft.AspNetCore.Mvc;
using division.Models;
using OpenTelemetry.Trace;
using Polly;
using Polly.Retry;
using RestSharp;

namespace division.Controllers;

[ApiController]
[Route("api/division")]
public class DivisionController: ControllerBase
{
    
    private const string BaseUrl = "http://storage-handler/";
    private HttpClient _httpClient = new HttpClient();
    private AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly Tracer _tracer;

    public DivisionController(Tracer tracer)
    {
        _tracer = tracer;
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _retryPolicy = Policy.HandleResult<HttpResponseMessage>(response =>
                !response.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(3));
    }
    
    [HttpPost]
    public async Task<ActionResult<decimal>> Divide([FromBody] DivisionRequest request)
    {
        using var startSpan = _tracer.StartActiveSpan("Division_Started");
        if (request == null || request.Number2 == 0)
        {
            return BadRequest("Invalid input data");
        }
    
        using var calculationSpan = _tracer.StartActiveSpan("Division_Performing");
        decimal result = request.Number1 / request.Number2;

        List<CalculationEntity>? history = null;
        try
        {
            history = await StoreCalculationAndFetchHistory($"{request.Number1} / {request.Number2}", result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in storage/history retrieval: {ex.Message}. StackTrace: {ex.StackTrace}");
        }
        using var returnSpan = _tracer.StartActiveSpan("Division_Completed");
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