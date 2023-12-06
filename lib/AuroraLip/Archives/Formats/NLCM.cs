using AuroraLib.Common;
using AuroraLib.Core.Interfaces;
using System.Text;

namespace AuroraLib.Archives.Formats
{
    // Rune Factory (Frontier) archive format
    // Cross-referenced with https://github.com/master801/Rune-Factory-Frontier-Tools
    public class NLCM : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("NLCM");

        private Stream reference_stream;

        public NLCM()
        { }

        public NLCM(string filename) : base(filename)
        {
        }

        public NLCM(Stream stream, string filename = null) : base(stream, filename)
        {
        }

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

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);
            uint table_offset = stream.ReadUInt32(Endian.Big);
            uint unknown2 = stream.ReadUInt32(Endian.Big);
            uint file_count = stream.ReadUInt32(Endian.Big);
            uint unknown3 = stream.ReadUInt32(Endian.Big);
            string reference_file = stream.ReadString();
            stream.Seek(table_offset, SeekOrigin.Begin);

            //try to request an external file.
            try
            {
                reference_stream = FileRequest.Invoke(reference_file);
            }
            catch (Exception)
            {
                throw new Exception($"{nameof(NLCM)}: could not request the file {reference_file}.");
            }

            Root = new ArchiveDirectory() { OwnerArchive = this };
            for (int i = 0; i < file_count; i++)
            {
                uint size = stream.ReadUInt32(Endian.Big);
                uint padding = stream.ReadUInt32(Endian.Big);
                uint file_offset = stream.ReadUInt32(Endian.Big);
                uint padding2 = stream.ReadUInt32(Endian.Big);
                string name = GetName(reference_stream, file_offset, size, i);
                Root.AddArchiveFile(reference_stream, size, file_offset, name);
            }
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
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
    }
}
