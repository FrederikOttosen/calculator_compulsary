﻿using System.Net;
using System.Text;
using addition.Models;
using EasyNetQ;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System.Text.Json;
using Polly;
using Polly.Retry;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace addition.Controllers
{
    [ApiController]
    [Route("api/addition")]
    public class AdditionController : ControllerBase
    {
        private readonly ILogger<AdditionController> _logger;
        private const string BaseUrl = "http://storage-handler/";
        private HttpClient _httpClient = new HttpClient();
        private AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        public AdditionController(ILogger<AdditionController> logger)
        {
            _logger = logger;
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _retryPolicy = Policy.HandleResult<HttpResponseMessage>(response =>
                !response.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(3));
        }

        // [HttpPost]
        // public async Task<ActionResult<decimal>> Add([FromBody] AdditionRequest request)
        // {
        //     try
        //     {
        //         using (var activity = Monitoring.ActivitySource.StartActivity("Addition"))
        //         {
        //             Console.WriteLine("Starting 'Addition' activity.");
        //             decimal result = request.Number1 + request.Number2;
        //             await SendToRabbitMQ(request.Number1, request.Number2, result);
        //             Console.WriteLine("Ending 'Addition' activity.");
        //             var calculationEntity = new CalculationEntity
        //             {
        //                 Expression = $"{request.Number1} + {request.Number2}",
        //                 Result = result
        //             };
        //
        //             var saveCalculationrequest = new RestRequest("storage", Method.Post);
        //             saveCalculationrequest.AddJsonBody(calculationEntity);
        //
        //             var response = await _restClient.ExecuteAsync<List<CalculationEntity>>(saveCalculationrequest);
        //
        //             ResponseDto responseDto = new ResponseDto
        //             {
        //                 Response = result
        //             };
        //             
        //             // var getCalculationRequest = new RestRequest("storage", Method.Get);
        //             // var responseFromGet = await _restClient.ExecuteAsync<List<CalculationEntity>>(getCalculationRequest);
        //             if (response is { IsSuccessful: true, Data.Count: > 0 })
        //             {
        //                 responseDto.History = response.Data;
        //             }
        //             else
        //             {
        //                 responseDto.History = null;
        //                 Console.WriteLine("Failed to retrieve history: " + response.ErrorMessage);
        //             }
        //             return Ok(responseDto);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Exception occurred: {ex.Message}. StackTrace: {ex.StackTrace}");
        //         return BadRequest(ex.Message);
        //     }
        // }
        
        [HttpPost]
        public async Task<ActionResult<decimal>> Add([FromBody] AdditionRequest? request)
        {
            if (request == null)
            {
                return BadRequest("Invalid input data");
            }
    
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
    
            return Ok(new ResponseDto
            {
                Response = result,
                History = history
            });
        }
        


        private async Task SendToRabbitMQ(decimal num1, decimal num2, decimal result)
        {
            try
            {
                using (var bus = RabbitHutch.CreateBus("host=rmq;username=application;password=pass"))
                {
                    var message = new Message($"Addition performed: {num1} + {num2} = {result}");
                    await bus.PubSub.PublishAsync(message);
                    _logger.LogInformation("Message sent to RabbitMQ successfully.");
                }
            }
            catch (EasyNetQException enqEx)
            {
                // Log more detailed information from the EasyNetQ exception
                _logger.LogError(enqEx, "Detailed error from RabbitMQ client.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending message to RabbitMQ.");
                throw;
            }
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
