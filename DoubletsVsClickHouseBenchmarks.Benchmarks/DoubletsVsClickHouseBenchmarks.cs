using System.Globalization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using ClickHouse.Client.ADO;
using CsvHelper;
using CsvHelper.Configuration;
using DoubletsVsClickHouseBenchmarks;
using Newtonsoft.Json;

namespace Platform.Data.Doublets.Benchmarks;


[MemoryDiagnoser]
[ShortRunJob]
public class DoubletsVsClickHouseBenchmarks
{
     public static string workingDirectory = Environment.CurrentDirectory;
     public static DirectoryInfo projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent;
     public static DirectoryInfo solutionDirectory = projectDirectory.Parent;
    public static string CsvFilePath = Path.Join(solutionDirectory.FullName, "MSFT.csv");
    public static ClickHouseConnection ClickHouseConnection;
    public static DateTimeOffset MaximumStartingTime = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.Now.ToUnixTimeSeconds());
    public static DateTimeOffset MinimumStartingTime = DateTimeOffset.Now.AddMonths(-1);
    public List<Candle> Candles;
    
    [GlobalSetup]
    public void GlobalSetup()
    {
         var connectionString = Environment.GetEnvironmentVariable(nameof(ClickHouseConnection));
         if (connectionString == null)
         {
              throw new Exception("ClickHouseConnection environment variable must be set");
         }
         ClickHouseConnection = new ClickHouseConnection(connectionString);
         Candles = new CsvCandleParser().Parse(CsvFilePath).ToList();
    }
    
    public IEnumerable<IBenchmarkable> Benchmarkables { get; } = new IBenchmarkable[]
    {
        new ClickHouseAdapter(ClickHouseConnection),
        new DoubletsAdapter<UInt64>(),
    };

    [ParamsSource(nameof(Benchmarkables))] public IBenchmarkable Benchmarkable { get; set; }

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
