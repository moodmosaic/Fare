using BenchmarkDotNet.Running;

namespace Fare.Benchmarking
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Benchmark>();
        }
    }
}