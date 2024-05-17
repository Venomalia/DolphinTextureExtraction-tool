using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Buffers;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Sting Entertainment PAC Archive.
    /// </summary>
    public sealed class PAC : ArchiveNode
    {
        public override bool CanWrite => false;

        public PAC()
        {
        }

        public PAC(string name) : base(name)
        {
        }

        public PAC(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.SequenceEqual(".PAC") && stream.Length > 52428800;

        protected override void Deserialize(Stream source)
        {
            //try to request an external file.
            string datname = Path.GetFileNameWithoutExtension(Name);
            if (TryGetRefFile(datname + ".PAH", out FileNode refFile))
            {
                using Stream streamPAH = refFile.Data;
                uint tabelEntrys = streamPAH.Read<uint>();
                uint tabelOffset = streamPAH.Read<uint>();
                uint tabelEnd = streamPAH.Read<uint>();
                // Unknown values from 0x7 to 0x70
                streamPAH.Seek(tabelOffset, SeekOrigin.Begin);
                using SpanBuffer<PAHFileEntry> entries = new((int)tabelEntrys);
                streamPAH.Read(entries.Span);
                foreach (var entry in entries)
                {
                    streamPAH.Seek(entry.NameOffset, SeekOrigin.Begin);
                    string name = streamPAH.ReadString();
                    FileNode file = new(name, new SubStream(source, entry.Size, entry.Offset));
                    Add(file);
                }
            }
            else
            {
                throw new Exception($"{nameof(PAC)}: could not request the file {datname}.");
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        private struct PAHFileEntry
        {
            public uint Offset;
            public uint Size;
            public uint Null;
            public uint NameOffset;
        }
    }
}
