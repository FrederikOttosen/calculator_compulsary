using addition.Models;

namespace subtraction.Models;

public class ResponseDto
{
    public decimal Response { get; set; } 
    public List<CalculationEntity>? History { get; set; }
}