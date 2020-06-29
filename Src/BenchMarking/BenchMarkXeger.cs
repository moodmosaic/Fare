using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;


namespace BenchMarking
{
    public class BenchMarkXeger
    {
        private const string pattern = ".";

        [Benchmark]
        public Fare.PR56Option1AndAHalf.Xeger Option1AndAHalf() => new Fare.PR56Option1AndAHalf.Xeger(pattern);

        [Benchmark]
        public Fare.Xeger OriginalImplementation() => new Fare.Xeger(pattern);

        

        [Benchmark]
        public Fare.PR56Option2.Xeger Option2() => new Fare.PR56Option2.Xeger(pattern);

        [Benchmark]
        public Fare.PR56Option3.Xeger Option3() => new Fare.PR56Option3.Xeger(pattern);
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<BenchMarkXeger>();
        }
    }
}
