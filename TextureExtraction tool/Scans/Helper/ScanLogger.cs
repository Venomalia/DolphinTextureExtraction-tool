using AuroraLib.Common;
using DolphinTextureExtraction.Scans.Options;
using DolphinTextureExtraction.Scans.Results;
using System.Text;

namespace DolphinTextureExtraction.Scans.Helper
{
    public enum FileAction
    {
        Unknown = -2, Unsupported = -1, Extract, Compress
    }

    internal class ScanLogger : LogBase
    {
        private const string separator = ": ";

        public ScanLogger(ScanOptions Option) : this(new MemoryPoolStream(4096), Option)
        { }

        public ScanLogger(Stream stream, ScanOptions Option) : base(stream)
            => WriteHeader(Option);

        public ScanLogger(string directory, string mode, ScanOptions Option) : base(GenerateFullPath(directory, mode))
            => WriteHeader(Option);

        private static string GenerateFullPath(string directory, string mode)
        {
            string basename = $"DTE-{mode} {DateTime.Now:yy-MM-dd_HH-mm-ss}";

            string FullPath = Path.ChangeExtension(Path.Combine(directory, basename), "log");
            if (File.Exists(FullPath))
            {
                int i = 2;
                while (File.Exists(FullPath = Path.ChangeExtension(Path.Combine(directory, basename + "_" + i), "log")))
                {
                    i++;
                }
            }
            return FullPath;
        }

        private void WriteHeader(ScanOptions Option)
        {
            WriteDivider(64);
            WriteLine($"{System.Diagnostics.Process.GetCurrentProcess().ProcessName} {IntPtr.Size * 8}bit v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}  {DateTime.Now}");
            WriteLine(Option.ToString());
            WriteDivider(64);
            Flush();
        }

        public void WriteFoot(ScanResults result)
        {
            WriteDivider(64);
            WriteLine($"~END  {DateTime.Now}");
            WriteDivider(64);
            WriteLine(result.ToString().LineBreak(120, 20));
            WriteDivider(64);
            Flush();
        }

        public void WriteNotification(NotificationType type, string message)
        {
            StringBuilder sb = new();
            AddThreadIndex(sb);
            sb.Append(type.ToString());
            sb.Append(separator);
            sb.AppendLine(message);
            lock (Lock)
            {
                Write(sb);
            }
        }

        public void WriteEX(Exception ex, in string strMessage = "")
        {
            const string EX = "Exception: ";
            StringBuilder sb = new();
            AddThreadIndex(sb);
            sb.Append(EX);
            sb.Append(strMessage);
            sb.Append(' ');
            sb.AppendLine(ex?.Message);
            Console.Error.WriteLine(sb);
            sb.Append(ex?.Source);
            sb.Append(separator);
            sb.AppendLine(ex?.StackTrace);
            lock (Lock)
            {
                WriteDivider(64);
                Write(sb);
                WriteDivider(64);
                Flush();
            }
        }

        public void Write(FileAction action, in string message, in string value)
        {
            StringBuilder sb = new();
            AddThreadIndex(sb);
            sb.Append(action.ToString());
            sb.Append(separator);
            sb.AppendLine(message);
            sb.Append('•');
            sb.Append(' ');
            sb.AppendLine(value);
            lock (Lock)
            {
                Write(sb);
            }
        }

        public void WriteDivider(int width)
        {
            Span<char> dividerSpan = stackalloc char[width];
            dividerSpan.Fill('-');
            WriteLine(dividerSpan);
        }

        private void AddThreadIndex(StringBuilder sb)
        {
            sb.Append('[');
            sb.Append(ThreadIndex.ToString("D2"));
            sb.Append(']');
            sb.Append(' ');
        }
    }
}
