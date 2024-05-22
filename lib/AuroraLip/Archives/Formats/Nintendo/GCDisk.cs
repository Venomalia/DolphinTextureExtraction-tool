using AuroraLib.Common.Node;
using AuroraLib.DiscImage.Dolphin;

namespace AuroraLib.Archives.Formats.Nintendo
{
    /// <summary>
    /// GameCube Game Disc (DOL-006) ISO Image.
    /// </summary>
    public class GCDisk : ArchiveNode
    {
        public override bool CanWrite => false;

        public GameID GameID => Header.GameID;

        public string GameName => Header.GameName;

        public GameHeader Header { get => header; set => header = value; }
        protected GameHeader header;

        public GCDisk()
        { }

        public GCDisk(string name) : base(name)
        { }

        public GCDisk(FileNode source) : base(source)
        { }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 9280 && extension.Contains(".iso", StringComparison.InvariantCultureIgnoreCase) && new BootBin(stream).DiskType == DiskTypes.GC;


        protected override void Deserialize(Stream source)
        {
            ProcessData(source, this);
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        protected void ProcessData(Stream source, DirectoryNode RootDirectory)
        {
            BootBin boot = new(source);
            if (Header == null)
            {
                Header = boot;
                RootDirectory.Name = $"{boot.GameName} [{boot.GameID}]";
            }

            // get appLoaderDate
            source.Seek(9280, SeekOrigin.Begin);
            ReadOnlySpan<char> appLoaderDate = source.ReadString(10).AsSpan();
            DateTime appDate = new(int.Parse(appLoaderDate[..4].ToString()), int.Parse(appLoaderDate.Slice(5, 2).ToString()), int.Parse(appLoaderDate.Slice(8, 2).ToString()));
            RootDirectory.CreationTimeUtc = appDate;

            DirectoryNode SysDirectory = new("sys") { CreationTimeUtc = appDate };
            RootDirectory.Add(SysDirectory);

            SysDirectory.Add(new FileNode("boot.bin", new SubStream(source, 1088, 0)) { CreationTimeUtc = appDate });
            SysDirectory.Add(new FileNode("bi2.bin", new SubStream(source, 8192, 1088)) { CreationTimeUtc = appDate });
            SysDirectory.Add(new FileNode("apploader.img", new SubStream(source, boot.DolOffset - 9280, 9280)) { CreationTimeUtc = appDate });
            SysDirectory.Add(new FileNode("main.dol", new SubStream(source, boot.FSTableOffset - boot.DolOffset, boot.DolOffset)) { CreationTimeUtc = appDate });
            SysDirectory.Add(new FileNode("fst.bin", new SubStream(source, boot.FSTableSize, boot.FSTableOffset)) { CreationTimeUtc = appDate });

            //FileSystemTable (fst.bin)
            DirectoryNode filedir = new("files") { LastAccessTimeUtc = appDate, CreationTimeUtc = appDate };
            RootDirectory.Add(filedir);
            source.Seek(boot.FSTableOffset, SeekOrigin.Begin);
            FSTBin FST = new(source, boot.DiskType == DiskTypes.GC);
            FST.ProcessEntres(source, filedir);
        }
    }
}
