using addition.Models;
using EasyNetQ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace addition.Controllers
{
    [ApiController]
    [Route("api/addition")]
    public class AdditionController : ControllerBase
    {
        private readonly ILogger<AdditionController> _logger;

        public AdditionController(ILogger<AdditionController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<decimal>> Add([FromBody] AdditionRequest request)
        {
            try
            {
                using (var activity = Monitoring.ActivitySource.StartActivity("Addition"))
                {
                    Console.WriteLine("Starting 'Addition' activity.");
                    decimal result = request.Number1 + request.Number2;
                    await SendToRabbitMQ(request.Number1, request.Number2, result);
                    Console.WriteLine("Ending 'Addition' activity.");
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}. StackTrace: {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
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

    }
}
