namespace addition.Models;

public class CalculationEntity
{
    public int CalculationId { get; set; } 
    public string Expression { get; set; }
    public decimal Result { get; set; } 
    public DateTime Timestamp { get; set; }
    public override string ToString()
    {
        return $"CalculationId: {CalculationId}, Expression: {Expression}, Result: {Result}, Timestamp: {Timestamp}";
    }
}