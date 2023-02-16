using BenchmarkDotNet.Attributes;
using ClickHouse.Client.ADO;
using DoubletsVsClickHouseBenchmarks;

namespace Platform.Data.Doublets.Benchmarks;

[SimpleJob]
[MemoryDiagnoser]
public class DoubletsVsClickHouseBenchmarks
{
    private static string WorkingDirectory = Environment.CurrentDirectory;
    private static string ProjectDirectory = Directory.GetParent(WorkingDirectory).Parent.Parent.FullName;
    private static string CsvFilePath = Path.Join(ProjectDirectory, "MSFT.csv");
    private static ClickHouseConnection ClickHouseConnection = new ClickHouseConnection(Environment.GetEnvironmentVariable(nameof(ClickHouseConnection)));
    private static DateTimeOffset MaximumStartingTime = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.Now.ToUnixTimeSeconds());
    private static DateTimeOffset MinimumStartingTime = DateTimeOffset.Now.AddMonths(-1);
    private List<Candle> Candles;

    private IEnumerable<IBenchmarkable> Benchmarkables = new IBenchmarkable[]
    {
        new DoubletsAdapter<UInt64>(),
        new ClickHouseAdapter(ClickHouseConnection)
    };

    [ParamsSource(nameof(Benchmarkables))] private IBenchmarkable Benchmarkable { get; set; }

    private Candle ParseCandleFromCsv(string[] cvsValues)
    {
        Candle candle = new Candle()
        {
            StartingTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(cvsValues[0])),
            OpeningPrice = decimal.Parse(cvsValues[1]),
            ClosingPrice = decimal.Parse(cvsValues[2]),
            HighestPrice = decimal.Parse(cvsValues[3]),
            LowestPrice = decimal.Parse(cvsValues[4]),
            Volume = long.Parse(cvsValues[5]),
        };
        return candle;
    }

    [GlobalSetup]
    public void GlobalSetup()
    {
        List<Candle> candles = new List<Candle>();
        using (var reader = new StreamReader(path: CsvFilePath))
        {
            while (!reader.EndOfStream)
            {
                string? line = reader.ReadLine();
                string[] values = line.Split(separator: ';');
                Candle candle = ParseCandleFromCsv(values);
                candles.Add(candle);
            }
        }
        Candles = candles;
    }
    
    [Benchmark]
    public void LinksPlatformBenchmark()
    {
        Benchmarkable.SaveCandles(Candles);
        Benchmarkable.GetCandles(MinimumStartingTime, MaximumStartingTime);
    }
}
