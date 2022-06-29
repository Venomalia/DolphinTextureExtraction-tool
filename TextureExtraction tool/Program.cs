using System;
using System.IO;

namespace DolphinTextureExtraction_tool
{
    partial class Program
    {
        static string InputDirectory;

        static string OutputDirectory;

        static TextureExtractor.Options options = new TextureExtractor.Options();

        static void Main(string[] args)
        {
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
                            Console.WriteLine("The directory could not be found!");
                        }
                    } while (!Directory.Exists(InputDirectory));

                    //Output Path
                    Console.WriteLine();
                    Console.WriteLine("Output Path:");
                    Console.WriteLine("Specify a directory where the textures should be saved.");
                    do
                    {
                        OutputDirectory = Console.ReadLine().Trim('"');
                        if (!PathIsValid(OutputDirectory))
                        {
                            Console.WriteLine("Path is invalid!");
                        }

                    } while (!PathIsValid(OutputDirectory));

                    //Options
                    Console.WriteLine();
                    PrintOptions(options);

                    Console.WriteLine();
                    Console.WriteLine("Adjust settings? \tYes or (No)");
                    if (ConsoleReadBool(false))
                    {
                        Console.WriteLine($"Extract mipmaps. \t(True) or False");
                        options.Mips = ConsoleReadBool(true);
                        Console.WriteLine($"Extracts all raw images that are found. \tTrue or (False)");
                        options.Raw = ConsoleReadBool(false);
                        Console.WriteLine($"Tries to extract textures from unknown files, may cause errors. \tTrue or (False)");
                        options.Force = ConsoleReadBool(false);
                    }

                    //Inputs correct?
                    Console.WriteLine();
                    Console.WriteLine($"Input Path: \"{InputDirectory}\"");
                    Console.WriteLine($"Output Path: \"{OutputDirectory}\"");
                    PrintOptions(options);

                    Console.WriteLine();
                    Console.WriteLine("Are the settings correct? \t(Yes) or No");
                    if (!ConsoleReadBool(true)) continue;

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

                switch (args[0].ToLower())
                {
                    case "extract":
                    case "e":
                        #region extract
                        if (args.Length < 3)
                        {
                            // Wrong syntax
                            goto default;
                        }

                        InputDirectory = args[1];
                        OutputDirectory = args[2];
                        if (!Directory.Exists(InputDirectory))
                        {
                            Console.WriteLine("The directory could not be found!");
                            Console.WriteLine(InputDirectory);
                            return;
                        }
                        if (!PathIsValid(OutputDirectory))
                        {
                            Console.WriteLine("Path is invalid!");
                            Console.WriteLine(OutputDirectory);
                            return;
                        }

                        options = new TextureExtractor.Options() { Mips = false, Raw = false, Force = false };

                        if (args.Length > 3)
                        {
                            for (int i = 3; i < args.Length; i++)
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
                        Console.WriteLine("Wrong syntax. use h for help");
                        break;
                }
            }
        }



        static bool ConsoleReadBool(bool defaultvalue)
        {
            while (true)
            {
                switch (Console.ReadLine().ToLower())
                {
                    case "t":
                    case "true":
                    case "y":
                    case "yes":
                        return true;
                    case "f":
                    case "false":
                    case "n":
                    case "no":
                        return false;
                    case "":
                        return defaultvalue;
                }
            }
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
            Console.WriteLine("".PadLeft(64, '-'));
            Console.WriteLine($"{System.Diagnostics.Process.GetCurrentProcess().ProcessName} v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}  {DateTime.Now.ToString()}");
            Console.WriteLine("Supported formats: arc, szs, szp, cpk, bdl, bmd, tpl, bti");
            Console.WriteLine("".PadLeft(64, '-'));
            Console.WriteLine("INFO: currently no ROM images are supported, Please unpack them with dolphin into a folder.");
            Console.WriteLine("right click on a game -> Properties -> Filesystem -> right click on \"Disc - [Game ID]\" -> Extract Files...");
            Console.WriteLine();
        }

        static void PrintOptions(TextureExtractor.Options options)
        {
            Console.WriteLine($"Extracts Mipmaps: {options.Mips}");
            Console.WriteLine($"Extracts Raw Image fiels: {options.Raw}");
            Console.WriteLine($"Extract textures from unknown files: {options.Force}");
        }

        static void PrintResult(TextureExtractor.Result result)
        {
            Console.WriteLine("".PadLeft(64, '-'));
            Console.WriteLine($"Extracted textures: {result.Extracted}");
            Console.WriteLine($"Unsupported files: {result.Unsupported}");
            Console.WriteLine($"Unknown files: {result.Unknown}");
            Console.WriteLine($"Extraction rate: ~{result.ExtractionRate}%");
            Console.WriteLine($"Scan time: {result.TotalTime.TotalSeconds}s");
            Console.WriteLine($"Log saved: \"{result.LogFullPath}\"");
            Console.WriteLine("".PadLeft(64, '-'));
        }
    }
}
