using System;
using BenchmarkDotNet.Attributes;

namespace Fare.Benchmarking
{
    public class Benchmark
    {
        private const string Pattern = ".";
        private readonly Random random = new Random();

        [Benchmark]
        public Xeger XegerCtorSimple() => new Xeger(Pattern);

        [Benchmark]
        public Xeger XegerCtorInjectRandom() => new Xeger(Pattern, this.random);
    }
}