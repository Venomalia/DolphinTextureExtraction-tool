using AuroraLib.Common;
using AuroraLib.Common.Node;

namespace DolphinTextureExtraction.Scans.Helper
{
    public readonly ref struct ScanObjekt
    {
        public FormatInfo Format { get; }
        public int Deep { get; }
        public FileNode File { get; }

        public ScanObjekt(FileNode file, int deep)
        {
            Format = file.Data.Identify(file.Extension);
            Deep = deep;
            File = file;
        }
    }
}
