using System.Text;
using System.Text.Json;
using addition.Models;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Retry;
using RestSharp;
using OpenTelemetry.Trace;
using subtraction.Models;

namespace subtraction.Controllers;

[ApiController]
[Route("api/subtraction")]
public class SubtractionController : ControllerBase
{
    private const string BaseUrl = "http://storage-handler/api/";
    private HttpClient _httpClient = new HttpClient();
    private AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly Tracer _tracer;

    public SubtractionController(Tracer tracer)
    {
        _tracer = tracer;
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _retryPolicy = Policy.HandleResult<HttpResponseMessage>(response =>
                !response.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(3));
    }
    
    [HttpPost]
    public Task<ActionResult<decimal>> Subtract([FromBody] SubtractionRequest? request)
    {
        using var startSpan = _tracer.StartActiveSpan("Subtraction_Started");
        if (request == null)
        {
            return Task.FromResult<ActionResult<decimal>>(BadRequest("Invalid input data"));
        }
    
        using var calculationSpan = _tracer.StartActiveSpan("Subtraction_Performing");
        decimal result = request.Number1 - request.Number2;
        
        _ = StoreCalculationAndFetchHistory($"{request.Number1} - {request.Number2}", result);
        
        using var returnSpan = _tracer.StartActiveSpan("Subtraction_Completed");
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