using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace DoubletsVsClickHouseBenchmarks
{
    
    // public class Options
    // {
    //     [Option( longName: "csvFilePath", Required = true, HelpText = "Path of CSV file which contains candles.")]
    //     public bool CsvFilePath { get; set; }
    //     
    //     [Option( longName: "clickHouseConnectionString", Required = true, HelpText = "Path of CSV file which contains candles.")]
    //     public bool ClickHouseConnectionString { get; set; }
    // }
    
    class Program
    {
        static void Main(string[] args)
        {
            var config = DefaultConfig.Instance.WithSummaryStyle(
                SummaryStyle.Default.WithMaxParameterColumnWidth(100));
            BenchmarkRunner.Run<DoubletsVsClickHouseBenchmarks>(config);
        }
    }
}
