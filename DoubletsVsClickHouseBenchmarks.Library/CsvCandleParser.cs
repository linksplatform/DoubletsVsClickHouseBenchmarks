using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace DoubletsVsClickHouseBenchmarks;

public class CsvCandleParser
{
    public IList<Candle> Parse(string csvFilePath)
    {
        List<Candle> candles = new List<Candle>();
        using var reader = new StreamReader(path: csvFilePath);  
        CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            NewLine = Environment.NewLine,
            Delimiter = ";",
        };
        using var csv = new CsvReader(reader, config);
        csv.Read();
        while (csv.Read())
        {
            var candle = new Candle
            {
                StartingTime = DateTimeOffset.FromUnixTimeMilliseconds(csv.GetField<long>(0)),
                OpeningPrice = csv.GetField<decimal>(1),
                ClosingPrice = csv.GetField<decimal>(2),
                HighestPrice = csv.GetField<decimal>(3),
                LowestPrice = csv.GetField<decimal>(4),
                Volume = csv.GetField<long>(5)
            };
            candles.Add(candle);
        }
        return candles;
    }
}
