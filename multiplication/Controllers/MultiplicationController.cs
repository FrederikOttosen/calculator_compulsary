using addition.Models;
using Microsoft.AspNetCore.Mvc;
using multiplication.Models;
using RestSharp;
using OpenTelemetry.Trace;

namespace multiplication.Controllers;

[ApiController]
[Route("api/multiplication")]
public class MultiplicationController : ControllerBase
{
    private const string BaseUrl = "http://storage-handler/";
    private static readonly RestClient RestClient = new RestClient(BaseUrl);
    private readonly Tracer _tracer;
    
    public MultiplicationController(Tracer tracer)
    {
        _tracer = tracer;
    }
    
    [HttpPost]
    public async Task<ActionResult<decimal>> Multiply([FromBody] MultiplicationRequest? request)
    {
        using var startSpan = _tracer.StartActiveSpan("Multiplication_Started");
        
        if (request == null || request.Number2 == 0) { return BadRequest("Invalid input data"); }
    
        using var calculationSpan = _tracer.StartActiveSpan("Multiplication_Performing");
        var result = request.Number1 * request.Number2;

        List<CalculationEntity> history = null;
        try
        {
            history = await StoreCalculationAndFetchHistory($"{request.Number1} * {request.Number2}", result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in storage/history retrieval: {ex.Message}. StackTrace: {ex.StackTrace}");
        }
        
        using var returnSpan = _tracer.StartActiveSpan("Multiplication_Completed");
        return Ok(new ResponseDto
        {
            Response = result,
            History = history
        });
    }
    
    private async Task<List<CalculationEntity>> StoreCalculationAndFetchHistory(string expression, decimal result)
    {
        var calculationEntity = new CalculationEntity
        {
            Expression = expression,
            Result = result
        };

        var saveCalculationrequest = new RestRequest("storage", Method.Post);
        saveCalculationrequest.AddJsonBody(calculationEntity);
    
        var saveResponse = await RestClient.ExecuteAsync(saveCalculationrequest);
        if (!saveResponse.IsSuccessful)
        {
            Console.WriteLine("Failed to store calculation: " + saveResponse.ErrorMessage);
            throw new Exception("Failed to store calculation");
        }

        var getCalculationRequest = new RestRequest("storage", Method.Get);
        var getResponse = await RestClient.ExecuteAsync<List<CalculationEntity>>(getCalculationRequest);
        if (!getResponse.IsSuccessful || getResponse.Data?.Count == 0)
        {
            Console.WriteLine("Failed to retrieve history: " + getResponse.ErrorMessage);
            throw new Exception("Failed to retrieve history");
        }
    
        return getResponse.Data;
    }
}