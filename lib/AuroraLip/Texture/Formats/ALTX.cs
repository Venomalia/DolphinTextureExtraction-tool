using AuroraLib.Core.Buffers;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Texture.Formats
{
    public class ALTX : ALIG
    {
        public override IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("ALTX");


        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier); ;
            bool isMultitexture = stream.ReadByte() != 0;
            TextureFlags flags = stream.Read<TextureFlags>();
            ushort numTextureEntry = stream.ReadUInt16(Endian.Big);
            uint imageOffset = stream.ReadUInt32(Endian.Big);

            //TODO: have to be derived from the base image.
            using SpanBuffer<ushort> entryOffsets = new(numTextureEntry);
            stream.Read<ushort>(entryOffsets, Endian.Big);
            List<TextureEntry> entries = new(numTextureEntry);
            foreach (ushort entryOffset in entryOffsets)
            {
                stream.Seek(entryOffset, SeekOrigin.Begin);
                entries.Add(stream.Read<TextureEntry>());
            }

            stream.Seek(imageOffset, SeekOrigin.Begin);
            base.Read(stream);
        }

        private class TextureEntry : IBinaryObject
        {
            public uint ID { get; set; }
            public EntryFlags Flags { get; set; }
            public readonly List<BoundingBox> Bounds;
            public short CenterPointX { get; set; }
            public short CenterPointY { get; set; }
            public string Name { get; set; }

            public TextureEntry()
                => Bounds = new List<BoundingBox>();

            public void BinaryDeserialize(Stream source)
            {
                ID = source.ReadUInt32(Endian.Big);
                ushort bounds = source.ReadUInt16(Endian.Big);
                Flags = source.Read<EntryFlags>();
                source.ReadByte();
                if ((Flags & EntryFlags.HasName) != 0)
                {
                    source.Seek(-0x20-8, SeekOrigin.Current);
                    Name = source.ReadString(0x20);
                    source.Seek(8, SeekOrigin.Current);
                }
                else
                {
                    Name = string.Empty;
                }

                Bounds.Clear();
                for (int i = 0; i < bounds; i++)
                {
                    Bounds.Add(source.Read<BoundingBox>(Endian.Big));
                }

                if ((Flags & EntryFlags.HasCenterPoint) != 0)
                {
                    CenterPointX = source.ReadInt16(Endian.Big);
                    CenterPointY = source.ReadInt16(Endian.Big);
                }
                else
                {
                    CenterPointX = CenterPointY = 0;
                }
            }

            public void BinarySerialize(Stream dest) => throw new NotImplementedException();
        }

        private readonly struct BoundingBox
        {
            public readonly short X;
            public readonly short Y;
            public readonly short W;
            public readonly short H;
        }

        [Flags]
        public enum TextureFlags : byte
        {
            Special = 1,
            Indirect = 2,
            OverrideDimensions = 4,
            Unk = 128
        }

        [Flags]
        public enum EntryFlags : byte
        {
            HasName = 1,
            HasCenterPoint = 2,
            Filtered = 4
        }
    }
}
