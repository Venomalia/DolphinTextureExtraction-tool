using AuroraLib.Archives;
using AuroraLib.Common;
using AuroraLip.Compression.Formats;

namespace AuroraLip.Archives.Formats
{
    public class ShrekDir : Archive, IFileAccess
    {

        public bool CanRead => true;

        public bool CanWrite => false;

        public const string Extension = ".DIR";

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
        {
            if (extension == Extension)
            {
                Endian endian = stream.DetectByteOrder<uint>();
                uint firstOffset = stream.ReadUInt32(endian);
                stream.Position = firstOffset - 8;
                uint lastOffset = stream.ReadUInt32(endian);
                uint end = stream.ReadUInt32(endian); // 0
                stream.Position = lastOffset;
                Entry entry = new(stream, endian);
                return stream.Position == stream.Length && end == 0;
            }
            return false;
        }

        protected override void Read(Stream stream)
        {
            //try to request an external file.
            string datname = Path.ChangeExtension(Path.GetFileNameWithoutExtension(FullPath), ".DAT");
            try
            {
                reference_stream = FileRequest.Invoke(datname);
            }
            catch (Exception)
            {
                throw new Exception($"{nameof(ShrekDir)}: could not request the file {datname}.");
            }
            Root = new ArchiveDirectory() { OwnerArchive = this };

            Endian endian = stream.DetectByteOrder<uint>();
            //Starts with a pointer list, last entry ends with 0x0
            uint firstOffset = stream.ReadUInt32(endian);

            uint files = (firstOffset - 8) / 4;
            stream.Position = firstOffset;
            for (int i = 0; i < files; i++)
            {
                Entry entry = new(stream, endian);
                if (!Root.Items.ContainsKey(entry.Name))
                {
                    MemoryStream decomp = new(Shrek.Decompress_ALG(reference_stream.At(entry.Offset, s => s.Read((int)entry.CompSize)), (int)entry.DecompSize).ToArray());
                    Root.AddArchiveFile(decomp, entry.Name);
                }
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        private struct Entry
        {
            public uint Offset;
            public uint DecompSize;
            public uint CompSize;
            public string Name;

            public Entry(Stream stream, Endian endian)
            {
                Offset = stream.ReadUInt32(endian);
                DecompSize = stream.ReadUInt32(endian);
                CompSize = stream.ReadUInt32(endian);
                Name = stream.ReadString();
                stream.Align(4);
            }
        }

        private Stream reference_stream;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                reference_stream?.Dispose();
            }
        }
    }
}
