using BenchmarkDotNet.Attributes;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Benchmark.Benchmarks
{
    [MemoryDiagnoser]
    public class ReadStruct
    {
        private const int n = 5000;

        private const int SIZE = 16;
        private readonly byte[] src = new byte[SIZE];

        [Benchmark]
        public void ReadMemoryMarshal()
        {
            using var stream = new MemoryStream(src);
            for (var i = 0; i < n; ++i)
            {
                _ = MarshalRead<int>(stream);
                _ = MarshalRead<int>(stream);
                _ = MarshalRead<int>(stream);
                _ = MarshalRead<int>(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MarshalRead<T>(Stream stream) where T : struct
        {
            Span<byte> Buffer = stackalloc byte[Unsafe.SizeOf<T>()];
            stream.Read(Buffer);
            return MemoryMarshal.Read<T>(Buffer);
        }

        [Benchmark]
        public void ReaderUnsafe()
        {
            using var stream = new MemoryStream(src);
            for (var i = 0; i < n; ++i)
            {
                _ = UnsafeRead<int>(stream);
                _ = UnsafeRead<int>(stream);
                _ = UnsafeRead<int>(stream);
                _ = UnsafeRead<int>(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T UnsafeRead<T>(Stream stream) where T : unmanaged
        {
            T value;
            var sValue = new Span<byte>(&value, sizeof(T));
            stream.Read(sValue);
            return value;
        }

        [Benchmark]
        public void ReadMemoryMarshalStruct()
        {
            using var stream = new MemoryStream(src);
            for (var i = 0; i < n; ++i)
            {
                _ = MarshalRead<TestStruct>(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }
        }

        [Benchmark]
        public void ReaderUnsafeStruct()
        {
            using var stream = new MemoryStream(src);
            for (var i = 0; i < n; ++i)
            {
                _ = UnsafeRead<TestStruct>(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }
        }

        private struct TestStruct
        {
            public int Test1;
            public int Test2;
            public int Test3;
            public int Test4;
        }

    }
}
