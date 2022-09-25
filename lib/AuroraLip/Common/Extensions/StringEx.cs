using System;
using System.IO;

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
    }
}
