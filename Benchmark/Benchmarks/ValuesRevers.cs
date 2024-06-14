using BenchmarkDotNet.Attributes;

namespace Benchmark.Benchmarks
{
    [MemoryDiagnoser]
    public class ReverseArray
    {
        private const int n = 10000;

        private const int SIZE = 32;
        private readonly byte[] src = new byte[SIZE];

        [Benchmark]
        public void ArrayReverse()
        {
            for (var i = 0; i < n; ++i)
            {
                for (var offset = 0; offset < SIZE - 1; offset += 4)
                {
                    Array.Reverse(src, offset, 4);
                }
            }
        }

        [Benchmark]
        public void SpanSliceReverse()
        {
            for (var i = 0; i < n; ++i)
            {
                for (var offset = 0; offset < SIZE - 1; offset += 4)
                {
                    src.AsSpan().Slice(offset, 4).Reverse();
                }
            }
        }
    }
}
