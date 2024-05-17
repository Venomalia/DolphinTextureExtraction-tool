using AuroraLib.Common;
using AuroraLib.Common.Node;
using System.IO;

namespace AuroraLib.Archives.Formats
{
    // Format references https://github.com/intns/MODConv. For now, the textures are enough for us.

    /// <summary>
    /// Pikmin 1 model archive.
    /// </summary>
    public sealed class MOD : ArchiveNode
    {
        public override bool CanWrite => false;

        public MOD()
        {
        }

        public MOD(string name) : base(name)
        {
        }

        public MOD(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.Contains(".mod", StringComparison.InvariantCultureIgnoreCase) && stream.ReadInt32(Endian.Big) == ((int)ChunkTyps.Header);

        protected override void Deserialize(Stream source)
        {
            while (source.Position < source.Length)
            {
                if ((source.Position & 0x1F) != 0)
                    throw new Exception($"Chunk start ({source.Position}) not on boundary!");

                ChunkTyps opcode = (ChunkTyps)source.ReadInt32(Endian.Big);
                int lengthOfStruct = source.ReadInt32(Endian.Big);
                long StartOfStruct = source.Position;

                switch (opcode)
                {
                    case ChunkTyps.Texture:
                        ReadTextureChunk(source, lengthOfStruct, StartOfStruct);
                        break;

                    case ChunkTyps.TextureAttribute:
                        ReadTextureAttributeChunk(source, lengthOfStruct, StartOfStruct);
                        break;

                    case ChunkTyps.EoF: //End of binary data Start of INI data
                        source.Seek(lengthOfStruct, SeekOrigin.Current);
                        StartOfStruct = source.Position;
                        lengthOfStruct = (int)(source.Length - source.Position);
                        Add(new FileNode("info.ini", new SubStream(source, lengthOfStruct)));
                        break;

                    default:
                        Add(new FileNode(opcode.ToString() + ".bin", new SubStream(source, lengthOfStruct)));
                        break;
                }

                // Read the file, move on to the next one
                source.Seek(StartOfStruct + lengthOfStruct, SeekOrigin.Begin);
            }
        }

        private void ReadTextureAttributeChunk(Stream stream, in int lengthOfStruct, in long StartOfStruct)
        {
            int NumberOfFiles = stream.ReadInt32(Endian.Big);
            stream.Seek(20, SeekOrigin.Current);

            for (int i = 1; i <= NumberOfFiles; i++)
            {
                Add(new FileNode(ChunkTyps.TextureAttribute.ToString() + i + ".attr", new SubStream(stream, 12)));
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
                Add(new FileNode(ChunkTyps.TextureAttribute.ToString() + i + ".txe", new SubStream(stream, 32 + DataSize, StartOfEntry)));
                stream.Seek(DataSize, SeekOrigin.Current);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

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
