using System.Text;
using System.Text.RegularExpressions;

namespace AuroraLib.Common.Extensions
{
    public static class PathEX
    {
        public const char ExtensionSeparatorChar = '.';

        public static ReadOnlySpan<char> GetRelativePath(in ReadOnlySpan<char> path, in ReadOnlySpan<char> MainPath)
        {
            if (path.StartsWith(MainPath))
                return path[MainPath.Length..].TrimStart(Path.DirectorySeparatorChar);
            return path;
        }

        public static ReadOnlySpan<char> WithoutExtension(in ReadOnlySpan<char> path)
        {
            if (path.LastIndexOf(ExtensionSeparatorChar) > path.LastIndexOf(Path.DirectorySeparatorChar))
                return path[..path.LastIndexOf(ExtensionSeparatorChar)];
            return path;
        }

        public static bool CheckInvalidPathChars(in ReadOnlySpan<char> path)
            => path.IndexOfAny(Path.GetInvalidPathChars()) == 0;

        internal static readonly Regex illegalChars = new(@"^(.*(//|\\))?(?'X'PRN|AUX|CLOCK\$|NUL|CON|COM\d|LPT\d|\..*| )((//|\\).*)?$|[\x00-\x1f\x7F?*:""<>|]| ((//|\\).*)?$", RegexOptions.CultureInvariant);

        public static string GetValidPath(string path)
        {
            Match match = illegalChars.Match(path);
            while (match.Success)
            {
                int index = match.Index;
                if (match.Groups["X"].Success)
                    index = match.Groups["X"].Index;
                char llegalChar = path[index];
                path = path.Remove(index, 1);
                path = path.Insert(index, $"{(byte)llegalChar:X2}");
                match = illegalChars.Match(path);
            }
            return path.TrimEnd(' ', '\\', '/');
        }
    }
}
