using AuroraLib.Common;
using AuroraLib.Compression;
using AuroraLib.Compression.Formats;

namespace AuroraLib.Compression.Formats
{
    public class ZLB : ICompression, IMagicIdentify
    {

        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        public const string magic = "ZLB";

        private static readonly ZLib ZLipInstance = new();

        public void Compress(in byte[] source, Stream destination)
            => throw new NotImplementedException();

        public byte[] Decompress(Stream source)
        {
            MemoryStream buffer = new();
            do
            {
                if (!IsMatch(source))
                {
                    if (!source.Search(magic))
                    {
                        break;
                    }
                    continue;
                }
                Header header = source.Read<Header>(Endian.Big);

                ZLipInstance.Decompress(source.Read((int)header.CompLength), buffer);
                source.Align(32);
            } while (source.Position + 16 < source.Length);
            return buffer.ToArray();
        }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic) && stream.ReadUInt8() == 0;

        public unsafe struct Header
        {
            public uint Version;
            public uint DeLength;
            public uint CompLength;
        }

    }
}
