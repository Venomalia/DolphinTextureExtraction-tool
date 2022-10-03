using System;
using System.IO;
using System.Runtime.InteropServices;

namespace AuroraLip.Common
{
    /// <summary>
    /// Extra <see cref="string"/> functions
    /// </summary>
    public static class StringEx
    {
        static string exePath = null;
        public static string ExePath => exePath = exePath ?? Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) ?? string.Empty;

        public static string SimpleDate => DateTime.Now.ToString("yy-MM-dd_HH-mm-ss");

        /// <summary>
        /// Introduces a linebreak if a certain threshold is passed
        /// </summary>
        /// <param name="max">The maximum number of characters to a line.</param>
        /// <param name="insert">What to prepend to any created newlines.</param>
        /// <returns></returns>
        public static string LineBreak(this string str, int max, in string insert)
        {
            int Index = 0;
            while (Index + max < str.Length)
            {
                ReadOnlySpan<char> line = str.AsSpan(Index, max);
                Index += line.LastIndexOf(' ');
                str = str.Insert(Index, insert);
            }
            return str;
        }

        public static string Combine(in ReadOnlySpan<char> path, in ReadOnlySpan<char> path2)
        {
            var str = new string('\0', path.Length + path2.Length);
            var strSpan = MemoryMarshal.AsMemory(str.AsMemory()).Span;
            path.CopyTo(strSpan);
            strSpan = strSpan.Slice(path.Length);
            path2.CopyTo(strSpan);
            return str;
        }

        public static string Combine(in ReadOnlySpan<char> path, in ReadOnlySpan<char> path2, in ReadOnlySpan<char> path3)
        {
            var str = new string('\0', path.Length + path2.Length + path3.Length);
            var strSpan = MemoryMarshal.AsMemory(str.AsMemory()).Span;
            path.CopyTo(strSpan);
            strSpan = strSpan.Slice(path.Length);
            path2.CopyTo(strSpan);
            strSpan = strSpan.Slice(path2.Length);
            path3.CopyTo(strSpan);
            return str;
        }

    }
}
