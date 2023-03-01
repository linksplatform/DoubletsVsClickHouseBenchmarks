# DoubletsVsClickHouseBenchmarks

##  ClickHouse prerequsitions
1. Install ClickHouse locally by using [Install ClickHouse Documentation](https://clickhouse.com/docs/en/install)  
2. Start clickhouse server by using
```
sudo clickhouse start
```
3. Create table `candles`
  1. Execute `clickhouse-client` command line program
  2. Write the password
  3. Insert this query:
  ```sql
  CREATE TABLE candles (
    starting_time TIMESTAMP NOT NULL,
    opening_price DECIMAL(18, 8) NOT NULL,
    closing_price DECIMAL(18, 8) NOT NULL,
    highest_price DECIMAL(18, 8) NOT NULL,
    lowest_price DECIMAL(18, 8) NOT NULL,
    volume BIGINT NOT NULL,
  PRIMARY KEY (starting_time)
  ) ENGINE = MergeTree()
  ORDER BY starting_time;
  ```
  4. Execute quit
4. Run benchmarks
```
cd DoubletsVsClickHouseBenchmarks && dotnet run --configuration Release
```

# Additional information
## ClickHouseConnection for gitpod
export ClickHouseConnection="Host=localhost;Protocol=http;Port=8123;Username=default;Password=123;Timeout=500"

