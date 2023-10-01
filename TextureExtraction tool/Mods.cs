namespace DolphinTextureExtraction
{
    internal enum Modes : byte
    {
        Help,
        Extract,
        Unpack,
        Compress,
        Split,
        Formats,
        Finalize,
    }

    internal static class ModesEX
    {
        public static ReadOnlySpan<char> GetDescription(this Modes mode) => mode switch
        {
            Modes.Help => "Displays a list with all commands.",
            Modes.Extract => "Extracts textures with Dolphins texture hash.",
            Modes.Unpack => "Unpacks all files.",
            Modes.Compress => "Compress files.",
            Modes.Split => "Splits files by identifier.",
            Modes.Formats => "Displays known formats.",
            Modes.Finalize => "Separates combined RGBA textures & fixes texture issues.",
            _ => throw new NotImplementedException(),
        };

        public static ReadOnlySpan<char> GetAlias(this Modes mode) => mode switch
        {
            Modes.Help => "h",
            Modes.Extract => "e",
            Modes.Unpack => "u",
            Modes.Compress => "c",
            Modes.Split => "s",
            Modes.Formats => "f",
            Modes.Finalize => "z",
            _ => throw new NotImplementedException(),
        };

        public static ReadOnlySpan<Options> GetOptions(this Modes mode) => mode switch
        {
            Modes.Help => Array.Empty<Options>(),
            Modes.Extract => new[] { Options.Force, Options.Tasks, Options.Dryrun, Options.Progress, Options.Mip, Options.Raw, Options.Cleanup, Options.Dolphinmipdetection, Options.ArbitraryMipmapDetection, Options.CombinedRGBA, Options.Log },
            Modes.Unpack => new[] { Options.Force, Options.Tasks, Options.Dryrun, Options.Progress, Options.Recursiv, Options.Log },
            Modes.Compress => new[] { Options.Tasks, Options.Progress, Options.Log },
            Modes.Split => new[] { Options.Tasks, Options.Dryrun, Options.Progress, Options.Log },
            Modes.Formats => Array.Empty<Options>(),
            Modes.Finalize => Array.Empty<Options>(),
            _ => throw new NotImplementedException(),
        };

        public static ReadOnlySpan<char> GetSyntax(this Modes mode) => mode switch
        {
            Modes.Help => string.Empty,
            Modes.Extract => $"{Modes.Extract} \"Input\" \"Output\" options",
            Modes.Unpack => $"{Modes.Unpack} \"Input\" \"Output\" options",
            Modes.Compress => $"{Modes.Compress} \"Input\" \"Output\" Algorithm",
            Modes.Split => $"{Modes.Split} \"Input\" \"Output\" Patterns",
            Modes.Formats => string.Empty,
            Modes.Finalize => $"{Modes.Finalize} \"Input\" \"Output\"",
            _ => throw new NotImplementedException(),
        };

        public static void PrintModes()
        {
            foreach (var mode in Enum.GetValues<Modes>())
            {
                ConsoleEx.WriteColoured(((int)mode).ToString(), ConsoleColor.Red);
                Console.WriteLine($".\t {GetDescription(mode)}");
            }
        }

        public static Modes SelectMode()
        {
            do
            {
                var key = Console.ReadKey().KeyChar;
                Console.CursorLeft = 0;
                foreach (var mode in Enum.GetValues<Modes>())
                {
                    if (Int32.TryParse(key.ToString(), out int v) && (int)mode == v)
                    {
                        return mode;
                    }
                }
            } while (true);
        }

        public static bool TryParse(ReadOnlySpan<char> name, out Modes mode)
        {
            if (Enum.TryParse(name, true, out mode))
                return true;

            foreach (var m in Enum.GetValues<Modes>())
            {
                ReadOnlySpan<char> alias = m.GetAlias();
                if (name.SequenceEqual(alias))
                {
                    mode = m;
                    return true;
                }
            }

            return false;
        }
    }
}
