using AuroraLib.Common;
using AuroraLib.DiscImage.Dolphin;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Nintendo Gamecube Mini Disc Image.
    /// </summary>
    public class GCDisk : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public GameHeader Header;

        public GCDisk()
        { }

        public GCDisk(string filename) : base(filename)
        {
        }

        public GCDisk(Stream stream, string fullpath = null) : base(stream, fullpath)
        {
        }

        public virtual bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 9280 && extension.Contains(".iso", StringComparison.InvariantCultureIgnoreCase) && new BootBin(stream).DiskType == DiskTypes.GC;

        protected override void Read(Stream stream) => Root = ProcessData(stream);

        protected ArchiveDirectory ProcessData(Stream stream)
        {
            BootBin boot = new(stream);
            Header ??= boot;

            // get appLoaderDate
            stream.Seek(9280, SeekOrigin.Begin);
            ReadOnlySpan<char> appLoaderDate = stream.ReadString(10).AsSpan();
            DateTime appDate = new(int.Parse(appLoaderDate[..4].ToString()), int.Parse(appLoaderDate.Slice(5, 2).ToString()), int.Parse(appLoaderDate.Slice(8, 2).ToString()));

            ArchiveDirectory RootDirectory = new() { Name = $"{boot.GameName} [{boot.GameID}]", OwnerArchive = this, LastAccessTimeUtc = appDate, CreationTimeUtc = appDate };
            ArchiveDirectory SysDirectory = new(RootDirectory.OwnerArchive, RootDirectory) { Name = "sys", LastAccessTimeUtc = appDate, CreationTimeUtc = appDate };
            RootDirectory.Items.Add(SysDirectory.Name, SysDirectory);

            SysDirectory.AddArchiveFile(stream, 1088, 0, "boot.bin");
            SysDirectory.AddArchiveFile(stream, 8192, 1088, "bi2.bin");
            SysDirectory.AddArchiveFile(stream, boot.DolOffset - 9280, 9280, "apploader.img");
            SysDirectory.AddArchiveFile(stream, boot.FSTableOffset - boot.DolOffset, boot.DolOffset, "main.dol");
            SysDirectory.AddArchiveFile(stream, boot.FSTableSize, boot.FSTableOffset, "fst.bin");

            //FileSystemTable (fst.bin)
            ArchiveDirectory filedir = new(RootDirectory.OwnerArchive, RootDirectory) { Name = "files", LastAccessTimeUtc = appDate, CreationTimeUtc = appDate };
            RootDirectory.Items.Add(filedir.Name, filedir);
            stream.Seek(boot.FSTableOffset, SeekOrigin.Begin);
            FSTBin FST = new(stream, boot.DiskType == DiskTypes.GC);
            FST.ProcessEntres(stream, filedir);
            return RootDirectory;
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }
    }
}
