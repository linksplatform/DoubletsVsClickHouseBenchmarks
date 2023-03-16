namespace DoubletsVsClickHouseBenchmarks.Library;

public interface IBenchmarkable
{
    Task SaveCandles(IList<Candle> candles);
    Task<IList<Candle>> GetCandles(DateTimeOffset minimumStartingTime, DateTimeOffset maximumStartingTime);
    Task DeleteCandles(DateTimeOffset minimumStartingTime, DateTimeOffset maximumStartingTime);
}
