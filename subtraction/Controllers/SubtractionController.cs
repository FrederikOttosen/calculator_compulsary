﻿using addition.Models;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using subtraction.Models;
using OpenTelemetry.Trace;

namespace subtraction.Controllers;

[ApiController]
[Route("api/subtraction")]
public class SubtractionController : ControllerBase
{
    private const string BaseUrl = "http://storage-handler/";
    private static readonly RestClient RestClient = new RestClient(BaseUrl);
    private readonly Tracer _tracer;
    
    public SubtractionController(Tracer tracer)
    {
        _tracer = tracer;
    }
    
    [HttpPost]
    public async Task<ActionResult<decimal>> Subtract([FromBody] SubtractionRequest? request)
    {
        using var startSpan = _tracer.StartActiveSpan("Subtraction_Started");
        if (request == null) { return BadRequest("Invalid input data"); }
    
        using var calculationSpan = _tracer.StartActiveSpan("Subtraction_Performing");
        var result = request.Number1 - request.Number2;

        List<CalculationEntity> history = null;
        try
        {
            history = await StoreCalculationAndFetchHistory($"{request.Number1} - {request.Number2}", result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in storage/history retrieval: {ex.Message}. StackTrace: {ex.StackTrace}");
        }
    
        using var returnSpan = _tracer.StartActiveSpan("Subtraction_Completed");
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