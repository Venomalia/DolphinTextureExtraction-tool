using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
{
    // Format references https://github.com/intns/MODConv. For now, the textures are enough for us.

    /// <summary>
    /// Pikmin model archive.
    /// </summary>
    public class MOD : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public static string Extension => ".mod";

        public MOD()
        { }

        public MOD(string filename) : base(filename)
        {
        }

        public MOD(Stream stream, string filename = null) : base(stream, filename)
        {
        }

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.Contains(Extension, StringComparison.InvariantCultureIgnoreCase) && stream.ReadInt32(Endian.Big) == ((int)ChunkTyps.Header);

        protected override void Read(Stream stream)
        {
            Root = new ArchiveDirectory() { OwnerArchive = this };

            while (stream.Position < stream.Length)
            {
                if ((stream.Position & 0x1F) != 0)
                    throw new Exception($"Chunk start ({stream.Position}) not on boundary!");

                ChunkTyps opcode = (ChunkTyps)stream.ReadInt32(Endian.Big);
                int lengthOfStruct = stream.ReadInt32(Endian.Big);
                long StartOfStruct = stream.Position;

                switch (opcode)
                {
                    case ChunkTyps.Texture:
                        ReadTextureChunk(stream, lengthOfStruct, StartOfStruct);
                        break;

                    case ChunkTyps.TextureAttribute:
                        ReadTextureAttributeChunk(stream, lengthOfStruct, StartOfStruct);
                        break;

                    case ChunkTyps.EoF: //End of binary data Start of INI data
                        stream.Seek(lengthOfStruct, SeekOrigin.Current);
                        StartOfStruct = stream.Position;
                        lengthOfStruct = (int)(stream.Length - stream.Position);
                        Root.AddArchiveFile(stream, lengthOfStruct, "info.ini");
                        break;

                    default:
                        Root.AddArchiveFile(stream, lengthOfStruct, opcode.ToString() + ".bin");
                        break;
                }

                // Read the file, move on to the next one
                stream.Seek(StartOfStruct + lengthOfStruct, SeekOrigin.Begin);
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        private void ReadTextureAttributeChunk(Stream stream, in int lengthOfStruct, in long StartOfStruct)
        {
            int NumberOfFiles = stream.ReadInt32(Endian.Big);
            stream.Seek(20, SeekOrigin.Current);

            for (int i = 1; i <= NumberOfFiles; i++)
            {
                Root.AddArchiveFile(stream, 12, ChunkTyps.TextureAttribute.ToString() + i + ".attr");
                stream.Seek(20, SeekOrigin.Current);
            }
        }

        private void ReadTextureChunk(Stream stream, in int lengthOfStruct, in long StartOfStruct)
        {
            int NumberOfFiles = stream.ReadInt32(Endian.Big);
            stream.Seek(20, SeekOrigin.Current);

            for (int i = 1; i <= NumberOfFiles; i++)
            {
                long StartOfEntry = stream.Position;
                stream.Seek(28, SeekOrigin.Current);
                int DataSize = stream.ReadInt32(Endian.Big);
                Root.AddArchiveFile(stream, 32 + DataSize, StartOfEntry, ChunkTyps.Texture.ToString() + i + ".txe");
                stream.Seek(DataSize, SeekOrigin.Current);
            }
        }

        public enum ChunkTyps : int
        {
            Header,

            VertexPosition = 0x0010,
            VertexNormal = 0x0011,
            VertexNBT = 0x0012,
            VertexColor = 0x0013,

            UVMap0 = 0x0018,
            UVMap1 = 0x0019,
            UVMap2 = 0x001A,
            UVMap3 = 0x001B,
            UVMap4 = 0x001C,
            UVMap5 = 0x001D,
            UVMap6 = 0x001E,
            UVMap7 = 0x001F,

            Texture = 0x0020,
            TextureAttribute = 0x0022,

            Material = 0x0030,
            Matrix = 0x0040,
            MatrixEnvelope = 0x0041,
            Mesh = 0x0050,
            Joint = 0x0060,
            JointName = 0x0061,
            Unknown = 0x0080,
            CollisionPrism = 0x0100,
            CollisionGrid = 0x0110,

            EoF = 0xFFFF
        }
    }
}
