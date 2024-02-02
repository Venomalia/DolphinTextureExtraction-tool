using System.Text;

namespace AuroraLib.Common
{
    /// <summary>
    /// Extra <see cref="string"/> functions
    /// </summary>
    public static class StringEx
    {
        private static string exePath = null;
        public static string ExePath => exePath ??= Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) ?? string.Empty;

        public static string SimpleDate => DateTime.Now.ToString("yy-MM-dd_HH-mm-ss");

        /// <summary>
        /// Introduces a linebreak if the current console's width is passed, prepending a given <paramref name="offset"/> of space characters
        /// </summary>
        /// <param name="max">The maximum number of characters to a line.</param>
        public static string LineBreak(this string text, int offset = 0) => text.LineBreak(Console.WindowWidth - 1, offset, Console.CursorLeft);

        /// <summary>
        /// Introduces a linebreak if the specified <paramref name="width"/> is passed, prepending a given <paramref name="offset"/> of space characters
        /// </summary>
        /// <param name="width">The maximum number of characters to a line.</param>
        /// <param name="offset">How many spaces to prepend to any created newlines.</param>
        /// <param name="columnOffset">column offset of the <paramref name="text"/>.</param>
        public static string LineBreak(this string text, int width, int offset, int columnOffset = 0)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            int cOffset = Console.CursorLeft;
            string insert = Environment.NewLine.PadRight(offset, ' ');
            StringBuilder output = new();

            int index = 0;
            while (index < text.Length)
            {
                int length = text[index..].IndexOf(Environment.NewLine);
                if ((length == -1))
                    length = text.Length;
                length -= -index;

                LineInsert(output, text.AsSpan(index, length), insert, width, columnOffset);
                index = length + Environment.NewLine.Length;
            }

            output.Length -= Environment.NewLine.Length;
            return output.ToString();
        }

        private static void LineInsert(StringBuilder output, ReadOnlySpan<char> text, ReadOnlySpan<char> insert, int width, int offset = 0)
        {
            int index = 0, length = width - offset;
            while (index + length < text.Length)
            {
                ReadOnlySpan<char> line = text.Slice(index, length);
                int lineEnd = line.LastIndexOf(' ');
                if (lineEnd == -1)
                    lineEnd = length;

                output.Append(line[..lineEnd]);
                output.Append(insert);

                if (index == 0)
                    length = width - insert.Length;
                index += lineEnd;
            }
            output.Append(text[index..]);
            output.AppendLine();
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
        public static string Divider(int width, char divider)
            => new(divider, width);
    }
}
