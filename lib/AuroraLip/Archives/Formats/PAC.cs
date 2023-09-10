using AuroraLib.Common;
using AuroraLib.Core.Buffers;

namespace AuroraLib.Archives.Formats
{
    public class PAC : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        private const string Extension = ".PAC";

        public bool IsMatch(Stream stream, in string extension = "")
            => extension == Extension && stream.Length > 52428800;

        protected override void Read(Stream stream)
        {
            //try to request an external file.
            Stream streamPAH;
            string datname = Path.ChangeExtension(Path.GetFileNameWithoutExtension(FullPath), ".PAH");
            try
            {
                streamPAH = FileRequest.Invoke(datname);
            }
            catch (Exception)
            {
                throw new Exception($"{nameof(PAC)}: could not request the file {datname}.");
            }
            Root = new ArchiveDirectory() { OwnerArchive = this };

            uint tabelEntrys = streamPAH.Read<uint>();
            uint tabelOffset = streamPAH.Read<uint>();
            uint tabelEnd = streamPAH.Read<uint>();
            // Unknown values from 0x7 to 0x70
            streamPAH.Seek(tabelOffset, SeekOrigin.Begin);
            SpanBuffer<PAHFileEntry> entries = new((int)tabelEntrys);
            streamPAH.Read(entries.Span);
            foreach (var entry in entries)
            {
                streamPAH.Seek(entry.NameOffset, SeekOrigin.Begin);
                string name = streamPAH.ReadString();
                stream.Seek(entry.Offset, SeekOrigin.Begin);
                Root.AddArchiveFile(stream, entry.Size, entry.Offset, name);

            }
            streamPAH.Close();
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        private struct PAHFileEntry
        {
            public uint Offset;
            public uint Size;
            public uint Null;
            public uint NameOffset;
        }
    }
}
