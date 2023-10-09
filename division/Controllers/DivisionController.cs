using Microsoft.AspNetCore.Mvc;
using division.Models;

namespace division.Controllers;

[ApiController]
[Route("api/division")]
public class DivisionController: ControllerBase
{
    [HttpPost]
    public ActionResult<decimal> Divide([FromBody] DivisionRequest request)
    {
        try
        {
            decimal result = request.Number1 / request.Number2;
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