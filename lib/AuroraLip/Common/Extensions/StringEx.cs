using System;
using System.IO;
using System.Text;
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
        /// Introduces a linebreak if the current console's width is passed
        /// </summary>
        public static string LineBreak(this string text) => text.LineBreak(Console.WindowWidth - 1, 0);

        /// <summary>
        /// Introduces a linebreak if the current console's width is passed, prepending a given <paramref name="offset"/> of space characters
        /// </summary>
        /// <param name="max">The maximum number of characters to a line.</param>
        public static string LineBreak(this string text, int offset) => text.LineBreak(Console.WindowWidth - 1, offset);

        /// <summary>
        /// Introduces a linebreak if the specified <paramref name="width"/> is passed, prepending a given <paramref name="offset"/> of space characters
        /// </summary>
        /// <param name="width">The maximum number of characters to a line.</param>
        /// <param name="offset">How many spaces to prepend to any created newlines.</param>
        public static string LineBreak(this string text, int width, int offset)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            string insert = Environment.NewLine.PadRight(offset, ' ');
            StringBuilder output = new StringBuilder();
            StringReader reader = new StringReader(text);
            while(true)
            {
                string read = reader.ReadLine();
                if (read == null) break;
                int index = 0;
                while (index + width < read.Length)
                {
                    ReadOnlySpan<char> line = read.AsSpan(index, width);
                    index += line.LastIndexOf(' ');
                    read = read.Insert(index, insert);
                }
                output.AppendLine(read);
            }
            output.Length -= Environment.NewLine.Length;
            return output.ToString();
        }

        /// <summary>
        /// Creates a divider line equal to the current console's width
        /// </summary>
        public static string Divider() => Divider(Console.WindowWidth - 1, '-');

        /// <summary>
        /// Creates a divider line to the specified <paramref name="width"/>
        /// </summary>
        /// <param name="width">How long the divider should be</param>
        public static string Divider(int width) => Divider(width, '-');

        /// <summary>
        /// Creates a divider line equal to the current console's width using the specified <paramref name="divider"/>
        /// </summary>
        /// <param name="divider">What character should make up the divider</param>
        public static string Divider(char divider) => Divider(Console.WindowWidth - 1, divider);

        /// <summary>
        /// Creates a divider line using the specified <paramref name="width"/> &amp; <paramref name="divider"/>
        /// </summary>
        /// <param name="width">How long the divider should be</param>
        /// <param name="divider">What character should make up the divider</param>
        public static string Divider(int width, char divider) => string.Empty.PadRight(width, divider);

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
