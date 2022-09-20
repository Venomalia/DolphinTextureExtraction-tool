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
            WriteLine($"{System.Diagnostics.Process.GetCurrentProcess().ProcessName} {IntPtr.Size * 8}bit v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}  {DateTime.Now.ToString()}");
            WriteLine("".PadLeft(64, '-'));
            Flush();
        }

        public void WriteFoot(TextureExtractor.ExtractorResult result)
        {
            WriteLine("".PadLeft(64, '-'));
            WriteLine($"~END  {DateTime.Now.ToString()}");
            WriteLine("".PadLeft(64, '-'));
            WriteLine($"Extracted textures: {result.Extracted}");
            WriteLine($"Unsupported files: {result.Unsupported}");
            if (result.Unsupported != 0) WriteLine($"Unsupported files Typs: {string.Join(", ", result.UnsupportedFormatType.Select(x => (x.GetFullDescription())))}");
            WriteLine($"Unknown files: {result.Unknown}");
            if (result.UnknownFormatType.Count != 0) WriteLine($"Unknown files Typs: {string.Join(", ", result.UnknownFormatType.Select(x => (x.Header == null || x.Header.MagicASKI.Length < 2) ? x.Extension : $"{x.Extension} \"{x.Header.MagicASKI}\""))}");
            WriteLine($"Extraction rate: ~ {result.GetExtractionSize()}");
            WriteLine($"Scan time: {Math.Round(result.TotalTime.TotalSeconds, 3)}s");
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
                WriteLine($"{action}: {message}");
                WriteLine($" {value}");
                Flush();
            }
        }
    }
}
