namespace DolphinTextureExtraction
{
    internal enum Options
    {
        Force,
        Dryrun,
        Progress,
        Tasks,
        Mip,
        Raw,
        Cleanup,
        Dolphinmipdetection,
        ArbitraryMipmapDetection,
        Recursiv,
        Log,
        CombinedRGBA,
    }

    internal static class OptionsEX
    {
        public static ReadOnlySpan<char> GetDescription(this Options mode) => mode switch
        {
            Options.Force => "Tries to extract unknown files, may cause errors!",
            Options.Dryrun => "Test run, only creates a log file.",
            Options.Progress => "Progress report typ.",
            Options.Tasks => "Maximum number of concurrent tasks.",
            Options.Mip => "Extract mipmaps",
            Options.Raw => "Extracts raw image files.",
            Options.Cleanup => "Folder cleanup typ.",
            Options.Dolphinmipdetection => "Tries to imitate dolphin mipmap detection.",
            Options.Recursiv => "How far to extract Recursive, 0 for infinity.",
            Options.ArbitraryMipmapDetection => "use Arbitrary Mipmap Detection.",
            Options.Log => "Defines the directory in which the log file will be saved.",
            Options.CombinedRGBA => "Combine IA8 palette texture pairs into one RGBA texture.",
            _ => throw new NotImplementedException(),
        };

        public static ReadOnlySpan<char> GetAlias(this Options mode) => mode switch
        {
            Options.Force => "f",
            Options.Dryrun => "d",
            Options.Progress => "p",
            Options.Tasks => "t",
            Options.Mip => "m",
            Options.Raw => "r",
            Options.Cleanup => "c",
            Options.Dolphinmipdetection => "dmd",
            Options.Recursiv => "rc",
            Options.ArbitraryMipmapDetection => "amd",
            Options.Log => "l",
            Options.CombinedRGBA => "rgba",
            _ => throw new NotImplementedException(),
        };


        public static ReadOnlySpan<char> GetSyntax(this Options mode) => mode switch
        {
            Options.Force => string.Empty,
            Options.Dryrun => string.Empty,
            Options.Progress => $"{Options.Progress}:Mode",
            Options.Tasks => $"{Options.Tasks} \"i\"",
            Options.Mip => string.Empty,
            Options.Raw => string.Empty,
            Options.Cleanup => $"{Options.Cleanup}:Mode",
            Options.Dolphinmipdetection => string.Empty,
            Options.Recursiv => $"{Options.Recursiv} \"i\"",
            Options.ArbitraryMipmapDetection => string.Empty,
            Options.Log => $"{Options.Log} \"path\"",
            Options.CombinedRGBA => string.Empty,
            _ => throw new NotImplementedException(),
        };

        public static bool TryParse(ReadOnlySpan<char> name, out Options Option)
        {
            if (Enum.TryParse(name, true, out Option))
                return true;

            foreach (var m in Enum.GetValues<Options>())
            {
                ReadOnlySpan<char> alias = m.GetAlias();
                if (name.SequenceEqual(alias))
                {
                    Option = m;
                    return true;
                }
            }

            return false;
        }
    }
}
