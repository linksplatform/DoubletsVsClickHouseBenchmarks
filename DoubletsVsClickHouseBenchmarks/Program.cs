using BenchmarkDotNet.Attributes;
using DoubletsVsClickHouseBenchmarks;
using TLinkAddress = System.UInt64;

namespace Platform.Data.Doublets.Benchmarks
{
    [SimpleJob]
    [MemoryDiagnoser]
    public class DoubletsVsClickHouseBenchmarks
    {
        public static string csvFilePath = "";
        public static DateTimeOffset MaximumTime = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.Now.ToUnixTimeSeconds());
        public static DateTimeOffset MinimumTime = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UnixEpoch.ToUnixTimeSeconds());
        
        [Benchmark]
        public void LinksPlatformBenchmark()
        {
            Doublets<TLinkAddress> doublets = new Doublets<ulong>();
            doublets.SaveCandles(csvFilePath);
            
        }
    }
}
