using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using static AuroraLip.Texture.J3D.JUtility;

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

        public GXImageFormat Format => base[0].Format;
        public GXPaletteFormat PaletteFormat => base[0].PaletteFormat;
        public GXWrapMode WrapS => base[0].WrapS;
        public GXWrapMode WrapT => base[0].WrapT;
        public GXFilterMode MagnificationFilter => base[0].MagnificationFilter;
        public GXFilterMode MinificationFilter => base[0].MinificationFilter;
        public float MinLOD => base[0].MinLOD;
        public float MaxLOD => base[0].MaxLOD;
        public float LODBias => base[0].LODBias;
        public bool EnableEdgeLoD => base[0].EnableEdgeLOD;

        public Size LargestSize => base[0].LargestImageSize;
        public Size SmallestSize => base[0].SmallestImageSize;
        public JUTTransparency AlphaSetting { get; set; }
        public bool ClampLODBias { get; set; } = true;
        public byte MaxAnisotropy { get; set; } = 0;

        public BTI() { }

        public BTI(Stream stream) => Read(stream);

        public new Bitmap this[int index]
        {
            get => base[0][index];
            set => base[0][index] = value;
        }
        public new int Count => base[0].Count;

        /// <summary>
        /// Creates a BTI from a bitmap[]
        /// </summary>
        /// <param name="Source"></param>
        public BTI(Bitmap[] Source, GXImageFormat format, GXPaletteFormat palette = GXPaletteFormat.IA8, JUTTransparency Alpha = JUTTransparency.OPAQUE, GXWrapMode S = GXWrapMode.CLAMP, GXWrapMode T = GXWrapMode.CLAMP, GXFilterMode MagFilter = GXFilterMode.Nearest, GXFilterMode MinFilter = GXFilterMode.Nearest)
        {
            AlphaSetting = Alpha;
            TexEntry entry = new TexEntry()
            {
                Format = format,
                PaletteFormat = palette,
                WrapS = S,
                WrapT = T,
                MagnificationFilter = MagFilter,
                MinificationFilter = MinFilter
            };
            entry.AddRange(Source);
            Add(entry);
        }

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
            List<byte> ImageData = new List<byte>();
            List<byte> PaletteData = new List<byte>();
            GetImageAndPaletteData(ref ImageData, ref PaletteData, base[0], Format, PaletteFormat);
            long HeaderStart = stream.Position;
            int ImageDataStart = (int)((DataOffset + PaletteData.Count) - HeaderStart), PaletteDataStart = (int)(DataOffset - HeaderStart);
            stream.WriteByte((byte)this.Format);
            stream.WriteByte((byte)AlphaSetting);
            stream.WriteBigEndian(BitConverter.GetBytes((ushort)this[0].Width), 2);
            stream.WriteBigEndian(BitConverter.GetBytes((ushort)this[0].Height), 2);
            stream.WriteByte((byte)WrapS);
            stream.WriteByte((byte)WrapT);
            if (Format.IsPaletteFormat())
            {
                stream.WriteByte(0x01);
                stream.WriteByte((byte)PaletteFormat);
                stream.WriteBigEndian(BitConverter.GetBytes((ushort)(PaletteData.Count / 2)), 2);
                stream.WriteBigEndian(BitConverter.GetBytes(PaletteDataStart), 4);
            }
            else
                stream.Write(new byte[8], 0, 8);

            stream.WriteByte((byte)(Count > 1 ? 0x01 : 0x00));
            stream.WriteByte((byte)(this.EnableEdgeLoD ? 0x01 : 0x00));
            stream.WriteByte((byte)(ClampLODBias ? 0x01 : 0x00));
            stream.WriteByte(MaxAnisotropy);
            stream.WriteByte((byte)MinificationFilter);
            stream.WriteByte((byte)MagnificationFilter);
            stream.WriteByte((byte)(MinLOD * 8));
            stream.WriteByte((byte)(MaxLOD * 8));
            stream.WriteByte((byte)Count);
            stream.WriteByte(0x00);
            stream.WriteBigEndian(BitConverter.GetBytes((short)(LODBias * 100)), 2);
            stream.WriteBigEndian(BitConverter.GetBytes(ImageDataStart), 4);

            long Pauseposition = stream.Position;
            stream.Position = DataOffset;

            stream.Write(PaletteData.ToArray(), 0, PaletteData.Count);
            stream.Write(ImageData.ToArray(), 0, ImageData.Count);
            DataOffset = stream.Position;
            stream.Position = Pauseposition;
        }

        /// <summary>
        /// Cast a Bitmap to a BTI
        /// </summary>
        /// <param name="Source"></param>
        public static explicit operator BTI(Bitmap Source)
        {
            BTI NewImage = new BTI { new TexEntry() { Source } };
            return NewImage;
        }
        /// <summary>
        /// Cast Bitmaps to a BTI
        /// </summary>
        /// <param name="Source"></param>
        public static explicit operator BTI(Bitmap[] Source)
        {
            TexEntry entry = new TexEntry() { MaxLOD = Source.Length - 1 };
            entry.AddRange(Source);
            BTI NewImage = new BTI { entry };
            return NewImage;
        }

        //=====================================================================
    }
}