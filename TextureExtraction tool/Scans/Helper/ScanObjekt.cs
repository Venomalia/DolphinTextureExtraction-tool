using AuroraLib.Archives;
using AuroraLib.Common;

namespace DolphinTextureExtraction.Scans.Helper
{
    public readonly ref struct ScanObjekt
    {
        public Stream Stream { get; }
        public FormatInfo Format { get; }
        public ReadOnlySpan<char> SubPath { get; }
        public int Deep { get; }
        public ReadOnlySpan<char> Extension { get; }
        public ArchiveFile File { get; }

        public ScanObjekt(Stream stream, ReadOnlySpan<char> subPath, int deep = 0, ReadOnlySpan<char> extension = default)
        {
            Stream = stream;
            Extension = extension;
            Format = stream.Identify(Extension);
            SubPath = PathX.GetFileWithoutExtension(subPath);
            Deep = deep;
            File = null;
        }

        public ScanObjekt(ArchiveFile file, ReadOnlySpan<char> subPath, int deep)
        {
            Stream = file.FileData;
            Extension = file.Extension;
            Format = Stream.Identify(Extension);
            SubPath = PathX.GetFileWithoutExtension(subPath);
            Deep = deep;
            File = file;
        }

        public string GetFullSubPath()
         => string.Concat(SubPath, Extension);
    }
}
