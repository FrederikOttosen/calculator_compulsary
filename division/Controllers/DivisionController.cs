using System.Text;
using System.Text.Json;
using addition.Models;
using Microsoft.AspNetCore.Mvc;
using division.Models;
using OpenTelemetry.Trace;
using Polly;
using Polly.Retry;

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
    public Task<ActionResult<decimal>> Divide([FromBody] DivisionRequest? request)
    {
        using var startSpan = _tracer.StartActiveSpan("Division_Started");
        if (request == null || request.Number2 == 0)
        {
            return Task.FromResult<ActionResult<decimal>>(BadRequest("Invalid input data"));
        }
    
        using var calculationSpan = _tracer.StartActiveSpan("Division_Performing");
        decimal result = request.Number1 / request.Number2;

        var _ = StoreCalculationAndFetchHistory($"{request.Number1} / {request.Number2}", result);
        
        using var returnSpan = _tracer.StartActiveSpan("Division_Completed");
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