using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DolphinTextureExtraction_tool
{
    partial class Program
    {
        static string InputDirectory;

        static string OutputDirectory;

        static TextureExtractor.ExtractorOptions options = new TextureExtractor.ExtractorOptions();

        static void Main(string[] args)
        {
#if DEBUG
            Console.Title = $"{System.Diagnostics.Process.GetCurrentProcess().ProcessName} v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} *DEBUG";
#else
            Console.Title = $"{System.Diagnostics.Process.GetCurrentProcess().ProcessName} v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
#endif
            if (args.Length == 0)
            {
                PrintHeader();
                while (true)
                {
                    //Input Path
                    Console.WriteLine();
                    Console.WriteLine("Input Path:");
                    Console.WriteLine("Specify a directory from which the textures should be extracted.");
                    do
                    {
                        InputDirectory = Console.ReadLine().Trim('"');

                        if (!Directory.Exists(InputDirectory))
                        {
                            ConsoleEx.WriteLineColoured("The directory could not be found!", ConsoleColor.Red);
                        }
                    } while (!Directory.Exists(InputDirectory));

                    //Output Path
                    Console.WriteLine();
                    Console.WriteLine("Output Path:");
                    Console.WriteLine("Specify a directory where the textures should be saved.");
                    Console.WriteLine($"Or use default \t\"{GetGenOutputPath(InputDirectory)}\"");
                    do
                    {
                        OutputDirectory = Console.ReadLine().Trim('"');
                        if (OutputDirectory == "")
                        {
                            OutputDirectory = GetGenOutputPath(InputDirectory);
                        }
                        if (!PathIsValid(OutputDirectory))
                        {
                            ConsoleEx.WriteLineColoured("Path is invalid!", ConsoleColor.Red);
                        }
                        if (OutputDirectory == InputDirectory)
                        {
                            ConsoleEx.WriteLineColoured("Output Directory and Input Directory cannot be the same!", ConsoleColor.Red);
                        }

                    } while (!PathIsValid(OutputDirectory) || OutputDirectory == InputDirectory);

                    //Options
                    Console.WriteLine();
                    PrintOptions(options);

                    Console.WriteLine();
                    Console.WriteLine("Adjust settings? Yes or (No)");
                    if (ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(false), "Yes", "\tNo", ConsoleColor.Green, ConsoleColor.Red))
                    {
                        Console.WriteLine($"Extract mipmaps. \t(True) or False");
                        options.Mips = ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(true, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);
                        Console.WriteLine($"Extracts all raw images that are found. \tTrue or (False)");
                        options.Raw = ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(false, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);
                        Console.WriteLine($"Tries to extract textures from unknown files, may cause errors. \tTrue or (False)");
                        options.Force = ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(false, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);
                        Console.Write($"Sorts textures and removes unnecessary folders. \t(True) or False");
                        options.Cleanup = ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(true, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);
                        Console.WriteLine($"High performance mode.(Multithreading) \t(True) or False");
                        if (ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(true, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red))
                        {
                            options.Parallel.MaxDegreeOfParallelism = 4;
                        }
                        else
                        {
                            options.Parallel.MaxDegreeOfParallelism = 1;
                        }
                    }

                    //Inputs correct?
                    Console.WriteLine();
                    Console.WriteLine($"Input Path: \"{InputDirectory}\"");
                    Console.WriteLine($"Output Path: \"{OutputDirectory}\"");
                    PrintOptions(options);

                    Console.WriteLine();
                    Console.WriteLine("Are the settings correct? \t(Yes) or No");
                    if (!ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(true), "Yes", "\tNo", ConsoleColor.Green, ConsoleColor.Red)) continue;

                    //Start
                    Console.WriteLine($"Search and extract textures from {InputDirectory}");
                    Console.WriteLine("This may take a few seconds...");
                    var result = TextureExtractor.StartScan(InputDirectory, OutputDirectory, options);

                    Console.WriteLine();
                    PrintResult(result);
                }
            }
            else
            {
                int p;
                switch (args[0].ToLower())
                {
                    case "formats":
                    case "format":
                    case "f":
                        #region formats
                        ConsoleEx.WriteLineColoured("".PadLeft(108, '-'), ConsoleColor.Blue);
                        Console.WriteLine($"Known formats: {FormatDictionary.Master.Length}.");
                        ConsoleEx.WriteLineColoured("".PadLeft(108, '-'), ConsoleColor.Blue);
                        foreach (var item in FormatDictionary.Master)
                            Console.WriteLine($"{item.GetFullDescription()} Typ:{item.Typ}");
                        ConsoleEx.WriteLineColoured("".PadLeft(108, '-'), ConsoleColor.Blue);
                        #endregion
                        break;
                    case "unpack":
                    case "u":
                        #region unpack
                        p = GetPahts(args);
                        if (p <= 0)
                            goto default;

                        Unpack.StartScan(InputDirectory, OutputDirectory, options);
                        Console.WriteLine();
                        Console.WriteLine("completed.");
                        //PrintResult(result);
                        #endregion
                        break;
                    case "extract":
                    case "e":
                        #region extract
                        p = GetPahts(args);
                        if (p <= 0)
                            goto default;

                        options = new TextureExtractor.ExtractorOptions() { Mips = false, Raw = false, Force = false };

                        if (args.Length > p)
                        {
                            for (int i = p; i < args.Length; i++)
                            {
                                switch (args[i].ToLower())
                                {
                                    case "-mip":
                                    case "-m":
                                        options.Mips = true;
                                        break;
                                    case "-raw":
                                    case "-r":
                                        options.Raw = true;
                                        break;
                                    case "-force":
                                    case "-f":
                                        options.Force = true;
                                        break;
                                }
                            }
                        }

                        var result = TextureExtractor.StartScan(InputDirectory, OutputDirectory, options);

                        Console.WriteLine();
                        PrintResult(result);

                        #endregion
                        break;
                    case "help":
                    case "h":
                        PrintHelp();
                        break;
                    default:
                        if (Directory.Exists(args[0]))
                        {
                            InputDirectory = args[0];
                            OutputDirectory = GetGenOutputPath(InputDirectory);
                            TextureExtractor.StartScan(InputDirectory, OutputDirectory);
                            return;
                        }
                        Console.WriteLine("Wrong syntax. use h for help");
                        break;
                }
            }
        }

        private static int GetPahts(string[] args)
        {
            if (args.Length < 2)
                return -1;

            InputDirectory = args[1];
            if (args.Length >= 3)
                OutputDirectory = args[2];
            else
                OutputDirectory = GetGenOutputPath(InputDirectory);

            if (!Directory.Exists(InputDirectory))
            {
                Console.WriteLine("The directory could not be found!");
                Console.WriteLine(InputDirectory);
                return -1;
            }
            if (!PathIsValid(OutputDirectory))
            {
                OutputDirectory = GetGenOutputPath(InputDirectory);
                return 2;
            }
            return 3;
        }

        private static string GetGenOutputPath(string Input)
        {
            DirectoryInfo InputInfo = new DirectoryInfo(Input);
            string Output = Path.Combine(InputInfo.Parent.FullName, '~' + InputInfo.Name);
            int i = 2;
            while (Directory.Exists(Output))
            {
                Output = Path.Combine(InputInfo.Parent.FullName, $"~{InputInfo.Name}_{i}");
                i++;
            }
            return Output;
        }

        static bool PathIsValid(string path)
        {
            try
            {
                return Path.IsPathRooted(path);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void PrintHelp()
        {
            PrintHeader();
            Console.WriteLine();
            Console.WriteLine("help\t| h\t: Print this list.");
            Console.WriteLine();
            Console.WriteLine("extract\t| e\t: Extracts all textures.");
            Console.WriteLine("\tSyntax:\textract \"Input\" \"Output\" options");
            Console.WriteLine("\tOption:\t -m -mip\tExtract mipmaps.");
            Console.WriteLine("\tOption:\t -r -raw\tExtracts raw images.");
            Console.WriteLine("\tOption:\t -f -force\tTries to extract unknown files, may cause errors.");
        }

        static void PrintHeader()
        {
            ConsoleEx.WriteLineColoured("".PadLeft(108, '-'), ConsoleColor.Blue);
            Console.ForegroundColor = ConsoleColor.Cyan;
#if DEBUG
            Console.WriteLine($"{System.Diagnostics.Process.GetCurrentProcess().ProcessName} v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}\t\t{DateTime.Now.ToString()}\t\t*DEBUG");
#else
            Console.WriteLine($"{System.Diagnostics.Process.GetCurrentProcess().ProcessName} v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}\t\t{DateTime.Now.ToString()}");
#endif
            PrintFormats();
            ConsoleEx.WriteLineColoured("".PadLeft(108, '-'), ConsoleColor.Blue);
            ConsoleEx.WriteColoured("INFO:", ConsoleColor.Red);
            Console.WriteLine(" currently no ROM images are supported, Please unpack them with dolphin into a folder.");
            Console.WriteLine("right click on a game -> Properties -> Filesystem -> right click on \"Disc - [Game ID]\" -> Extract Files...");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void PrintFormats()
        {
            List<string> formats = new List<string>(AuroraLip.Common.Reflection.FileAccess.GetReadable().Select(x => x.Name));
            formats.Add("CPK");
            FormatDictionary.GetValue("CPK ").Class = typeof(LibCPK.CPK);
            formats.Add("AFS");
            FormatDictionary.GetValue("AFS").Class = typeof(AFSLib.AFS);
            formats.Add("BDL4");
            FormatDictionary.GetValue("J3D2bdl4").Class = typeof(Hack.io.BMD.BDL);
            formats.Add("BMD3");
            FormatDictionary.GetValue("J3D2bmd3").Class = typeof(Hack.io.BMD.BMD);
            formats.Add("TEX1");
            FormatDictionary.GetValue("TEX1").Class = typeof(Hack.io.BMD.BMD.TEX1);
            formats.Sort();
            Console.WriteLine(LineBreak($"Supported formats: {string.Join(", ", formats)}.", 108, "\n                  "));
        }

        static string LineBreak(string str, int max, in string insert)
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

        static void PrintOptions(TextureExtractor.ExtractorOptions options)
        {
            Console.Write($"Extracts Mipmaps: ");
            ConsoleEx.WriteBoolPrint(options.Mips, ConsoleColor.Green, ConsoleColor.Red);
            Console.Write($"Extracts Raw Image fiels: ");
            ConsoleEx.WriteBoolPrint(options.Raw, ConsoleColor.Green, ConsoleColor.Red);
            Console.Write($"Extract textures from unknown files: ");
            ConsoleEx.WriteBoolPrint(options.Force, ConsoleColor.Green, ConsoleColor.Red);
            Console.Write($"Sorts textures and removes unnecessary folders: ");
            ConsoleEx.WriteBoolPrint(options.Cleanup, ConsoleColor.Green, ConsoleColor.Red);
            Console.Write($"High performance mode (Multithreading): ");
            ConsoleEx.WriteBoolPrint(options.Parallel.MaxDegreeOfParallelism > 1, ConsoleColor.Green, ConsoleColor.Red);
        }

        static void PrintResult(TextureExtractor.Result result)
        {
            ConsoleEx.WriteLineColoured("".PadLeft(108, '-'), ConsoleColor.Blue);
            Console.WriteLine($"Extracted textures: {result.Extracted}");
            Console.WriteLine($"Unsupported files: {result.Unsupported}");
            if (result.Unsupported != 0) Console.WriteLine($"Unsupported files Typs: {string.Join(", ", result.UnsupportedFormatType.Select(x => (x.GetFullDescription())))}");
            Console.WriteLine($"Unknown files: {result.Unknown}");
            if (result.UnknownFormatType.Count != 0) Console.WriteLine(LineBreak($"Unknown files Typs: {string.Join(", ", result.UnknownFormatType.Select(x => (x.Header == null || x.Header.MagicASKI.Length < 2) ? x.Extension : $"{x.Extension} \"{x.Header.MagicASKI}\""))}", 108, "\n                   "));
            Console.WriteLine($"Extraction rate: ~ {result.GetExtractionSize()}");
            Console.WriteLine($"Scan time: {Math.Round(result.TotalTime.TotalSeconds, 3)}s");
            Console.WriteLine($"Log saved: \"{result.LogFullPath}\"");
            ConsoleEx.WriteLineColoured("".PadLeft(108, '-'), ConsoleColor.Blue);
        }
    }
}
