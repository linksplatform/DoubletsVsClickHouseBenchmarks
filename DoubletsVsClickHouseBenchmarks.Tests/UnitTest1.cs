namespace DoubletsVsClickHouseBenchmarks.Tests;

public class UnitTest1
{
    public string CsvFilePath = "/home/freephoenix888/Programming/linksplatform/DoubletsVsClickHouseBenchmarks/DoubletsVsClickHouseBenchmarks/MSFT.csv";
    public static DateTimeOffset MaximumStartingTime = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.Now.ToUnixTimeSeconds());
    public static DateTimeOffset MinimumStartingTime = DateTimeOffset.Now.AddMonths(-1);
    
    [Fact]
    public async void DoubletsAdapter()
    {
        var doubletsAdapter = new DoubletsAdapter<UInt64>();
        var candles = new CsvCandleParser().Parse(CsvFilePath);
        await doubletsAdapter.RemoveCandles();
        await doubletsAdapter.SaveCandles(candles);
        await doubletsAdapter.GetCandles(MinimumStartingTime, MaximumStartingTime);
    }
}
