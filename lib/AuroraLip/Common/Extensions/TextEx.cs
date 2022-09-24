using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraLip.Common
{
    /// <summary>
    /// Extra text functions
    /// </summary>
    public static class TextEx
    {
        static string exePath = null;
        public static string ExePath => exePath = exePath ?? Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) ?? string.Empty;

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
