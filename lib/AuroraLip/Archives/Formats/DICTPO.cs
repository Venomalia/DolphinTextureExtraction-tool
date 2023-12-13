using AuroraLib.Common;
using AuroraLib.Core.Buffers;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    public class DICTPO : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new(0x5824F3A9);

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            //try to request an external file.
            string datname = Path.ChangeExtension(Path.GetFileNameWithoutExtension(FullPath), ".data");
            try
            {
                streamData = FileRequest.Invoke(datname);
            }
            catch (Exception)
            {
                throw new Exception($"{nameof(DICTPO)}: could not request the file {datname}.");
            }
            // Read DICT
            stream.MatchThrow(_identifier);
            Header header = stream.Read<Header>(Endian.Big);
            Span<BlockData> blocks = stackalloc BlockData[8];
            stream.Read(blocks, Endian.Big);

            // get Block offsets
            Span<uint> blockOffset = stackalloc uint[8];
            uint offset = 0;
            for (int i = 0; i < blocks.Length; i++)
            {
                uint size = blocks[i].Size;
                if (size != 0)
                    blockOffset[i] = offset;
                offset += size;
            }

            // Read DATA, Chunks
            streamData.Seek(offset, SeekOrigin.Begin);
            using SpanBuffer<ChunkInfo> chunks = new(header.ChunkTableSize / 12);
            streamData.Read(chunks.Span, Endian.Big);
            Root = new ArchiveDirectory() { OwnerArchive = this };
            Stream textureHeadersChunk = null;
            int textureTabel = 0;
            for (int i = 0; i < chunks.Length; i++)
            {
                if (chunks[i].BlockIndex == -1)
                    continue;

                Stream chunkData = new SubStream(streamData, chunks[i].Size, blockOffset[chunks[i].BlockIndex] + chunks[i].Offset);
                switch (chunks[i].SectionType)
                {
                    case SectionMagic.TextureHeaders:
                        textureHeadersChunk = chunkData;
                        break;
                    case SectionMagic.TextureData:
                        int textures = (int)(textureHeadersChunk.Length / 0x60);
                        Span<uint> textureOffsets = stackalloc uint[textures + 1];
                        Span<byte> headerBuffer = stackalloc byte[0x60];
                        for (int ti = 0; ti < textures; ti++)
                        {
                            textureHeadersChunk.Seek(ti * 0x60 + 0x14, SeekOrigin.Begin);
                            textureOffsets[ti] = textureHeadersChunk.ReadUInt32(Endian.Big);
                        }
                        textureOffsets[textures] = (uint)chunkData.Length;
                        textureHeadersChunk.Seek(0, SeekOrigin.Begin);
                        for (int ti = 0; ti < textures; ti++)
                        {
                            uint size = textureOffsets[ti + 1] - textureOffsets[ti];
                            MemoryPoolStream texture = new((int)(size + 0x60));
                            SubStream textureData = new(chunkData, size, textureOffsets[ti]);
                            textureHeadersChunk.Read(headerBuffer);
                            texture.Write(headerBuffer);
                            textureData.CopyTo(texture);
                            texture.Seek(0, SeekOrigin.Begin);
                            Root.AddArchiveFile(texture, $"Texture_{textureTabel}_{ti}.TexPO");
                        }
                        textureTabel++;
                        break;
                    default:
                        Root.AddArchiveFile(chunkData, $"{chunks[i].SectionType}_{i}.dat");
                        break;
                }
            }
        }

        protected override void Write(Stream ArchiveFile) => throw new NotImplementedException();

        protected readonly struct Header
        {
            public readonly byte Unk1; // 6
            public readonly byte Unk2; // 1
            private readonly byte Compressed;
            private readonly byte Pad;
            public readonly uint Unk3; // 1
            public readonly uint Unk4; // 0
            public readonly uint Files;
            public readonly uint ChunkTableSize;

            public readonly bool IsCompressed => Compressed == 1;
        }

        protected readonly struct BlockData
        {
            public readonly uint Size;
            public readonly uint Parameter;
        }

        protected readonly struct ChunkInfo
        {
            public readonly byte ChunkFlag;
            public readonly byte unk; // 1
            public readonly SectionMagic SectionType;
            public readonly uint Size;
            public readonly uint Offset;

            public readonly int BlockIndex
            {
                get
                {
                    int index = (byte)(ChunkFlag >> 4);
                    if (Size == 0 || index > 7)
                        return -1;
                    else
                        return index;
                }
            }
        }

        public enum SectionMagic : ushort
        {
            RootName = 0x1,
            String = 0x2,
            TextureHeaders = 0xB601,
            TextureData = 0xB603,
            MaterialData = 0xB016,
            IndexData = 0xB007,
            VertexData = 0xB006,
            VertexAttributePointerData = 0xB005,
            MeshData = 0xB004,
            ModelData = 0xB003,
            MatrixData = 0xB002,
            SkeletonData = 0xB008,
            BoneHashes = 0xB00B,
            BoneData = 0xB00A,
            UnknownHashList = 0xB00C,
        }

        private Stream streamData;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                streamData?.Dispose();
            }
        }
    }
}
