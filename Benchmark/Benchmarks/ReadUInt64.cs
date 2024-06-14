using AuroraLib.Core;
using BenchmarkDotNet.Attributes;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Benchmark.Benchmarks
{
    [MemoryDiagnoser]
    public class ReadUInt64
    {
        private const int n = 10000;

        private const ulong SIZE = 32;

        [Benchmark]
        public void BinaryPrimitives_Read()
        {
            for (var i = 0; i < n; ++i)
            {
                BinaryPrimitives.ReverseEndianness(SIZE);
            }
        }

        [Benchmark]
        public void MemoryMarshal_Read()
        {
            for (var i = 0; i < n; ++i)
            {
                BitConverterX.Swap(SIZE);
            }
        }

    }
}
