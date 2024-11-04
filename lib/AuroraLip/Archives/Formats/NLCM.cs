using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;
using System.Text;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Rune Factory (Frontier) archive format
    /// </summary>
    // Cross-referenced with https://github.com/master801/Rune-Factory-Frontier-Tools
    public sealed  class NLCM : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("NLCM");

        private Stream reference_stream;

        public NLCM()
        {
        }

        public NLCM(string name) : base(name)
        {
        }

        public NLCM(FileNode source) : base(source)
        {
        }


        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);
            uint table_offset = source.ReadUInt32(Endian.Big);
            uint unknown2 = source.ReadUInt32(Endian.Big);
            uint file_count = source.ReadUInt32(Endian.Big);
            uint unknown3 = source.ReadUInt32(Endian.Big);
            string reference_file = source.ReadCString();
            source.Seek(table_offset, SeekOrigin.Begin);

            //try to request an external file.
            if (Parent is not null && Parent.TryGet(reference_file, out ObjectNode refNode) && refNode is FileNode refFile)
            {
                reference_stream = refFile.Data;
                for (int i = 0; i < file_count; i++)
                {
                    uint size = source.ReadUInt32(Endian.Big);
                    uint padding = source.ReadUInt32(Endian.Big);
                    uint file_offset = source.ReadUInt32(Endian.Big);
                    uint padding2 = source.ReadUInt32(Endian.Big);
                    string name = GetName(reference_stream, file_offset, size, i);
                    FileNode Sub = new(name, new SubStream(reference_stream, size, file_offset));
                    Add(Sub);
                }
            }
            else
            {
                throw new Exception($"{nameof(NLCM)}: could not request the file {reference_file}.");
            }
        }

        internal static string GetName(Stream stream, uint offset, uint size, int index)
        {
            stream.Seek(offset, SeekOrigin.Begin);
            if (stream.Read<ushort>() == 22600)
            {
                if (stream.Read<ushort>() == 16980)
                {
                    stream.Seek(offset + size - 0x20, SeekOrigin.Begin);
                    return stream.ReadString(0x20) + '_' + index;
                }
                else
                {
                    stream.Seek(offset + size - 0x30, SeekOrigin.Begin);
                    return ReadHXString(stream) + '_' + index;
                }
            }
            return index.ToString();
        }

        internal static string ReadHXString(Stream stream)
        {
            Span<byte> bytes = stackalloc byte[0x30];
            stream.Read(bytes);

            int start;
            int end;
            for (end = 0x30 - 1; end >= 0; end--)
            {
                if (bytes[end] != 0)
                    break;
            }
            for (start = end; start >= 0; start--)
            {
                if (bytes[start] < 0x30 || bytes[start] > 0x7A)//58-64
                    break;
            }
            return Encoding.ASCII.GetString(bytes[(start + 1)..(end + 1)]);
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

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
    }
}
