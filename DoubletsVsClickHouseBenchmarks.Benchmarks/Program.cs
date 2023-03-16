using System.Reflection;
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
            // var a = Parser.Default.ParseArguments<Options>(args);
            var summary = BenchmarkRunner.Run<DoubletsVsClickHouseBenchmarks>();   
        }
    }
}
