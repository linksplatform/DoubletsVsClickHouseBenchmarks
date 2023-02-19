namespace DoubletsVsClickHouseBenchmarks;

public interface IBenchmarkable
{
    Task SaveCandles(IList<Candle> candles);
    Task<IList<Candle>> GetCandles(DateTimeOffset minimumStartingTime, DateTimeOffset maximumStartingTime);
    Task RemoveCandles(IList<Candle> candles);
}
