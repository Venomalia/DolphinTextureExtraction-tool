using AuroraLib.Common;
using AuroraLib.Core.Text;
using AuroraLib.Texture;
using DolphinTextureExtraction.Data;
using SixLabors.ImageSharp;
using System.Text;
using static DolphinTextureExtraction.ScanBase;

namespace DolphinTextureExtraction
{
    partial class Program
    {
        static string InputPath;

        static string OutputDirectory;

        static TextureExtractor.ExtractorOptions options;
        static Cleanup.Option cleanOptions;

        static string Algorithm;

        static Modes Mode;

#if DEBUG
        private static readonly string Title = $"{System.Diagnostics.Process.GetCurrentProcess().ProcessName} v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} {IntPtr.Size * 8}bit *DEBUG";
#else
        private static readonly string Title = $"{System.Diagnostics.Process.GetCurrentProcess().ProcessName} v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} {IntPtr.Size * 8}bit";
#endif


        static Program()
        {
            //link external classes
            if (FormatDictionary.TryGetValue(new Identifier64("J3D2bdl4"), out FormatInfo formatInfo))
            {
                formatInfo.Class = typeof(Hack.io.BDL);
            }
            if (FormatDictionary.TryGetValue(new Identifier64("J3D2bmd3"), out formatInfo))
            {
                formatInfo.Class = typeof(Hack.io.BMD);
            }
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

                #region Main loop
                while (true)
                {
                    Console.WriteLine();
                    Console.WriteLine("Select a Mode:");
                    Console.WriteLine();
                    ModesEX.PrintModes();
                    Console.WriteLine();
                    Mode = ModesEX.SelectMode();
                    Console.WriteLine($"Mode: {Mode.GetDescription()}");

                    switch (Mode)
                    {
                        case Modes.Help:
                            PrintHelp();
                            break;
                        case Modes.Compress:
                        case Modes.Split:
                        case Modes.Unpack:
                        case Modes.Extract:
                            #region Extract

                            #region User Set Input Path
                            Console.WriteLine();
                            Console.WriteLine("Input Path:");
                            Console.WriteLine("Specify a directory or a file.");
                            do
                            {
                                InputPath = Console.ReadLine().Trim('"');

                                if (!Directory.Exists(InputPath) && !File.Exists(InputPath))
                                {
                                    ConsoleEx.WriteLineColoured("The directory or file could not be found!", ConsoleColor.Red);
                                }
                            } while (!Directory.Exists(InputPath) && !File.Exists(InputPath));
                            #endregion

                            #region User Set Output Path
                            Console.WriteLine();
                            Console.WriteLine("Output Path:");
                            Console.WriteLine("Specify an output directory where the files will be saved.");
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

                            #endregion

                            #region Algorithm

                            if (Mode == Modes.Compress)
                            {
                                var algo = Reflection.Compression.GetWritable().Select(s => s.Name);

                                Console.WriteLine();
                                Console.WriteLine("Algorithm:");
                                Console.WriteLine("Specify a compression algorithm to be used.");
                                Console.Write("Algoriths: ");
                                Console.WriteLine(string.Join(", ", algo));
                                do
                                {
                                    Algorithm = Console.ReadLine();
                                    if (!algo.Contains(Algorithm))
                                    {
                                        ConsoleEx.WriteLineColoured("Algorithm is invalid!", ConsoleColor.Red);
                                    }
                                } while (!algo.Contains(Algorithm));
                            }
                            #endregion

                            #region Split Pattern
                            List<byte[]> pattern = new();

                            if (Mode == Modes.Split)
                            {
                                List<string> patternstrings = pattern.Select(s => EncodingX.GetValidString(s)).ToList();
                                do
                                {
                                    if (patternstrings.Count > 0)
                                    {
                                        Console.WriteLine(string.Join(", ", patternstrings));
                                        Console.WriteLine("Edit identifiers? Yes or (No)");
                                        if (!ConsoleEx.WriteLineBoolPrint(ConsoleEx.ReadBool(false), "Yes", "\tNo", ConsoleColor.Green, ConsoleColor.Red))
                                        {
                                            pattern = patternstrings.Select(s => s.GetBytes()).ToList();
                                            break;
                                        }
                                    }

                                    Console.WriteLine("For which identifiers should be searched? \tdivided by \",\"");
                                    Console.Write(string.Join(", ", patternstrings));
                                    Console.CursorLeft = 0;
                                    patternstrings = Console.ReadLine().Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList();
                                } while (true);
                            }
                            #endregion

                            #region Adjust settings?
                            ConsoleEx.WriteLineColoured(StringEx.Divider(), ConsoleColor.Blue);
                            PrintOptions();
                            ConsoleEx.WriteLineColoured(StringEx.Divider(), ConsoleColor.Blue);
                            Console.WriteLine();
                            Console.WriteLine("Adjust settings? Yes or (No)");
                            if (ConsoleEx.WriteLineBoolPrint(ConsoleEx.ReadBool(false), "Yes", "\tNo", ConsoleColor.Green, ConsoleColor.Red))
                            {
                                UserSetOptions();
                            }
                            #endregion

                            #region settings correct?
                            Console.WriteLine();
                            ConsoleEx.WriteLineColoured(StringEx.Divider(), ConsoleColor.Blue);
                            Console.WriteLine($"Mode: {Mode.GetDescription()}");
                            Console.WriteLine($"Input Path: \"{InputPath}\"");
                            Console.WriteLine($"Output Path: \"{OutputDirectory}\"");
                            if (Mode == Modes.Split) Console.WriteLine($"Pattern: {string.Join(", ", pattern.Select(s => EncodingX.GetValidString(s)))}");
                            PrintOptions();
                            ConsoleEx.WriteLineColoured(StringEx.Divider(), ConsoleColor.Blue);

                            Console.WriteLine();
                            Console.WriteLine("Are the settings correct? \t(Yes) or No");
                            if (!ConsoleEx.WriteLineBoolPrint(ConsoleEx.ReadBool(true), "Yes", "\tNo", ConsoleColor.Green, ConsoleColor.Red)) continue;
                            #endregion

                            #region Start
                            Console.CursorVisible = false;
                            switch (Mode)
                            {
                                case Modes.Extract:
                                    Console.WriteLine($"Search and extract textures from {InputPath}");
                                    Console.WriteLine("This may take a few seconds...");
                                    Console.WriteLine();
                                    var result = TextureExtractor.StartScan(InputPath, OutputDirectory, options);
                                    PrintResult(result);

                                    if (options.DryRun == false || cleanOptions.CleanupType != Cleanup.Type.None)
                                    {
                                        Console.WriteLine("Start Cleanup...");
                                        if (Cleanup.Start(new DirectoryInfo(OutputDirectory), cleanOptions))
                                            Console.WriteLine("Cleanup Completed");
                                        else
                                            Console.WriteLine("Error! Cleanup failed");
                                    }
                                    break;
                                case Modes.Unpack:
                                    Console.WriteLine($"Unpacks all files from {InputPath}");
                                    Console.WriteLine("This may take a few seconds...");
                                    Console.WriteLine();
                                    Unpack.StartScan(InputPath, OutputDirectory, options);

                                    break;
                                case Modes.Compress:
                                    Console.WriteLine($"Compress data from {InputPath}");
                                    Compress.StartScan(InputPath, OutputDirectory, Reflection.Compression.GetByName(Algorithm), options);
                                    break;
                                case Modes.Split:
                                    Console.WriteLine($"Split data from {InputPath}");

                                    Cutter.StartScan(InputPath, OutputDirectory, pattern, options);
                                    break;
                            }
                            Console.WriteLine();
                            Console.WriteLine("Done!");
                            Console.CursorVisible = true;
                            #endregion

                            #endregion
                            break;
                        case Modes.Formats:
                            PrintFormats();
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                #endregion
            }
            else
            {
                int p;

                if (ModesEX.TryParse(args[0].ToLower(), out Mode))
                {
                    switch (Mode)
                    {
                        case Modes.Help:
                            PrintHeader();
                            PrintHelp();
                            break;
                        case Modes.Extract:
                            #region extract
                            p = GetPahts(args);
                            if (p <= 0)
                                goto default;

                            options = new TextureExtractor.ExtractorOptions() { Mips = false, Raw = false, Force = false, ProgressAction = options.ProgressAction };
                            if (args.Length > p)
                            {
                                ParseOptions(args.AsSpan(p));
                            }

                            var result = TextureExtractor.StartScan(InputPath, OutputDirectory, options);

                            Console.WriteLine();

                            if (options.TextureAction == null)
                                PrintResult(result);

                            #endregion
                            break;
                        case Modes.Unpack:
                            #region unpack
                            p = GetPahts(args);
                            if (p <= 0)
                                goto default;

                            options = new TextureExtractor.ExtractorOptions() { Mips = false, Raw = false, Force = false, ProgressAction = options.ProgressAction };
                            if (args.Length > p)
                            {
                                ParseOptions(args.AsSpan(p));
                            }

                            Unpack.StartScan(InputPath, OutputDirectory, options);
                            Console.WriteLine();
                            Console.WriteLine("completed.");
                            #endregion
                            break;
                        case Modes.Compress:
                            p = GetPahts(args);
                            if (p <= 0)
                                goto default;

                            Compress.StartScan(InputPath, OutputDirectory, Reflection.Compression.GetByName(args[p++]), options);
                            Console.WriteLine();
                            Console.WriteLine("completed.");
                            break;
                        case Modes.Split:
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
                                    pattern.Add(args[p++].GetBytes());
                                }
                            }

                            Cutter.StartScan(InputPath, OutputDirectory, pattern, options);
                            Console.WriteLine();
                            Console.WriteLine("completed.");
                            #endregion
                            break;
                        case Modes.Formats:
                            PrintFormats();
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    Environment.Exit(0);
                }
                if (Directory.Exists(args[0]))
                {
                    InputPath = args[0];
                    OutputDirectory = GetGenOutputPath(InputPath);
                    TextureExtractor.StartScan(InputPath, OutputDirectory);
                }
                Console.Error.WriteLine("Wrong syntax.");
                Console.WriteLine("use h for help");
                Environment.Exit(-2);
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
            DirectoryInfo InputInfo = new(Input);
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

        private static void PrintFormats()
        {
            ConsoleEx.WriteLineColoured(StringEx.Divider(), ConsoleColor.Blue);
            Console.WriteLine($"Known formats: {FormatDictionary.Master.Length}.");
            ConsoleEx.WriteLineColoured(StringEx.Divider(), ConsoleColor.Blue);
            foreach (var item in FormatDictionary.Master)
            {
                Console.Write($"{item.GetFullDescription()}");
                Console.CursorLeft = 45;
                ConsoleEx.WriteColoured("Typ:", ConsoleColor.Cyan);
                Console.Write(item.Typ);
                Console.CursorLeft = 60;
                ConsoleEx.WriteColoured("supported:", ConsoleColor.Cyan);
                ConsoleEx.WriteBoolPrint(item.Class != null, ConsoleColor.Green, ConsoleColor.Red);
                if (item.Identifier != null)
                {
                    Console.CursorLeft = 80;
                    ConsoleEx.WriteColoured("Identifier:", ConsoleColor.Cyan);
                    Console.Write(item.Identifier.ToString());
                }
                Console.WriteLine();
            }
            ConsoleEx.WriteLineColoured(StringEx.Divider(), ConsoleColor.Blue);
        }

        private static void PrintHelp()
        {
            foreach (var mode in Enum.GetValues<Modes>())
            {
                ConsoleEx.WriteColoured($"{mode}", ConsoleColor.Red);
                Console.CursorLeft = 13;
                ConsoleEx.WriteColoured($"{mode.GetAlias()}", ConsoleColor.Red);
                Console.WriteLine($"\t{mode.GetDescription()}");
                if (!mode.GetSyntax().IsEmpty)
                {
                    ConsoleEx.WriteColoured("\tSyntax: ", ConsoleColor.Cyan);
                    Console.WriteLine(mode.GetSyntax().ToString());
                    var options = mode.GetOptions();
                    if (options.Length != 0)
                    {
                        ConsoleEx.WriteColoured("\tOptions: ", ConsoleColor.Cyan);
                        for (int i = 0; i < options.Length; i++)
                        {
                            if (i != 0) Console.Write(", ");
                            Console.Write($"{options[i]}");
                        }
                        Console.WriteLine();
                    }
                    switch (mode)
                    {
                        case Modes.Compress:
                            ConsoleEx.WriteColoured("\tAlgorithm: ", ConsoleColor.Cyan);
                            Console.WriteLine(string.Join(", ", Reflection.Compression.GetWritable().Select(s => s.Name)));
                            break;
                        case Modes.Split:
                            ConsoleEx.WriteColoured("\tPatterns: ", ConsoleColor.Cyan);
                            Console.WriteLine("A identifier or a list divided with \" \"");
                            break;
                    }
                }
                Console.WriteLine();
            }
            ConsoleEx.WriteLineColoured(StringEx.Divider(), ConsoleColor.Blue);
            ConsoleEx.WriteLineColoured($"Options", ConsoleColor.Red);
            ConsoleEx.WriteLineColoured(StringEx.Divider(), ConsoleColor.Blue);
            foreach (var Option in Enum.GetValues<Options>())
            {
                ConsoleEx.WriteColoured($"-{Option}", ConsoleColor.Red);
                Console.CursorLeft = 25;
                ConsoleEx.WriteColoured($"-{Option.GetAlias()}", ConsoleColor.Red);
                Console.WriteLine($"\t{Option.GetDescription()}");

                if (!Option.GetSyntax().IsEmpty)
                {
                    ConsoleEx.WriteColoured("\tSyntax: ", ConsoleColor.Cyan);
                    Console.WriteLine(Option.GetSyntax().ToString());

                    switch (Option)
                    {
                        case Options.Tasks:
                            ConsoleEx.WriteColoured("\ti: ", ConsoleColor.Cyan);
                            Console.WriteLine("a number that determines the maximum number of tasks.");
                            break;
                        case Options.Progress:
                            ConsoleEx.WriteLineColoured("\tModes: ", ConsoleColor.Cyan);
                            Console.WriteLine("\t\tnone\toutputs only important events in the console.");
                            Console.WriteLine("\t\tbar\tshows the progress as a progress bar in the console.");
                            Console.WriteLine("\t\tlist\toutputs the extracted textures as a list in the console.");
                            break;
                        case Options.Cleanup:
                            ConsoleEx.WriteLineColoured("\tModes: ", ConsoleColor.Cyan);
                            Console.WriteLine("\t\tnone\tRetains the original folder structure.");
                            Console.WriteLine("\t\tsimple\tMove all files to a single folder.");
                            Console.WriteLine("\t\tdefault\tShortens the path by deleting unnecessary folders.");
                            break;
                    }
                }
            }
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
            List<string> formats = new(AuroraLib.Common.Reflection.FileAccess.GetReadable().Select(x => x.Name)) { "BDL4", "BMD3", "TEX1" };
            formats.Sort();
            Console.WriteLine($"Supported formats: {string.Join(", ", formats)}.".LineBreak(20));

            ConsoleEx.WriteLineColoured(StringEx.Divider(), ConsoleColor.Blue);
            ConsoleEx.WriteColoured("INFO:", ConsoleColor.Red);
            Console.WriteLine(" If you use RVZ images, unpack them into a folder using Dolphin.");
            Console.WriteLine("right click on a game -> Properties -> Filesystem -> right click on \"Disc - [Game ID]\" -> Extract Files...");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void PrintOptions()
        {
            var modeoptions = Mode.GetOptions();
            foreach (var option in modeoptions)
            {
                Console.Write($"{option.GetDescription()} : ");
                switch (option)
                {
                    case Options.Force:
                        ConsoleEx.WriteLineBoolPrint(options.Force, ConsoleColor.Green, ConsoleColor.Red);
                        break;
                    case Options.Dryrun:
                        ConsoleEx.WriteLineBoolPrint(options.DryRun, ConsoleColor.Green, ConsoleColor.Red);
                        break;
                    case Options.Progress:
                        Console.CursorLeft = 0;
                        break;
                    case Options.Tasks:
                        Console.CursorLeft = 0;
                        Console.Write($"High performance mode. (Multithreading) : ");
                        ConsoleEx.WriteLineBoolPrint(options.Parallel.MaxDegreeOfParallelism != 1, ConsoleColor.Green, ConsoleColor.Red);
                        break;
                    case Options.Mip:
                        ConsoleEx.WriteLineBoolPrint(options.Mips, ConsoleColor.Green, ConsoleColor.Red);
                        break;
                    case Options.Raw:
                        ConsoleEx.WriteLineBoolPrint(options.Raw, ConsoleColor.Green, ConsoleColor.Red);
                        break;
                    case Options.Cleanup:
                        if (cleanOptions.CleanupType == Cleanup.Type.None)
                            ConsoleEx.WriteLineColoured(cleanOptions.CleanupType.ToString(), ConsoleColor.Red);
                        else
                        {
                            ConsoleEx.WriteLineColoured(cleanOptions.CleanupType.ToString(), ConsoleColor.Green);
                            Console.Write($"Clean minimum files per foldere: ");
                            ConsoleEx.WriteLineColoured(cleanOptions.MinGroupsSize.ToString(), ConsoleColor.Green);
                        }
                        break;
                    case Options.Dolphinmipdetection:
                        ConsoleEx.WriteLineBoolPrint(options.DolphinMipDetection, ConsoleColor.Green, ConsoleColor.Red);
                        break;
                    case Options.Recursiv:
                        ConsoleEx.WriteLineBoolPrint(options.Deep == 0, ConsoleColor.Green, ConsoleColor.Red);
                        break;
                    case Options.ArbitraryMipmapDetection:
                        ConsoleEx.WriteLineBoolPrint(options.ArbitraryMipmapDetection, ConsoleColor.Green, ConsoleColor.Red);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private static void UserSetOptions()
        {
            var modeoptions = Mode.GetOptions();
            foreach (var option in modeoptions)
            {
                Console.Write($"{option.GetDescription()} \t");
                switch (option)
                {
                    case Options.Force:
                        HelpPrintTrueFalse(options.Force);
                        options.Force = ConsoleEx.WriteLineBoolPrint(ConsoleEx.ReadBool(options.Force, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);
                        break;
                    case Options.Dryrun:
                        HelpPrintTrueFalse(options.DryRun);
                        options.DryRun = ConsoleEx.WriteLineBoolPrint(ConsoleEx.ReadBool(options.DryRun, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);
                        break;
                    case Options.Progress:
                        Console.CursorLeft = 0;
                        break;
                    case Options.Tasks:
                        Console.CursorLeft = 0;
                        Console.Write($"High performance mode.(Multithreading) \t");
                        HelpPrintTrueFalse(options.Parallel.MaxDegreeOfParallelism != 0);
                        options.Parallel.MaxDegreeOfParallelism = ConsoleEx.WriteLineBoolPrint(ConsoleEx.ReadBool(options.Parallel.MaxDegreeOfParallelism != 0, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red) ? 4 : 1;
                        break;
                    case Options.Mip:
                        HelpPrintTrueFalse(options.Mips);
                        options.Mips = ConsoleEx.WriteLineBoolPrint(ConsoleEx.ReadBool(options.Mips, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);
                        break;
                    case Options.Raw:
                        HelpPrintTrueFalse(options.Raw);
                        options.Raw = ConsoleEx.WriteLineBoolPrint(ConsoleEx.ReadBool(options.Raw, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);
                        break;
                    case Options.Cleanup:
                        Console.WriteLine($"0={Cleanup.Type.None} 1={Cleanup.Type.Default} 2={Cleanup.Type.Simple}");
                        cleanOptions.CleanupType = ConsoleEx.ReadEnum<Cleanup.Type>(cleanOptions.CleanupType);
                        if (cleanOptions.CleanupType == Cleanup.Type.Default)
                        {
                            Console.WriteLine($"CleanUp, minimum files per folder. \t(3)");
                            cleanOptions.MinGroupsSize = ConsoleEx.ReadInt32(3);
                        }
                        break;
                    case Options.Dolphinmipdetection:
                        HelpPrintTrueFalse(options.DolphinMipDetection);
                        options.DolphinMipDetection = ConsoleEx.WriteLineBoolPrint(ConsoleEx.ReadBool(options.DolphinMipDetection, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);
                        break;
                    case Options.Recursiv:
                        HelpPrintTrueFalse(options.Deep == 0);
                        options.Deep = ConsoleEx.WriteLineBoolPrint(ConsoleEx.ReadBool(options.Deep == 0, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red) ? (uint)0 : (uint)1;
                        break;
                    case Options.ArbitraryMipmapDetection:
                        HelpPrintTrueFalse(options.ArbitraryMipmapDetection);
                        options.ArbitraryMipmapDetection = ConsoleEx.WriteLineBoolPrint(ConsoleEx.ReadBool(options.ArbitraryMipmapDetection, ConsoleKey.T, ConsoleKey.F), "True", "\tFalse", ConsoleColor.Green, ConsoleColor.Red);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private static void ParseOptions(ReadOnlySpan<string> args)
        {

            for (int i = 0; i < args.Length; i++)
            {

                int Separator = args[i].IndexOf(":");
                ReadOnlySpan<char> arg2 = string.Empty;
                if (Separator == -1)
                    Separator = args[i].Length;
                else
                    arg2 = args[i].AsSpan(Separator + 1, args[i].Length - Separator - 1);

                if (args[i][0] == '-' && OptionsEX.TryParse(args[i].AsSpan(1, Separator - 1), out Options option))
                {
                    switch (option)
                    {
                        case Options.Force:
                            options.Force = true;
                            break;
                        case Options.Dryrun:
                            options.DryRun = true;
                            break;
                        case Options.Progress:
                            switch (arg2.ToString())
                            {

                                case "none":
                                case "n":
                                    options.ProgressAction = ProgressTitleUpdate;
                                    options.TextureAction = null;
                                    break;
                                case "bar":
                                case "b":
                                    options.ProgressAction = ProgressUpdate;
                                    options.TextureAction = null;
                                    break;
                                case "list":
                                case "l":
                                    options.ProgressAction = null;
                                    options.TextureAction = TextureUpdate;
                                    break;
                            }
                            break;
                        case Options.Tasks:
                            if (!int.TryParse(args[++i], out int parse))
                            {
                                Console.Error.WriteLine($"Wrong syntax: \"{args[i - 1]} {args[i]}\" Task needs a second parameter.");
                                Console.WriteLine("use h for help");
                                Environment.Exit(-2);
                                break;
                            }
                            options.Parallel.MaxDegreeOfParallelism = parse <= 0 ? -1 : parse;
                            break;
                        case Options.Mip:
                            options.Mips = true;
                            break;
                        case Options.Raw:
                            options.Raw = true;
                            break;
                        case Options.Cleanup:
                            switch (arg2.ToString())
                            {
                                case "none":
                                case "n":
                                    cleanOptions.CleanupType = Cleanup.Type.None;
                                    break;
                                case "default":
                                case "d":
                                    cleanOptions.CleanupType = Cleanup.Type.Default;
                                    break;
                                case "simple":
                                case "s":
                                    cleanOptions.CleanupType = Cleanup.Type.Simple;
                                    break;
                                case "groupsize":
                                case "gs":
                                    if (!int.TryParse(args[++i], out int groupparse))
                                    {
                                        Console.Error.WriteLine($"Wrong syntax: \"{args[i - 1]} {args[i]}\" Task needs a second parameter.");
                                        Console.WriteLine("use h for help");
                                        Environment.Exit(-2);
                                        break;
                                    }
                                    cleanOptions.MinGroupsSize = groupparse <= 0 ? 1 : groupparse;
                                    break;
                            }
                            break;
                        case Options.Dolphinmipdetection:
                            options.DolphinMipDetection = true;
                            break;
                        case Options.Recursiv:
                            options.Deep = 1;
                            break;
                        case Options.ArbitraryMipmapDetection:
                            options.ArbitraryMipmapDetection = true;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }
        private static void HelpPrintTrueFalse(bool value)
            => Console.WriteLine(value ? "(True) or False" : "True or (False)");

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
                {
                    if (result is TextureExtractor.ExtractorResult exResult)
                    {
                        Console.Title = $"{Title} | {ProgressPercentage:00.00}% | Textures: {exResult.Extracted}";
                    }
                    else
                    {
                        Console.Title = $"{Title} | {ProgressPercentage:00.00}%";
                    }
                }
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
            StringBuilder sb = new();
            sb.Append("Prog:");
            sb.Append(Math.Round(ProgressPercentage, 2));
            sb.Append("% Extract:");
            sb.Append(Path.Combine(subdirectory, texture.GetDolphinTextureHash()));
            sb.Append(".png mips:");
            sb.Append(texture.Count - 1);
            sb.Append(" LODBias:");
            sb.Append(texture.LODBias);
            sb.Append(" MinLOD:");
            sb.Append(texture.MinLOD);
            sb.Append(" MaxLOD:");
            sb.Append(texture.MinLOD);
            sb.AppendLine();
            lock (result)
            {
                Console.Out.Write(sb);
            }
        }
    }

}
