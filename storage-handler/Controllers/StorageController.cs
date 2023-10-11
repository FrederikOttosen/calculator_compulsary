using System.Data;
using System.Data.Dabber;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using storage_handler.Entities;

namespace storage_handler.Controllers;

[ApiController]
[Route("[controller]")]
public class StorageController : ControllerBase
{
    private static IDbConnection _dbConnection = new MySqlConnection("Server=calculation-history-db;Database=calculation-history-database;Uid=calculatorHistory;Pwd=C@ch3d1v;");

    public StorageController(IDbConnection calculatinHistoryDbConnection)
    {
        calculatinHistoryDbConnection.Open();
        var tables = calculatinHistoryDbConnection.Query<string>("SHOW TABLES LIKE 'calculations'");
        if (!tables.Any())
        {
            calculatinHistoryDbConnection.Execute("CREATE TABLE calculations (calculationId INT primary key AUTO_INCREMENT, expression VARCHAR(255) not null, result DECIMAL(10, 2), timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP)");
        }
    }
    
    [HttpGet]
    public IActionResult Get()
    {
        try
        {
            var result = FetchCalculations();
            Console.WriteLine(result);
            if (result == null || result.Count == 0)
            {
                return NotFound("No calculations found.");
            }
            return Ok(result);
        }
        catch(Exception ex)
        {
            // Log the exception here
            return StatusCode(500, "Internal server error. Unable to fetch calculations.");
        }
    }

    [HttpPost]
    public IActionResult Post([FromBody] CalculationEntity calculationEntity)
    {
        if (calculationEntity == null)
        {
            return BadRequest("Calculation is null");
        }

        try
        {
            _dbConnection.Execute(
                "INSERT INTO calculations (expression, result) VALUES (@Expression, @Result); ",
                new { Expression = calculationEntity.Expression, Result = calculationEntity.Result});

            var history = FetchCalculations();
            
            return Ok(history);
        }
        catch(Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpDelete]
    public IActionResult DeleteAllCalculations()
    {
        try
        {
            // Execute SQL to delete all records from the Calculations table
            _dbConnection.Execute("DELETE FROM calculations WHERE calculationId > 0 ;");
        
            return Ok("All calculations deleted.");
        }
        catch(Exception ex)
        {
            // Log the exception here
            return StatusCode(500, "Internal server error. Unable to delete calculations.");
        }
    }

    private List<CalculationEntity> FetchCalculations()
    {
        var sqlQuery = "SELECT * FROM calculations"; // Replace with your actual SQL query
        return _dbConnection.Query<CalculationEntity>(sqlQuery).ToList();
    }

}