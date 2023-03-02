using System.Globalization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using ClickHouse.Client.ADO;
using CsvHelper;
using CsvHelper.Configuration;
using DoubletsVsClickHouseBenchmarks;
using Newtonsoft.Json;

namespace Platform.Data.Doublets.Benchmarks;

[SimpleJob]
[MemoryDiagnoser]
[SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 1, iterationCount: 1, invocationCount:1)]
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
    
    [IterationSetup(Target = nameof(DeleteBenchmark))]
    public void DeleteIterationSetup()
    {
         Benchmarkable.SaveCandles(Candles).Wait();
    }

    [Benchmark]
    public void DeleteBenchmark()
    {
         Benchmarkable.DeleteCandles(MinimumStartingTime, MaximumStartingTime).Wait();
    }

    [Benchmark]
    public void SaveBenchmark()
    {
         Benchmarkable.SaveCandles(Candles).Wait();
    }
    
    [IterationCleanup(Target = nameof(SaveBenchmark))]
    public void SaveIterationCleanup()
    {
         Benchmarkable.DeleteCandles(MinimumStartingTime, MaximumStartingTime).Wait();
    }

    [IterationSetup(Target = nameof(GetBenchmark))]
    public void GetIterationSetup()
    {
         Benchmarkable.SaveCandles(Candles).Wait();
    }
    
    [Benchmark]
    public void GetBenchmark()
    {
         Benchmarkable.GetCandles(MinimumStartingTime, MaximumStartingTime).Wait();
    }

    [IterationCleanup(Target = nameof(GetBenchmark))]
    public void GetIterationCleanup()
    {
         Benchmarkable.DeleteCandles(MinimumStartingTime, MaximumStartingTime).Wait();
    }

}
