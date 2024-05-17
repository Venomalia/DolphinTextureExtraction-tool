using AuroraLib.Common.Node;
using AuroraLib.Core.Buffers;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Square Enix Crystal Bearers Archive Header
    /// </summary>
    public sealed class POSD : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("POSD");

        public POSD()
        {
        }

        public POSD(string name) : base(name)
        {
        }

        public POSD(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            //try to request an external file.

            string datname = Path.GetFileNameWithoutExtension(Name) + ".dat";
            if (TryGetRefFile(datname, out FileNode refFile))
            {
                reference_stream = refFile.Data;

                source.MatchThrow(_identifier);

                uint dir_count = source.ReadUInt32(Endian.Big);

                using SpanBuffer<FolderEntry> folders = new(dir_count);
                source.Read<FolderEntry>(folders, Endian.Big);
                byte[] pedding = source.Read(8);

                for (int i = 0; i < dir_count; i++)
                {
                    DirectoryNode directory = new($"dir_{i}");
                    Add(directory);

                    for (int f = 0; f < folders[f].FileCount; f++)
                    {
                        FieleEntry file = source.Read<FieleEntry>(Endian.Big);
                        directory.Add(new FileNode($"file_{f}", new SubStream(reference_stream, file.Size, file.Offset)));
                    }
                }
            }
            else
            {
                throw new Exception($"{nameof(POSD)}: could not request the file {datname}.");
            }
        }

        private struct FolderEntry
        {
            public int Unk;
            public uint FileCount;
        }

        private struct FieleEntry
        {
            private uint offset;
            public uint Size;

            public readonly long Offset => offset << 11;
        }

        private Stream reference_stream;


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (reference_stream != null)
                {
                    reference_stream.Dispose();
                }
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();
    }
}
