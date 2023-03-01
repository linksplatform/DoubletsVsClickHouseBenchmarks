using System.Globalization;
using BenchmarkDotNet.Attributes;
using ClickHouse.Client.ADO;
using CsvHelper;
using CsvHelper.Configuration;
using DoubletsVsClickHouseBenchmarks;
using Newtonsoft.Json;

namespace Platform.Data.Doublets.Benchmarks;

[SimpleJob]
[MemoryDiagnoser]
public class DoubletsVsClickHouseBenchmarks
{
    // public static string ProjectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
    public static string CsvFilePath = "/workspace/DoubletsVsClickHouseBenchmarks/MSFT.csv";
    public static ClickHouseConnection ClickHouseConnection = new ClickHouseConnection(Environment.GetEnvironmentVariable(nameof(ClickHouseConnection)));
    public static DateTimeOffset MaximumStartingTime = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.Now.ToUnixTimeSeconds());
    public static DateTimeOffset MinimumStartingTime = DateTimeOffset.Now.AddMonths(-1);
    public List<Candle> Candles;

    public IEnumerable<IBenchmarkable> Benchmarkables { get; } = new IBenchmarkable[]
    {
        new ClickHouseAdapter(ClickHouseConnection),
        new DoubletsAdapter<UInt64>(),
    };

    [ParamsSource(nameof(Benchmarkables))] public IBenchmarkable Benchmarkable { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        Candles = new CsvCandleParser().Parse(CsvFilePath).ToList();
    }
    
    [Benchmark]
    public async Task LinksPlatformBenchmark()
    {
        await Benchmarkable.RemoveCandles();
        await Benchmarkable.SaveCandles(Candles);
        await Benchmarkable.GetCandles(MinimumStartingTime, MaximumStartingTime);
    }
}
