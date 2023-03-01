using System.Collections;
using System.Numerics;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;
using ClickHouse.Client.Utility;
using Platform.Collections.Stacks;
using Platform.Converters;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.CriterionMatchers;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Sequences.Converters;
using Platform.Data.Doublets.Sequences.Numbers.Raw;
using Platform.Data.Doublets.Sequences.Unicode;
using Platform.Data.Doublets.Sequences.Walkers;
using Platform.Data.Numbers.Raw;
using Platform.IO;
using Platform.Memory;

namespace DoubletsVsClickHouseBenchmarks;

public class ClickHouseAdapter : IBenchmarkable
{
    public Uri restApiUri = new Uri("http://localhost:8123");
    public ClickHouseConnection ClickHouseConnection;
    
    public ClickHouseAdapter(ClickHouseConnection сlickHouseConnection)
    {
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
    using var bulkCopyInterface = new ClickHouseBulkCopy(ClickHouseConnection)
    {
        DestinationTableName = "candles"
    };

    foreach (var candle in candles)
    {
        var candleRows = new List<Object[]> {new object[] { candle.StartingTime, candle.OpeningPrice, candle.ClosingPrice, candle.LowestPrice, candle.HighestPrice, candle.Volume }};
        await bulkCopyInterface.WriteToServerAsync(candleRows);
    }
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
        return candles;
    }

    public async Task RemoveCandles(DateTimeOffset minimumStartingTime, DateTimeOffset maximumStartingTime)
    {
        var candles = await GetCandles(minimumStartingTime, maximumStartingTime);
        foreach (var candle in candles)
        {
            await ClickHouseConnection.ExecuteStatementAsync(@$"
DELETE FROM candles
WHERE 
	starting_time == ${candle.StartingTime.ToUnixTimeSeconds()}
");
        }
    }
}
