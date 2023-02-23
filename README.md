# DoubletsVsClickHouseBenchmarks

##  ClickHouse prerequsitions
1. Install ClickHouse locally by usin [Install ClickHouse Documentation](https://clickhouse.com/docs/en/install)  
2. Enable PostgresQL protocol to your server by using [PostgreSQL Interface Documentation](https://clickhouse.com/docs/en/interfaces/postgresql/)
3. Create table `candles` by using this SQL statement:
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
