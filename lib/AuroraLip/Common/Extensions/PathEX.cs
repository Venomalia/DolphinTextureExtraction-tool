using System;
using System.IO;

namespace AuroraLip.Common.Extensions
{
    public static class PathEX
    {
        public const char ExtensionSeparatorChar = '.';

        public static ReadOnlySpan<char> GetRelativePath(in ReadOnlySpan<char> path, in ReadOnlySpan<char> MainPath)
        {
            if (path.StartsWith(MainPath))
                return path.Slice(MainPath.Length).TrimStart(Path.DirectorySeparatorChar);
            return path;
        }

        public static ReadOnlySpan<char> WithoutExtension(in ReadOnlySpan<char> path)
        {
            if (path.LastIndexOf(ExtensionSeparatorChar) > path.LastIndexOf(Path.DirectorySeparatorChar))
                return path.Slice(0, path.LastIndexOf(ExtensionSeparatorChar));
            return path;
        }

        public static ReadOnlySpan<char> GetExtension(in ReadOnlySpan<char> path)
        {
            if (path.LastIndexOf(ExtensionSeparatorChar) > path.LastIndexOf(Path.DirectorySeparatorChar))
                return path.Slice(path.LastIndexOf(ExtensionSeparatorChar));
            return string.Empty.AsSpan();
        }

        public static bool CheckInvalidPathChars(in ReadOnlySpan<char> path)
            => path.IndexOfAny(Path.GetInvalidPathChars()) == 0;

    }
}
