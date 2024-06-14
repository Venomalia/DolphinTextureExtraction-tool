using Benchmark.Benchmarks;
using BenchmarkDotNet.Running;
using System.Buffers.Binary;

namespace Benchmark
{
    public class Benchmarker
    {
        static void Main(string[] args)
        {
            var Result = BenchmarkRunner.Run<ReadUInt64>();
        }
    }
}
