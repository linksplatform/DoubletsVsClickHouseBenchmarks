namespace DoubletsVsClickHouseBenchmarks;

public struct Candle
{
    public DateTimeOffset StartingTime {get; set;} 
    public decimal OpeningPrice {get; set;} 
    public decimal ClosingPrice {get; set;} 
    public decimal HighestPrice {get; set;} 
    public decimal LowestPrice {get; set;} 
    public long Volume {get; set;} 
}
