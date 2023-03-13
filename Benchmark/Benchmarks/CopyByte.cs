using BenchmarkDotNet.Attributes;
using System.Runtime.InteropServices;

namespace Benchmark.Benchmarks
{
    [MemoryDiagnoser]
    public class CopyByte
    {
        private const int n = 10000;

        private const int SIZE = 32;
        private readonly byte[] src = new byte[SIZE];
        private readonly byte[] dst = new byte[SIZE];

        [Benchmark]
        public void ArrayCopy()
        {
            for (var i = 0; i < n; ++i)
            {
                Array.Copy(src, dst, SIZE);
            }
        }

        [Benchmark]
        public void SpanCopyTo()
        {
            for (var i = 0; i < n; ++i)
            {
                src.AsSpan().CopyTo(dst.AsSpan());
            }
        }

        [Benchmark]
        public void BufferBlockCopy()
        {
            for (var i = 0; i < n; ++i)
            {
                Buffer.BlockCopy(src, 0, dst, 0, SIZE);
            }
        }

        [Benchmark]
        public void MarshalCopy()
        {
            for (var i = 0; i < n; ++i)
            {
                GCHandle handle = GCHandle.Alloc(dst, GCHandleType.Pinned);
                try
                {
                    IntPtr rawDataPtr = handle.AddrOfPinnedObject();
                    Marshal.Copy(src, 0, rawDataPtr, SIZE);
                }
                finally
                {
                    handle.Free();
                }
            }
        }
    }
}
