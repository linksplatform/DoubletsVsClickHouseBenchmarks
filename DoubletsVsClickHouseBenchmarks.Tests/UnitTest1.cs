using ClickHouse.Client.ADO;

namespace DoubletsVsClickHouseBenchmarks.Tests;

public class UnitTest1
{
    public string CsvFilePath = "/home/freephoenix888/Programming/linksplatform/DoubletsVsClickHouseBenchmarks/DoubletsVsClickHouseBenchmarks/MSFT.csv";
    public static DateTimeOffset MaximumStartingTime = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.Now.ToUnixTimeSeconds());
    public static DateTimeOffset MinimumStartingTime = DateTimeOffset.Now.AddMonths(-1);
    public static ClickHouseConnection ClickHouseConnection = new ClickHouseConnection(Environment.GetEnvironmentVariable(nameof(ClickHouseConnection)));
    
    public static IEnumerable<object[]> Benchmarkables =>
        new List<IBenchmarkable[]>
        {
            new IBenchmarkable[] {new DoubletsAdapter<UInt64>()},
            new IBenchmarkable[] {new ClickHouseAdapter(ClickHouseConnection)},
        };
    
    [Theory]
    [MemberData(nameof(Benchmarkables))]
    public async void Test(IBenchmarkable benchmarkable)
    {
        var candles = new CsvCandleParser().Parse(CsvFilePath);
        await benchmarkable.RemoveCandles(MinimumStartingTime, MaximumStartingTime);
        await benchmarkable.SaveCandles(candles);
        await benchmarkable.GetCandles(MinimumStartingTime, MaximumStartingTime);
    }
}
