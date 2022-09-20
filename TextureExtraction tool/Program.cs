using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static AuroraLip.Texture.J3D.JUtility;
using static DolphinTextureExtraction_tool.ScanBase;

namespace DolphinTextureExtraction_tool
{
    partial class Program
    {
        static string InputDirectory;

        static string OutputDirectory;

        static TextureExtractor.ExtractorOptions options;

        static Modes Mode;
        private enum Modes : byte
        {
            Extract = 1,
            Unpacks = 2,
        }

#if DEBUG
        private static readonly string Title = $"{System.Diagnostics.Process.GetCurrentProcess().ProcessName} v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} {IntPtr.Size * 8}bit *DEBUG";
#else
        private static readonly string Title = $"{System.Diagnostics.Process.GetCurrentProcess().ProcessName} v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} {IntPtr.Size * 8}bit";
#endif
        static Program()
        {
            //link external classes
            FormatDictionary.GetValue("CPK ").Class = typeof(LibCPK.CPK);
            FormatDictionary.GetValue("AFS").Class = typeof(AFSLib.AFS);
            FormatDictionary.GetValue("J3D2bdl4").Class = typeof(Hack.io.BMD.BDL);
            FormatDictionary.GetValue("J3D2bmd3").Class = typeof(Hack.io.BMD.BMD);
            FormatDictionary.GetValue("TEX1").Class = typeof(Hack.io.BMD.BMD.TEX1);
            GC.Collect();

            //are we able to change the Title?
            try
            {
                Console.Title = Title;
            }
            catch (Exception) { }

            options = new TextureExtractor.ExtractorOptions();

            //Do we have restricted access to the Console.Cursor?
            try
            {
                Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop);
                options.ProgressAction = ProgressUpdate;
            }
            catch (Exception)
            {
                options.ProgressAction = ProgressTitleUpdate;
            }
        }

        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                PrintHeader();

                Console.WriteLine();
                Console.WriteLine("Select Extraction Mode");
                Console.WriteLine("1.\t Extract textures.");
                Console.WriteLine("2.\t Unpacks all files.");
                Console.WriteLine();
                do
                {
                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.D1:
                        case ConsoleKey.NumPad1:
                        case ConsoleKey.E:
                        case ConsoleKey.Enter:
                            Mode = Modes.Extract;
                            Console.CursorLeft = 0;
                            Console.WriteLine("Mode: Extract textures.");
                            break;
                        case ConsoleKey.D2:
                        case ConsoleKey.NumPad2:
                        case ConsoleKey.U:
                            Mode = Modes.Unpacks;
                            Console.CursorLeft = 0;
                            Console.WriteLine("Mode: Unpacks all files.");
                            break;
                        default:
                            continue;
                    }
                    break;
                } while (true);

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



                    switch (Mode)
                    {
                        case Modes.Extract:
                            //Options
                            Console.WriteLine();
                            PrintOptions(options);

                            Console.WriteLine();
                            Console.WriteLine("Adjust settings? Yes or (No)");
                            if (ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(false), "Yes", "\tNo", ConsoleColor.Green, ConsoleColor.Red))
                            {
                                Console.WriteLine($"Extract mipmaps. \t(True) or False");
                                options.Mips = ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(true, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);
                                Console.WriteLine($"Extracts raw image files. \tTrue or (False)");
                                options.Raw = ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(false, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);
                                Console.WriteLine($"Tries to extract textures from unknown file formats, may cause errors. \tTrue or (False)");
                                options.Force = ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(false, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);
                                Console.Write($"Tries to Imitate dolphin mipmap detection. \t(True) or False");
                                options.DolphinMipDetection = ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(true, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);
                                Console.Write($"Clean up folder structure. \t(True) or False");
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
                            break;
                        case Modes.Unpacks:
                            break;
                    }

                    //Inputs correct?
                    Console.WriteLine();
                    Console.WriteLine($"Input Path: \"{InputDirectory}\"");
                    Console.WriteLine($"Output Path: \"{OutputDirectory}\"");
                    if (Mode == Modes.Extract)
                        PrintOptions(options);

                    Console.WriteLine();
                    Console.WriteLine("Are the settings correct? \t(Yes) or No");
                    if (!ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(true), "Yes", "\tNo", ConsoleColor.Green, ConsoleColor.Red)) continue;

                    //Start
                    Console.CursorVisible = false;
                    switch (Mode)
                    {
                        case Modes.Extract:
                            Console.WriteLine($"Search and extract textures from {InputDirectory}");
                            Console.WriteLine("This may take a few seconds...");
                            Console.WriteLine();
                            var result = TextureExtractor.StartScan(InputDirectory, OutputDirectory, options);
                            Console.WriteLine();
                            PrintResult(result);
                            break;
                        case Modes.Unpacks:
                            Console.WriteLine($"Unpacks all files from {InputDirectory}");
                            Console.WriteLine("This may take a few seconds...");
                            Console.WriteLine();
                            Unpack.StartScan(InputDirectory, OutputDirectory, options);
                            Console.WriteLine();
                            Console.WriteLine("Done.");
                            break;
                    }
                    Console.CursorVisible = true;
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
                    case "cut":
                    case "c":
                        #region cut
                        p = GetPahts(args);
                        if (p <= 0)
                            goto default;

                        List<byte[]> pattern = null;
                        if (args.Length > p)
                        {
                            pattern = new List<byte[]>();
                            while (args.Length > p)
                            {
                                pattern.Add(args[p++].ToByte());
                            }
                        }

                        Cutter.StartScan(InputDirectory, OutputDirectory, pattern, options);
                        Console.WriteLine();
                        Console.WriteLine("completed.");
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

                        options = new TextureExtractor.ExtractorOptions() { Mips = false, Raw = false, Force = false, ProgressAction = options.ProgressAction };

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
                                    case "-cleanup":
                                    case "-c":
                                        options.Cleanup = true;
                                        break;
                                    case "-cleanup:none":
                                    case "-c:none":
                                    case "-c:n":
                                        options.Cleanup = false;
                                        break;
                                    case "-progress:none":
                                    case "-p:none":
                                    case "-p:n":
                                        options.ProgressAction = ProgressTitleUpdate;
                                        options.TextureAction = null;
                                        break;
                                    case "-progress:bar":
                                    case "-p:bar":
                                    case "-p:b":
                                        options.ProgressAction = ProgressUpdate;
                                        options.TextureAction = null;
                                        break;
                                    case "-progress:list":
                                    case "-p:list":
                                    case "-p:l":
                                        options.ProgressAction = null;
                                        options.TextureAction = TextureUpdate;
                                        break;
                                    case "-dolphinmipdetection":
                                    case "-dm":
                                        options.Cleanup = false;
                                        break;
                                    case "-tasks":
                                    case "-t":
                                        i++;
                                        options.Parallel.MaxDegreeOfParallelism = Int32.Parse(args[i]);
                                        break;
                                }
                            }
                        }
                        var result = TextureExtractor.StartScan(InputDirectory, OutputDirectory, options);

                        Console.WriteLine();

                        if (options.TextureAction == null)
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
                        }
                        Console.Error.WriteLine("Wrong syntax.");
                        Console.WriteLine("use h for help");
                        Environment.Exit(-2);
                        break;
                }
                Environment.Exit(0);
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
            Console.WriteLine("formats\t| f\t: Displays all known formats.");
            Console.WriteLine("\tSyntax:\tformats");
            Console.WriteLine();
            Console.WriteLine("extract\t| e\t: Extracts all textures.");
            Console.WriteLine("\tSyntax:\textract \"Input*\" \"Output*\" options");
            Console.WriteLine("\tOption:\t -m -mip\tExtract mipmaps.");
            Console.WriteLine("\tOption:\t -r -raw\tExtracts raw images.");
            Console.WriteLine("\tOption:\t -f -force\tTries to extract unknown files, may cause errors.");
            Console.WriteLine("\tOption:\t -c -cleanup\tuses the default folder cleanup.");
            Console.WriteLine("\tOption:\t -c:n -cleanup:none\tretains the original folder structure.");
            Console.WriteLine("\tOption:\t -p:n -progress:none\toutputs only important events in the console.");
            Console.WriteLine("\tOption:\t -p:b -progress:bar\tshows the progress as a progress bar in the console.");
            Console.WriteLine("\tOption:\t -p:l -progress:list\toutputs the extracted textures as a list in the console.");
            Console.WriteLine("\tOption:\t -dmd -dolphinmipdetection\tTries to imitate dolphin mipmap detection.");
            Console.WriteLine("\tOption:\t -t -tasks \"i\"\tsets the maximum number of concurrent tasks.");
            Console.WriteLine($"\t\ti:\t integer that represents the maximum degree of parallelism. default:{options.Parallel.MaxDegreeOfParallelism}");
            Console.WriteLine();
            Console.WriteLine("unpack\t| u\t: Extracts all files.");
            Console.WriteLine("\tSyntax:\tunpack \"Input\" \"Output*\"");
            Console.WriteLine();
            Console.WriteLine("cut\t| c\t: Splits all files into individual parts.");
            Console.WriteLine("\tSyntax:\tcut \"Input\" \"Output*\" \"Pattern*\"");
            Console.WriteLine("\tPattern:\t a list of patterns divided with \" \"");
        }

        static void PrintHeader()
        {
            ConsoleEx.WriteLineColoured("".PadLeft(108, '-'), ConsoleColor.Blue);
            Console.ForegroundColor = ConsoleColor.Cyan;
#if DEBUG
            Console.WriteLine($"{System.Diagnostics.Process.GetCurrentProcess().ProcessName} v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} {IntPtr.Size * 8}bit\t\t{DateTime.Now.ToString()}\t\t*DEBUG");
#else
            Console.WriteLine($"{System.Diagnostics.Process.GetCurrentProcess().ProcessName} v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} {IntPtr.Size * 8}bit\t\t{DateTime.Now.ToString()}");
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
            formats.Add("AFS");
            formats.Add("BDL4");
            formats.Add("BMD3");
            formats.Add("TEX1");
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
            Console.Write($"Extracts raw image files: ");
            ConsoleEx.WriteBoolPrint(options.Raw, ConsoleColor.Green, ConsoleColor.Red);
            Console.Write($"Force extract textures from unknown formats: ");
            ConsoleEx.WriteBoolPrint(options.Force, ConsoleColor.Green, ConsoleColor.Red);
            Console.Write($"Imitate dolphin mipmap detection: ");
            ConsoleEx.WriteBoolPrint(options.DolphinMipDetection, ConsoleColor.Green, ConsoleColor.Red);
            Console.Write($"Clean up folder structure: ");
            ConsoleEx.WriteBoolPrint(options.Cleanup, ConsoleColor.Green, ConsoleColor.Red);
            Console.Write($"High performance mode (Multithreading): ");
            ConsoleEx.WriteBoolPrint(options.Parallel.MaxDegreeOfParallelism > 1, ConsoleColor.Green, ConsoleColor.Red);
        }

        static void PrintResult(TextureExtractor.ExtractorResult result)
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

        private static ConsoleBar ScanProgress;

        static void ProgressUpdate(ScanBase.Results result)
        {

            if (ScanProgress == null)
                ScanProgress = new ConsoleBar(result.WorkeLength, 40);

            int Cursor = Console.CursorTop;
            ScanProgress.CursorTop = Cursor;
            ScanProgress.Value = result.ProgressLength;
            ScanProgress.Print();

            double ProgressPercentage = ScanProgress.Value / ScanProgress.Max * 100;
            Console.Write($" {(int)ProgressPercentage}%");
            Console.WriteLine();

            if (result.Progress < result.Worke)
                Console.SetCursorPosition(0, Cursor);
            else
                ScanProgress = null;

            ProgressTitleUpdate(result);
        }

        static void ProgressTitleUpdate(ScanBase.Results result)
        {
            //are we able to change the Title?
            try
            {
                double ProgressPercentage = result.ProgressLength / result.WorkeLength * 100;
                if (result.Progress < result.Worke)
                    Console.Title = $"{Title} | {Math.Round(ProgressPercentage, 2)}%";
                else
                    Console.Title = Title;
            }
            catch (Exception)
            { }
        }

        static void TextureUpdate(JUTTexture.TexEntry texture,Results result , in string subdirectory)
        {
            double ProgressPercentage = result.ProgressLength / result.WorkeLength * 100;
            Console.WriteLine($"Prog:{Math.Round(ProgressPercentage, 2)}% Extract:{Path.Combine(subdirectory, texture.GetDolphinTextureHash()) + ".png"} mips:{texture.Count - 1} LODBias:{texture.LODBias} MinLOD:{texture.MinLOD} MaxLOD:{texture.MaxLOD}");
        }
    }
}
