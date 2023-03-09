using AuroraLip.Common;

namespace AuroraLip.Texture.Formats
{
    /*
    * Super Hackio Incorporated
    * "Copyright © Super Hackio Incorporated 2020-2021"
    * https://github.com/SuperHackio/Hack.io
    */

    public class BTI : JUTTexture, IFileAccess
    {

        public string Extension => ".bti";

        public bool CanRead => true;

        public bool CanWrite => true;

        public bool IsMatch(Stream stream, in string extension = "")
            => extension.ToLower() == Extension;

        public JUTTransparency AlphaSetting { get; set; }
        public bool ClampLODBias { get; set; } = true;
        public byte MaxAnisotropy { get; set; } = 0;

        public BTI() { }

        public BTI(Stream stream) => Read(stream);

        public override void Save(Stream stream)
        {
            long BaseDataOffset = stream.Position + 0x20;
            Write(stream, ref BaseDataOffset);
        }
        public void Save(Stream BTIFile, ref long DataOffset) => Write(BTIFile, ref DataOffset);

        protected override void Read(Stream stream)
        {
            long HeaderStart = stream.Position;
            GXImageFormat Format = (GXImageFormat)stream.ReadByte();
            AlphaSetting = (JUTTransparency)stream.ReadByte();
            ushort ImageWidth = stream.ReadUInt16(Endian.Big);
            ushort ImageHeight = stream.ReadUInt16(Endian.Big);
            GXWrapMode WrapS = (GXWrapMode)stream.ReadByte();
            GXWrapMode WrapT = (GXWrapMode)stream.ReadByte();
            bool UsePalettes = stream.ReadByte() > 0;
            short PaletteCount = 0;
            uint PaletteDataAddress = 0;
            byte[] PaletteData = null;
            GXPaletteFormat PaletteFormat = GXPaletteFormat.IA8;
            if (UsePalettes)
            {
                PaletteFormat = (GXPaletteFormat)stream.ReadByte();
                PaletteCount = stream.ReadInt16(Endian.Big);
                PaletteDataAddress = stream.ReadUInt32(Endian.Big);
                long PausePosition = stream.Position;
                stream.Position = HeaderStart + PaletteDataAddress;
                PaletteData = stream.Read(PaletteCount * 2);
                stream.Position = PausePosition;
            }
            else
                stream.Position += 7;
            bool EnableMipmaps = stream.ReadByte() > 0;
            bool EnableEdgeLOD = stream.ReadByte() > 0;
            ClampLODBias = stream.ReadByte() > 0;
            MaxAnisotropy = (byte)stream.ReadByte();
            GXFilterMode MinificationFilter = (GXFilterMode)stream.ReadByte();
            GXFilterMode MagnificationFilter = (GXFilterMode)stream.ReadByte();
            float MinLOD = ((sbyte)stream.ReadByte() / 8.0f);
            float MaxLOD = ((sbyte)stream.ReadByte() / 8.0f);

            byte TotalImageCount = (byte)stream.ReadByte();
            if (TotalImageCount == 0)
                TotalImageCount = (byte)MaxLOD;

            stream.Position++;
            float LODBias = stream.ReadInt16(Endian.Big) / 100.0f;
            uint ImageDataAddress = stream.ReadUInt32(Endian.Big);

            stream.Position = HeaderStart + ImageDataAddress;
            ushort ogwidth = ImageWidth, ogheight = ImageHeight;

            TexEntry current = new TexEntry(stream, PaletteData, Format, PaletteFormat, PaletteCount, ImageWidth, ImageHeight, TotalImageCount - 1)
            {
                LODBias = LODBias,
                MagnificationFilter = (GXFilterMode)MagnificationFilter,
                MinificationFilter = (GXFilterMode)MinificationFilter,
                WrapS = (GXWrapMode)WrapS,
                WrapT = (GXWrapMode)WrapT,
                EnableEdgeLOD = EnableEdgeLOD,
                MinLOD = MinLOD,
                MaxLOD = MaxLOD
            };
            Add(current);
        }
        protected override void Write(Stream stream) { throw new NotSupportedException("DO NOT CALL THIS"); }
        protected void Write(Stream stream, ref long DataOffset)
        {
            long HeaderStart = stream.Position;
            int ImageDataStart = (int)((DataOffset + this[0].Palettes.Sum(p => p.Size)) - HeaderStart), PaletteDataStart = (int)(DataOffset - HeaderStart);
            stream.WriteByte((byte)this[0].Format);
            stream.WriteByte((byte)AlphaSetting);
            stream.WriteBigEndian(BitConverter.GetBytes(this[0].ImageWidth), 2);
            stream.WriteBigEndian(BitConverter.GetBytes((ushort)this[0].ImageHeight), 2);
            stream.WriteByte((byte)this[0].WrapS);
            stream.WriteByte((byte)this[0].WrapT);
            if (this[0].Format.IsPaletteFormat())
            {
                stream.WriteByte(0x01);
                stream.WriteByte((byte)this[0].PaletteFormat);
                stream.WriteBigEndian(BitConverter.GetBytes((ushort)(this[0].Palettes[0].Size / 2)), 2);
                stream.WriteBigEndian(BitConverter.GetBytes(PaletteDataStart), 4);
            }
            else
                stream.Write(new byte[8], 0, 8);

            stream.WriteByte((byte)(Count > 1 ? 0x01 : 0x00));
            stream.WriteByte((byte)(this[0].EnableEdgeLOD ? 0x01 : 0x00));
            stream.WriteByte((byte)(ClampLODBias ? 0x01 : 0x00));
            stream.WriteByte(MaxAnisotropy);
            stream.WriteByte((byte)this[0].MinificationFilter);
            stream.WriteByte((byte)this[0].MagnificationFilter);
            stream.WriteByte((byte)(this[0].MinLOD * 8));
            stream.WriteByte((byte)(this[0].MaxLOD * 8));
            stream.WriteByte((byte)Count);
            stream.WriteByte(0x00);
            stream.WriteBigEndian(BitConverter.GetBytes((short)(this[0].LODBias * 100)), 2);
            stream.WriteBigEndian(BitConverter.GetBytes(ImageDataStart), 4);

            long Pauseposition = stream.Position;
            stream.Position = DataOffset;

            foreach (var bytes in this[0].Palettes)
                stream.Write(bytes.GetBytes());
            foreach (var bytes in this[0].RawImages)
                stream.Write(bytes);
            DataOffset = stream.Position;
            stream.Position = Pauseposition;
        }
    }
}
