using AuroraLip.Common;
using System;
using System.IO;
using System.Linq;

namespace DolphinTextureExtraction_tool
{
    public enum FileAction
    {
        Unknown = -2, Unsupported = -1, Extract
    }

    internal class ScanLogger : LogBase
    {
        private static readonly object LockFile = new object();

        public ScanLogger(string directory) : base(GenerateFullPath(directory))
        {
            WriteHeader();
        }

        private static string GenerateFullPath(string directory)
        {
            string basename = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            string FullPath;
            if (File.Exists(FullPath = Path.ChangeExtension(Path.Combine(directory, basename), "log")))
            {
                int i = 2;
                while (File.Exists(FullPath = Path.ChangeExtension(Path.Combine(directory, basename + "_" + i), "log")))
                {
                    i++;
                }
            }
            return FullPath;
        }

        private void WriteHeader()
        {
            WriteLine("".PadLeft(64, '-'));
            WriteLine($"{System.Diagnostics.Process.GetCurrentProcess().ProcessName} {IntPtr.Size * 8}bit v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}  {DateTime.Now}");
            WriteLine("".PadLeft(64, '-'));
            Flush();
        }

        public void WriteFoot(TextureExtractor.ExtractorResult result)
        {
            WriteLine("".PadLeft(64, '-'));
            WriteLine($"~END  {DateTime.Now}");
            WriteLine("".PadLeft(64, '-'));
            WriteLine(result.ToString());
            WriteLine("".PadLeft(64, '-'));
            Flush();
        }


        public void WriteNotification(NotificationType type, string message)
            => WriteLine($"{type}: {message}");

        public void WriteEX(Exception ex, in string strMessage = "")
        {
            lock (LockFile)
            {
                WriteLine("".PadLeft(64, '-'));
                WriteLine($"Exception: {strMessage} {ex?.Message}");
                WriteLine($"{ex?.Source}:{ex?.StackTrace}");
                WriteLine("".PadLeft(64, '-'));
                Flush();
            }
            Console.Error.WriteLine($"Exception: {strMessage} {ex?.Message}");
        }

        public void Write(FileAction action, in string message, in string value)
        {
            lock (LockFile)
            {
                WriteLine($"[{TextureExtractor.ThreadIndex:D2}] {action}: {message}");
                WriteLine($"• {value}");
                Flush();
            }
        }
    }
}
