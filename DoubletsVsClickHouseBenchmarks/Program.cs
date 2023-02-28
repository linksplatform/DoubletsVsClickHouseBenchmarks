
using System.Globalization;
using System.Xml;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ClickHouse.Client.ADO;
using CommandLine;
using CsvHelper;
using CsvHelper.Configuration;
using DoubletsVsClickHouseBenchmarks;
using Newtonsoft.Json;
using TLinkAddress = System.UInt64;

namespace Platform.Data.Doublets.Benchmarks
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
