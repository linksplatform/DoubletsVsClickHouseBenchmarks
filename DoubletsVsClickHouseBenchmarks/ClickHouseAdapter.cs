using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;
using ClickHouse.Client.Utility;

namespace DoubletsVsClickHouseBenchmarks.Library;

public class ClickHouseAdapter : IBenchmarkable
{
    public ClickHouseConnection ClickHouseConnection;
    
    public ClickHouseAdapter(ClickHouseConnection сlickHouseConnection)
    {
        Console.WriteLine($"сlickHouseConnection: {сlickHouseConnection}");
        ClickHouseConnection = сlickHouseConnection;
        ClickHouseConnection.ExecuteStatementAsync(@"
  CREATE TABLE IF NOT EXISTS candles (
    starting_time TIMESTAMP NOT NULL,
    opening_price DECIMAL(18, 8) NOT NULL,
    closing_price DECIMAL(18, 8) NOT NULL,
    highest_price DECIMAL(18, 8) NOT NULL,
    lowest_price DECIMAL(18, 8) NOT NULL,
    volume BIGINT NOT NULL,
  PRIMARY KEY (starting_time)
  ) ENGINE = MergeTree()
  ORDER BY starting_time;
").Wait();
    }
    
public async Task SaveCandles(IList<Candle> candles)
{
    // Console.WriteLine($"Saving {candles.Count} candles");
    using var bulkCopyInterface = new ClickHouseBulkCopy(ClickHouseConnection)
    {
        DestinationTableName = "candles"
    };

    foreach (var candle in candles)
    {
        var candleRows = new List<Object[]> {new object[] { candle.StartingTime, candle.OpeningPrice, candle.ClosingPrice, candle.LowestPrice, candle.HighestPrice, candle.Volume }};
        await bulkCopyInterface.WriteToServerAsync(candleRows);
        // Console.WriteLine($"Saving {candle} #{i}");
    }
        // Console.WriteLine($"Saved {candles.Count} candles");
    // var chunkedCandles = candles.Chunk(100000);
    //
    // using var bulkCopyInterface = new ClickHouseBulkCopy(ClickHouseConnection)
    // {
    //     DestinationTableName = "candles"
    // };
    //
    // foreach (var candlesChunk in chunkedCandles)
    // {
    //     var candleRows = candlesChunk.Select((candle) => new object[] { candle.StartingTime, candle.OpeningPrice, candle.ClosingPrice, candle.LowestPrice, candle.HighestPrice, candle.Volume });
    //     await bulkCopyInterface.WriteToServerAsync(candleRows);
    // }
}
    
    public async Task<IList<Candle>> GetCandles(DateTimeOffset minimumStartingTime, DateTimeOffset maximumStartingTime)
    {
        // Console.WriteLine($"Getting candles");
        List<Candle> candles = new List<Candle>();
        string sql = @$"
SELECT * FROM candles
WHERE 
	({minimumStartingTime.ToUnixTimeSeconds()} < starting_time)
	AND
	(starting_time < {maximumStartingTime.ToUnixTimeSeconds()})
";
        using var reader = await ClickHouseConnection.ExecuteReaderAsync(sql);
        while (reader.Read())
        {
            Candle candle = new Candle()
            {
                StartingTime = reader.GetDateTime(0),
                OpeningPrice = reader.GetDecimal(1),
                ClosingPrice = reader.GetDecimal(2),
                LowestPrice = reader.GetDecimal(3),
                HighestPrice = reader.GetDecimal(4),
                Volume = reader.GetInt64(5)
            };
            candles.Add(candle);
        }
        // Console.WriteLine($"Got {candles.Count} candles");
        return candles;
    }

    public async Task DeleteCandles(DateTimeOffset minimumStartingTime, DateTimeOffset maximumStartingTime)
    {
        // Console.WriteLine($"Removing candles");
        var candles = await GetCandles(minimumStartingTime, maximumStartingTime);
        foreach (var candle in candles)
        {
            await ClickHouseConnection.ExecuteStatementAsync(@$"
DELETE FROM candles
WHERE 
	starting_time == ${candle.StartingTime.ToUnixTimeSeconds()}
");
        }
        // Console.WriteLine($"Removed {candles.Count} candles");
    }
}
