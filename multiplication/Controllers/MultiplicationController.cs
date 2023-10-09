using Microsoft.AspNetCore.Mvc;
using multiplication.Models;

namespace multiplication.Controllers;

[ApiController]
[Route("api/multiplication")]
public class MultiplicationController : ControllerBase
{
    [HttpPost]
    public ActionResult<decimal> Multiply([FromBody] MultiplicationRequest request)
    {
        try
        {
            decimal result = request.Number1 - request.Number2;
            // You can store the calculation result in your database here.
            return Ok(result);
        }
        catch (Exception ex)
        {
            // Handle any errors or exceptions here.
            return BadRequest(ex.Message);
        }
    }
}