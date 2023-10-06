using Microsoft.AspNetCore.Mvc;
using subtraction.Models;

namespace subtraction.Controllers;

[ApiController]
[Route("api/subtraction")]
public class SubtractionController : ControllerBase
{
    [HttpPost]
    public ActionResult<decimal> Subtract([FromBody] SubtractionRequest request)
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