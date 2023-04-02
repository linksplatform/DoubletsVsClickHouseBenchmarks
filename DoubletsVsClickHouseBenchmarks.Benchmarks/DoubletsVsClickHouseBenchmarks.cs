using System.ComponentModel.DataAnnotations;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using ClickHouse.Client.ADO;
using DoubletsVsClickHouseBenchmarks.Library;

namespace DoubletsVsClickHouseBenchmarks;

[Config(typeof(Config))]
[MemoryDiagnoser]
public class DoubletsVsClickHouseBenchmarks
{
     private class Config : ManualConfig
     {
          public Config()
          {
               
               AddJob(Job.ShortRun.WithWarmupCount(1).WithLaunchCount(1).WithIterationCount(1));
          }
     }
    public static string CsvFilePath;
    public static ClickHouseConnection ClickHouseConnection;
    public static List<Candle> Candles;
    public static System.Random Random;
    public static DoubletsAdapter<UInt64> DoubletsAdapter;
    public static ClickHouseAdapter ClickHouseAdapter;

    static DoubletsVsClickHouseBenchmarks()
    {
         CsvFilePath = Environment.GetEnvironmentVariable(nameof(CsvFilePath)) ?? throw new Exception($"{nameof(CsvFilePath)} environment variable must be set");
         Candles = new CsvCandleParser().Parse(CsvFilePath).ToList();
         Random = new System.Random();
         var clickHouseConnectionString = Environment.GetEnvironmentVariable(nameof(ClickHouseConnection)) ?? throw new Exception($"{nameof(ClickHouseConnection)} environment variable must be set"); 
         ClickHouseConnection = new ClickHouseConnection(clickHouseConnectionString);
         DoubletsAdapter = new DoubletsAdapter<UInt64>();
         ClickHouseAdapter = new ClickHouseAdapter(ClickHouseConnection);
    }

    public (DateTimeOffset, DateTimeOffset) GenerateRandomMinAndMaxStartingTimes()
    {
         var randomStartingTime0 = Random.NextInt64(DateTimeOffset.Now.AddMonths(-1).ToUnixTimeSeconds(), DateTimeOffset.Now.ToUnixTimeSeconds());
         var randomStartingTime1 = Random.NextInt64(DateTimeOffset.Now.AddMonths(-1).ToUnixTimeSeconds(), DateTimeOffset.Now.ToUnixTimeSeconds());
         if (randomStartingTime0 > randomStartingTime1)
         {
              return (DateTimeOffset.FromUnixTimeSeconds(randomStartingTime0), DateTimeOffset.FromUnixTimeSeconds(randomStartingTime1));
         }
         else
         {
              return (DateTimeOffset.FromUnixTimeSeconds(randomStartingTime1), DateTimeOffset.FromUnixTimeSeconds(randomStartingTime0));
         }
    }

    [IterationSetup(Target = nameof(ClickHouseDeleteBenchmark))]
    public void ClickHouseDeleteIterationSetup()
    {
         ClickHouseAdapter.SaveCandles(Candles).Wait();
    }
    
    [IterationSetup(Target = nameof(DoubletsDeleteBenchmark))]
    public void DoubletsDeleteIterationSetup()
    {
         DoubletsAdapter.SaveCandles(Candles).Wait();
    }

    [Benchmark]
    public void ClickHouseDeleteBenchmark()
    {
         var minAndMaxStartingTimes = GenerateRandomMinAndMaxStartingTimes();
         ClickHouseAdapter.DeleteCandles(minAndMaxStartingTimes.Item1, minAndMaxStartingTimes.Item2).Wait();
    }
    
    [Benchmark]
    public void DoubletsDeleteBenchmark()
    {
         var minAndMaxStartingTimes = GenerateRandomMinAndMaxStartingTimes();
         DoubletsAdapter.DeleteCandles(minAndMaxStartingTimes.Item1, minAndMaxStartingTimes.Item2).Wait();
    }

    [Benchmark]
    public void ClickHouseSaveBenchmark()
    {
         ClickHouseAdapter.SaveCandles(Candles).Wait();
    }
    
    [Benchmark]
    public void DoubletsSaveBenchmark()
    {
         DoubletsAdapter.SaveCandles(Candles).Wait();
    }
    
    [IterationCleanup(Target = nameof(ClickHouseSaveBenchmark))]
    public void ClickHouseSaveIterationCleanup()
    {
         var minAndMaxStartingTimes = GenerateRandomMinAndMaxStartingTimes();
         DoubletsAdapter.DeleteCandles(minAndMaxStartingTimes.Item1, minAndMaxStartingTimes.Item2).Wait();
    }
    
    [IterationCleanup(Target = nameof(DoubletsSaveBenchmark))]
    public void DoubletsSaveIterationCleanup()
    {
         var minAndMaxStartingTimes = GenerateRandomMinAndMaxStartingTimes();
         DoubletsAdapter.DeleteCandles(minAndMaxStartingTimes.Item1, minAndMaxStartingTimes.Item2).Wait();
    }

    [IterationSetup(Target = nameof(ClickHouseGetBenchmark))]
    public void ClickHouseGetIterationSetup()
    {
         ClickHouseAdapter.SaveCandles(Candles).Wait();
    }
    
    [IterationSetup(Target = nameof(DoubletsGetBenchmark))]
    public void DoubletsGetIterationSetup()
    {
         DoubletsAdapter.SaveCandles(Candles).Wait();
    }
    
    [Benchmark]
    public void ClickHouseGetBenchmark()
    {
         var minAndMaxStartingTimes = GenerateRandomMinAndMaxStartingTimes();
         ClickHouseAdapter.GetCandles(minAndMaxStartingTimes.Item1, minAndMaxStartingTimes.Item2).Wait();
    }
    
    [Benchmark]
    public void DoubletsGetBenchmark()
    {
         var minAndMaxStartingTimes = GenerateRandomMinAndMaxStartingTimes();
         DoubletsAdapter.GetCandles(minAndMaxStartingTimes.Item1, minAndMaxStartingTimes.Item2).Wait();
    }

    [IterationCleanup(Target = nameof(ClickHouseGetBenchmark))]
    public void ClickHouseGetIterationCleanup()
    {
         var minAndMaxStartingTimes = GenerateRandomMinAndMaxStartingTimes();
         ClickHouseAdapter.DeleteCandles(minAndMaxStartingTimes.Item1, minAndMaxStartingTimes.Item2).Wait();
    }
    
    [IterationCleanup(Target = nameof(DoubletsGetBenchmark))]
    public void DoubletsGetIterationCleanup()
    {
         var minAndMaxStartingTimes = GenerateRandomMinAndMaxStartingTimes();
         DoubletsAdapter.DeleteCandles(minAndMaxStartingTimes.Item1, minAndMaxStartingTimes.Item2).Wait();
    }

}
