using System;
using BenchmarkDotNet.Attributes;

namespace Fare.Benchmarking
{
    public class Benchmark
    {
        private readonly Random random = new Random();

        [Benchmark]
        public Xeger XegerCtorSimple() => new Xeger(".");

        [Benchmark]
        public Xeger XegerCtorInjectRandom() => new Xeger(".", this.random);
    }
}
