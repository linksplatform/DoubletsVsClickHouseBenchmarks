# DoubletsVsClickHouseBenchmarks
```sql
CREATE TABLE candles (
    StartingTime TIMESTAMP NOT NULL,
    OpeningPrice DECIMAL(18, 8) NOT NULL,
    ClosingPrice DECIMAL(18, 8) NOT NULL,
    HighestPrice DECIMAL(18, 8) NOT NULL,
    LowestPrice DECIMAL(18, 8) NOT NULL,
    Volume BIGINT NOT NULL,
    PRIMARY KEY (StartingTime)
) ENGINE = MergeTree()
ORDER BY StartingTime;
```
