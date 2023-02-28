# DoubletsVsClickHouseBenchmarks

##  ClickHouse prerequsitions
1. Install ClickHouse locally by usin [Install ClickHouse Documentation](https://clickhouse.com/docs/en/install)  
2. Enable PostgresQL protocol to your server by using [PostgreSQL Interface Documentation](https://clickhouse.com/docs/en/interfaces/postgresql/)
3. Create table `candles` by using this SQL statement:
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

# Additional information
## ClickHouseConnection for gitpod
export ClickHouseConnection="Host=localhost;Protocol=http;Port=8123;Username=default;Password=123;Timeout=500"

