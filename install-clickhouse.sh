#!/usr/bin/expect

spawn sudo ./clickhouse install

expect "Enter password for default user: "
send -- "123"

expect eof
