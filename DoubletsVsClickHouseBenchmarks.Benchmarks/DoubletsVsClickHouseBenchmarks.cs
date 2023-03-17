using System.Reflection;
using BenchmarkDotNet.Attributes;
using ClickHouse.Client.ADO;
using DoubletsVsClickHouseBenchmarks.Library;

namespace DoubletsVsClickHouseBenchmarks;


[MemoryDiagnoser]
[ShortRunJob]
public class DoubletsVsClickHouseBenchmarks
{
    public static string CsvFilePath = Environment.GetEnvironmentVariable(nameof(CsvFilePath));
    public static ClickHouseConnection ClickHouseConnection;
    public static List<Candle> Candles;
    public static System.Random Random;

    [GlobalSetup]
    static public void GlobalSetup()
    {
         var connectionString = Environment.GetEnvironmentVariable(nameof(ClickHouseConnection));
         Console.WriteLine($"{nameof(ClickHouseConnection)}: {ClickHouseConnection}");
         if (connectionString == null)
         {
              throw new Exception("ClickHouseConnection environment variable must be set");
         }
         ClickHouseConnection = new ClickHouseConnection(connectionString);
         Candles = new CsvCandleParser().Parse(CsvFilePath).ToList();
         Console.WriteLine($"{nameof(Candles)}: {Candles}");
         Random = new System.Random();
    }
    
    public IEnumerable<IBenchmarkable> Benchmarkables { get;} = new IBenchmarkable[]
    {
         new ClickHouseAdapter(ClickHouseConnection),
         new DoubletsAdapter<UInt64>(),
    };

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

    [ParamsSource(nameof(Benchmarkables))]
    public IBenchmarkable Benchmarkable { get; set; }

    [IterationSetup]
    public void IterationSetup()
    {

    }

    [IterationSetup(Target = nameof(DeleteBenchmark))]
    public void DeleteIterationSetup()
    {
         Benchmarkable.SaveCandles(Candles).Wait();
    }

    [Benchmark]
    public void DeleteBenchmark()
    {
         var minAndMaxStartingTimes = GenerateRandomMinAndMaxStartingTimes();
         Benchmarkable.DeleteCandles(minAndMaxStartingTimes.Item1, minAndMaxStartingTimes.Item2).Wait();
    }

    [Benchmark]
    public void SaveBenchmark()
    {
         Benchmarkable.SaveCandles(Candles).Wait();
    }
    
    [IterationCleanup(Target = nameof(SaveBenchmark))]
    public void SaveIterationCleanup()
    {
         var minAndMaxStartingTimes = GenerateRandomMinAndMaxStartingTimes();
         Benchmarkable.DeleteCandles(minAndMaxStartingTimes.Item1, minAndMaxStartingTimes.Item2).Wait();
    }

    [IterationSetup(Target = nameof(GetBenchmark))]
    public void GetIterationSetup()
    {
         Benchmarkable.SaveCandles(Candles).Wait();
    }
    
    [Benchmark]
    public void GetBenchmark()
    {
         var minAndMaxStartingTimes = GenerateRandomMinAndMaxStartingTimes();
         Benchmarkable.GetCandles(minAndMaxStartingTimes.Item1, minAndMaxStartingTimes.Item2).Wait();
    }

    [IterationCleanup(Target = nameof(GetBenchmark))]
    public void GetIterationCleanup()
    {
         var minAndMaxStartingTimes = GenerateRandomMinAndMaxStartingTimes();
         Benchmarkable.DeleteCandles(minAndMaxStartingTimes.Item1, minAndMaxStartingTimes.Item2).Wait();
    }

}
