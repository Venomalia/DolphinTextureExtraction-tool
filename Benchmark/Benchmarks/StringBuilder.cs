using AuroraLib.Common;
using BenchmarkDotNet.Attributes;
using System.Text;

namespace Benchmark.Benchmarks
{
    [MemoryDiagnoser]
    public class StringBuild
    {
        private const int n = 2500;
        private const int n2 = 5;

        private const string text = "ABC0XXX.exe";

        [Benchmark]
        public void Add()
        {
            for (var i = 0; i < n; ++i)
            {
                string s = text;
                for (var i2 = 0; i2 < n2; ++i2)
                {
                    s += text[..7];
                }
            }
        }


        [Benchmark]
        public void Concat()
        {
            for (var i = 0; i < n; ++i)
            {
                string s = text;
                for (var i2 = 0; i2 < n2; ++i2)
                {
                    s = string.Concat(s, text.AsSpan()[..7]);
                }
            }
        }

        [Benchmark]
        public void StringBuilder()
        {
            for (var i = 0; i < n; ++i)
            {
                StringBuilder sb = new(text);
                for (var i2 = 0; i2 < n2; ++i2)
                {
                    sb.Append(text.AsSpan()[..7]);
                }
                sb.ToString();
            }
        }

        [Benchmark]
        public void ValueStringBuilder()
        {
            for (var i = 0; i < n; ++i)
            {
                ValueStringBuilder sb = new();
                sb.Append(text.AsSpan());
                sb.Dispose();
                for (var i2 = 0; i2 < n2; ++i2)
                {
                    sb.Append(text.AsSpan()[..7]);
                }
                sb.AsSpan();
            }
        }

    }
}
