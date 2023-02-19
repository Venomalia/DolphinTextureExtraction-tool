using AuroraLip.Common;
using AuroraLip.Texture;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static DolphinTextureExtraction_tool.ScanBase;

namespace DolphinTextureExtraction_tool
{
    partial class Program
    {
        static string InputPath;

        static string OutputDirectory;

        static TextureExtractor.ExtractorOptions options;
        static Cleanup.Option cleanOptions;

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
            FormatDictionary.GetValue("AFS").Class = typeof(AFSLib.AFS);
            FormatDictionary.GetValue("J3D2bdl4").Class = typeof(Hack.io.BDL);
            FormatDictionary.GetValue("J3D2bmd3").Class = typeof(Hack.io.BMD);
            FormatDictionary.GetValue("TEX1").Class = typeof(Hack.io.BMD.TEX1);
            GC.Collect();

            //are we able to change the Title?
            try
            {
                Console.Title = Title;
            }
            catch (Exception) { }

            options = new TextureExtractor.ExtractorOptions();
            cleanOptions = new Cleanup.Option();

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
                    var key = Console.ReadKey().Key;
                    switch (key)
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
                    Console.WriteLine("Specify a directory or a file to be extracted.");
                    do
                    {
                        InputPath = Console.ReadLine().Trim('"');

                        if (!Directory.Exists(InputPath) && !File.Exists(InputPath))
                        {
                            ConsoleEx.WriteLineColoured("The directory or file could not be found!", ConsoleColor.Red);
                        }
                    } while (!Directory.Exists(InputPath) && !File.Exists(InputPath));

                    //Output Path
                    Console.WriteLine();
                    Console.WriteLine("Output Path:");
                    Console.WriteLine("Specify an output directory where the extra files will be saved.");
                    Console.WriteLine($"Or use default \t\"{GetGenOutputPath(InputPath)}\"");
                    do
                    {
                        OutputDirectory = Console.ReadLine().Trim('"');
                        if (OutputDirectory == "")
                        {
                            OutputDirectory = GetGenOutputPath(InputPath);
                        }
                        if (!PathIsValid(OutputDirectory))
                        {
                            ConsoleEx.WriteLineColoured("Path is invalid!", ConsoleColor.Red);
                        }
                        if (OutputDirectory == InputPath)
                        {
                            ConsoleEx.WriteLineColoured("Output Directory and Input Path cannot be the same!", ConsoleColor.Red);
                        }

                    } while (!PathIsValid(OutputDirectory) || OutputDirectory == InputPath);



                    //Options
                    Console.WriteLine();
                    PrintOptions(options);

                    Console.WriteLine();
                    Console.WriteLine("Adjust settings? Yes or (No)");
                    if (ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(false), "Yes", "\tNo", ConsoleColor.Green, ConsoleColor.Red))
                    {
                        switch (Mode)
                        {
                            case Modes.Extract:
                                Console.WriteLine($"Extract mipmaps. \t(True) or False");
                                options.Mips = ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(true, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);
                                Console.WriteLine($"Extracts raw image files. \tTrue or (False)");
                                options.Raw = ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(false, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);
                                Console.WriteLine($"Tries to extract textures from unknown file formats, may cause errors. \tTrue or (False)");
                                options.Force = ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(false, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);
                                Console.WriteLine($"Tries to Imitate dolphin mipmap detection. \t(True) or False");
                                options.DolphinMipDetection = ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(true, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);
                                Console.WriteLine($"Clean up typ. \t0={Cleanup.Type.None} (1)={Cleanup.Type.Default} 2={Cleanup.Type.Simple}");
                                cleanOptions.CleanupType = ConsoleEx.ReadEnum<Cleanup.Type>(Cleanup.Type.Default);
                                if (cleanOptions.CleanupType == Cleanup.Type.Default)
                                {
                                    Console.WriteLine($"CleanUp, minimum files per folder. \t(3)");
                                    cleanOptions.MinGroupsSize = ConsoleEx.ReadInt32(3);
                                }
                                break;
                            case Modes.Unpacks:

                                Console.WriteLine($"Tries to extract unknown file formats, may cause errors. \tTrue or (False)");
                                options.Force = ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(false, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);

                                break;
                        }

                        Console.WriteLine($"Perform a Dry Run. (Test) \tTrue or (False)");
                        options.DryRun = ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(false, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);
                        Console.WriteLine($"High performance mode.(Multithreading) \t(True) or False");
                        options.Parallel.MaxDegreeOfParallelism = ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(true, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red) ? 4 : 1;

                    }

                    //Inputs correct?
                    Console.WriteLine();
                    Console.WriteLine($"Input Path: \"{InputPath}\"");
                    Console.WriteLine($"Output Path: \"{OutputDirectory}\"");
                    PrintOptions(options);

                    Console.WriteLine();
                    Console.WriteLine("Are the settings correct? \t(Yes) or No");
                    if (!ConsoleEx.WriteBoolPrint(ConsoleEx.ReadBool(true), "Yes", "\tNo", ConsoleColor.Green, ConsoleColor.Red)) continue;

                    /*
                    var main = new DirectoryInfo(InputPath);
                    foreach (var item in main.GetFiles())
                    {
                        Bitmap bitmap = new Bitmap(item.FullName);
                        var data = new List<byte>();
                        var pall = new List<byte>();
                        GetImageAndPaletteData(ref data, ref pall, bitmap, GXImageFormat.I4, GXPaletteFormat.RGB5A3);
                        //EncodeImage(ref data, bitmap, GXImageFormat.C14X2, new Dictionary<Color, int>());
                        Bitmap newmap = DecodeImage(data.ToArray(), pall.ToArray(), GXImageFormat.I4, GXPaletteFormat.RGB5A3, pall.Count / 2, bitmap.Width, bitmap.Height);
                        bitmap.Dispose();
                        newmap.Save(item.FullName, ImageFormat.Png);
                        newmap.Dispose();
                    }
                    */

                    //Start
                    Console.CursorVisible = false;
                    switch (Mode)
                    {
                        case Modes.Extract:
                            Console.WriteLine($"Search and extract textures from {InputPath}");
                            Console.WriteLine("This may take a few seconds...");
                            Console.WriteLine();
                            var result = TextureExtractor.StartScan(InputPath, OutputDirectory, options);
                            Console.WriteLine();
                            PrintResult(result);

                            if (cleanOptions.CleanupType != Cleanup.Type.None)
                            {
                                Console.WriteLine("Start Cleanup...");
                                if (Cleanup.Start(new DirectoryInfo(OutputDirectory), cleanOptions))
                                    Console.WriteLine("Cleanup Completed");
                                else
                                    Console.WriteLine("Error! Cleanup failed");
                            }
                            break;
                        case Modes.Unpacks:
                            Console.WriteLine($"Unpacks all files from {InputPath}");
                            Console.WriteLine("This may take a few seconds...");
                            Console.WriteLine();
                            Unpack.StartScan(InputPath, OutputDirectory, options);
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
                        ConsoleEx.WriteLineColoured(StringEx.Divider(), ConsoleColor.Blue);
                        Console.WriteLine($"Known formats: {FormatDictionary.Master.Length}.");
                        ConsoleEx.WriteLineColoured(StringEx.Divider(), ConsoleColor.Blue);
                        foreach (var item in FormatDictionary.Master)
                            Console.WriteLine($"{item.GetFullDescription()} Typ:{item.Typ}");
                        ConsoleEx.WriteLineColoured(StringEx.Divider(), ConsoleColor.Blue);
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

                        Cutter.StartScan(InputPath, OutputDirectory, pattern, options);
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

                        Unpack.StartScan(InputPath, OutputDirectory, options);
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
                                    case "-dryrun":
                                    case "-d":
                                        options.DryRun = true;
                                        break;
                                    case "-cleanup":
                                    case "-c":
                                    case "-cleanup:default":
                                    case "-c:default":
                                    case "-c:d":
                                        cleanOptions.CleanupType = Cleanup.Type.Default;
                                        break;
                                    case "-cleanup:none":
                                    case "-c:none":
                                    case "-c:n":
                                        cleanOptions.CleanupType = Cleanup.Type.None;
                                        break;
                                    case "-cleanup:simple":
                                    case "-c:simple":
                                    case "-c:s":
                                        cleanOptions.CleanupType = Cleanup.Type.Simple;
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
                                        options.DolphinMipDetection = true;
                                        break;
                                    case "-groups":
                                    case "-g":
                                    case "-tasks":
                                    case "-t":
                                        if (!int.TryParse(args[++i], out int parse))
                                        {
                                            Console.Error.WriteLine($"Wrong syntax: \"{args[i - 1]} {args[i]}\" Task needs a second parameter.");
                                            Console.WriteLine("use h for help");
                                            Environment.Exit(-2);
                                            break;
                                        }
                                        switch (args[i - 1].ToLower())
                                        {
                                            case "-groups":
                                            case "-g":
                                                cleanOptions.MinGroupsSize = parse <= 0 ? 1 : parse;
                                                break;
                                            case "-tasks":
                                            case "-t":
                                                options.Parallel.MaxDegreeOfParallelism = parse <= 0 ? -1 : parse;
                                                break;
                                        }
                                        break;
                                }
                            }
                        }
                        var result = TextureExtractor.StartScan(InputPath, OutputDirectory, options);

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
                            InputPath = args[0];
                            OutputDirectory = GetGenOutputPath(InputPath);
                            TextureExtractor.StartScan(InputPath, OutputDirectory);
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

            InputPath = args[1];
            if (args.Length >= 3)
                OutputDirectory = args[2];
            else
                OutputDirectory = GetGenOutputPath(InputPath);

            if (!Directory.Exists(InputPath) && !File.Exists(InputPath))
            {
                Console.WriteLine("The directory could not be found!");
                Console.WriteLine(InputPath);
                return -1;
            }
            if (!PathIsValid(OutputDirectory))
            {
                OutputDirectory = GetGenOutputPath(InputPath);
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
            Console.WriteLine("\tOption:\t -d -dryrun\tDoesn't actually extract anything.");
            Console.WriteLine("\tOption:\t -c -cleanup\tuses the default folder cleanup.");
            Console.WriteLine("\tOption:\t -c:n -cleanup:none\tRetains the original folder structure.");
            Console.WriteLine("\tOption:\t -c:s -cleanup:simple\tMove all files to a single folder.");
            Console.WriteLine("\tOption:\t -c:s -cleanup:simple\tMove all files to a single folder.");
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
            ConsoleEx.WriteLineColoured(StringEx.Divider(), ConsoleColor.Blue);
            Console.ForegroundColor = ConsoleColor.Cyan;
#if DEBUG
            Console.WriteLine($"{System.Diagnostics.Process.GetCurrentProcess().ProcessName} v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} {IntPtr.Size * 8}bit\t\t{DateTime.Now.ToString()}\t\t*DEBUG");
#else
            Console.WriteLine($"{System.Diagnostics.Process.GetCurrentProcess().ProcessName} v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} {IntPtr.Size * 8}bit\t\t{DateTime.Now.ToString()}");
#endif
            PrintFormats();
            ConsoleEx.WriteLineColoured(StringEx.Divider(), ConsoleColor.Blue);
            ConsoleEx.WriteColoured("INFO:", ConsoleColor.Red);
            Console.WriteLine(" If you use RVZ images, unpack them into a folder using Dolphin.");
            Console.WriteLine("right click on a game -> Properties -> Filesystem -> right click on \"Disc - [Game ID]\" -> Extract Files...");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void PrintFormats()
        {
            List<string> formats = new List<string>(AuroraLip.Common.Reflection.FileAccess.GetReadable().Select(x => x.Name));
            formats.Add("AFS");
            formats.Add("BDL4");
            formats.Add("BMD3");
            formats.Add("TEX1");
            formats.Sort();
            Console.WriteLine($"Supported formats: {string.Join(", ", formats)}.".LineBreak(20));
        }

        static void PrintOptions(TextureExtractor.ExtractorOptions options)
        {
            Console.Write($"Force extract unknown formats: ");
            ConsoleEx.WriteBoolPrint(options.Force, ConsoleColor.Green, ConsoleColor.Red);
            switch (Mode)
            {
                case Modes.Extract:
                    Console.Write($"Extracts Mipmaps: ");
                    ConsoleEx.WriteBoolPrint(options.Mips, ConsoleColor.Green, ConsoleColor.Red);
                    Console.Write($"Extracts raw image files: ");
                    ConsoleEx.WriteBoolPrint(options.Raw, ConsoleColor.Green, ConsoleColor.Red);
                    Console.Write($"Imitate dolphin mipmap detection: ");
                    ConsoleEx.WriteBoolPrint(options.DolphinMipDetection, ConsoleColor.Green, ConsoleColor.Red);
                    Console.Write($"Clean up type: ");
                    if (cleanOptions.CleanupType == Cleanup.Type.None)
                        ConsoleEx.WriteLineColoured(cleanOptions.CleanupType.ToString(), ConsoleColor.Red);
                    else
                    {
                        ConsoleEx.WriteLineColoured(cleanOptions.CleanupType.ToString(), ConsoleColor.Green);
                        Console.Write($"Clean minimum files per foldere: ");
                        ConsoleEx.WriteLineColoured(cleanOptions.MinGroupsSize.ToString(), ConsoleColor.Green);
                    }
                    break;
                case Modes.Unpacks:
                    break;
            }
            Console.Write($"Perform a dry run: ");
            ConsoleEx.WriteBoolPrint(options.DryRun, ConsoleColor.Green, ConsoleColor.Red);
            Console.Write($"High performance mode (Multithreading): ");
            ConsoleEx.WriteBoolPrint(options.Parallel.MaxDegreeOfParallelism != 1, ConsoleColor.Green, ConsoleColor.Red);
        }

        static void PrintResult(TextureExtractor.ExtractorResult result)
        {
            ConsoleEx.WriteLineColoured(StringEx.Divider(), ConsoleColor.Blue);
            Console.WriteLine(result.ToString().LineBreak(20));
            Console.WriteLine($"Log saved: \"{result.LogFullPath}\"");
            ConsoleEx.WriteLineColoured(StringEx.Divider(), ConsoleColor.Blue);
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
            Console.Write($" {ProgressPercentage:00.00}%");
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
                    Console.Title = $"{Title} | {ProgressPercentage:00.00}%";
                else
                    Console.Title = Title;
            }
            catch (Exception)
            { }
        }

        //change only with caution! this function is required by the Custom Texture Tool https://forums.dolphin-emu.org/Thread-custom-texture-tool-ps-v50-1
        static void TextureUpdate(JUTTexture.TexEntry texture, Results result, in string subdirectory)
        {
            double ProgressPercentage = result.ProgressLength / result.WorkeLength * 100;
            Console.WriteLine($"Prog:{Math.Round(ProgressPercentage, 2)}% Extract:{Path.Combine(subdirectory, texture.GetDolphinTextureHash()) + ".png"} mips:{texture.Count - 1} LODBias:{texture.LODBias} MinLOD:{texture.MinLOD} MaxLOD:{texture.MaxLOD}");
        }
    }
}
