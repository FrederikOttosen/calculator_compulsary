using System;

namespace storage_handler.Entities;

public class CalculationEntity
{
    public int CalculationId { get; set; } 
    public string Expression { get; set; }
    public double Result { get; set; } 
    public DateTime Timestamp { get; set; }
}