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

    public class BTI : JUTTexture, IFileFormat
    {

        public FileType FileType => FileType.Texture;

        public string Description => description;

        private const string description = "Binary Texture Image";

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

        internal BTI() { }

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

        protected override void Read(Stream BTIFile)
        {
            long HeaderStart = BTIFile.Position;
            GXImageFormat Format = (GXImageFormat)BTIFile.ReadByte();
            AlphaSetting = (JUTTransparency)BTIFile.ReadByte();
            ushort ImageWidth = BitConverter.ToUInt16(BTIFile.ReadBigEndian(0, 2), 0);
            ushort ImageHeight = BitConverter.ToUInt16(BTIFile.ReadBigEndian(0, 2), 0);
            GXWrapMode WrapS = (GXWrapMode)BTIFile.ReadByte();
            GXWrapMode WrapT = (GXWrapMode)BTIFile.ReadByte();
            bool UsePalettes = BTIFile.ReadByte() > 0;
            short PaletteCount = 0;
            uint PaletteDataAddress = 0;
            byte[] PaletteData = null;
            GXPaletteFormat PaletteFormat = GXPaletteFormat.IA8;
            if (UsePalettes)
            {
                PaletteFormat = (GXPaletteFormat)BTIFile.ReadByte();
                PaletteCount = BitConverter.ToInt16(BTIFile.ReadBigEndian(0, 2), 0);
                PaletteDataAddress = BitConverter.ToUInt32(BTIFile.ReadBigEndian(0, 4), 0);
                long PausePosition = BTIFile.Position;
                BTIFile.Position = HeaderStart + PaletteDataAddress;
                PaletteData = BTIFile.Read(0, PaletteCount * 2);
                BTIFile.Position = PausePosition;
            }
            else
                BTIFile.Position += 7;
            bool EnableMipmaps = BTIFile.ReadByte() > 0;
            bool EnableEdgeLOD = BTIFile.ReadByte() > 0;
            ClampLODBias = BTIFile.ReadByte() > 0;
            MaxAnisotropy = (byte)BTIFile.ReadByte();
            GXFilterMode MinificationFilter = (GXFilterMode)BTIFile.ReadByte();
            GXFilterMode MagnificationFilter = (GXFilterMode)BTIFile.ReadByte();
            float MinLOD = ((sbyte)BTIFile.ReadByte() / 8.0f);
            float MaxLOD = ((sbyte)BTIFile.ReadByte() / 8.0f);

            byte TotalImageCount = (byte)BTIFile.ReadByte();
            if (TotalImageCount == 0)
                TotalImageCount = (byte)MaxLOD;

            BTIFile.Position++;
            float LODBias = BitConverter.ToInt16(BTIFile.ReadBigEndian(0, 2), 0) / 100.0f;
            uint ImageDataAddress = BitConverter.ToUInt32(BTIFile.ReadBigEndian(0, 4), 0);

            BTIFile.Position = HeaderStart + ImageDataAddress;
            ushort ogwidth = ImageWidth, ogheight = ImageHeight;

            TexEntry current = new TexEntry(BTIFile, PaletteData, Format, PaletteFormat, PaletteCount, ImageWidth, ImageHeight, TotalImageCount)
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
        protected override void Write(Stream stream) { throw new Exception("DO NOT CALL THIS"); }
        protected void Write(Stream BTIFile, ref long DataOffset)
        {
            List<byte> ImageData = new List<byte>();
            List<byte> PaletteData = new List<byte>();
            GetImageAndPaletteData(ref ImageData, ref PaletteData, base[0], Format, PaletteFormat);
            long HeaderStart = BTIFile.Position;
            int ImageDataStart = (int)((DataOffset + PaletteData.Count) - HeaderStart), PaletteDataStart = (int)(DataOffset - HeaderStart);
            BTIFile.WriteByte((byte)this.Format);
            BTIFile.WriteByte((byte)AlphaSetting);
            BTIFile.WriteBigEndian(BitConverter.GetBytes((ushort)this[0].Width), 0, 2);
            BTIFile.WriteBigEndian(BitConverter.GetBytes((ushort)this[0].Height), 0, 2);
            BTIFile.WriteByte((byte)WrapS);
            BTIFile.WriteByte((byte)WrapT);
            if (Format.IsPaletteFormat())
            {
                BTIFile.WriteByte(0x01);
                BTIFile.WriteByte((byte)PaletteFormat);
                BTIFile.WriteBigEndian(BitConverter.GetBytes((ushort)(PaletteData.Count/2)), 0, 2);
                BTIFile.WriteBigEndian(BitConverter.GetBytes(PaletteDataStart), 0, 4);
            }
            else
                BTIFile.Write(new byte[8], 0, 8);

            BTIFile.WriteByte((byte)(Count > 1 ? 0x01 : 0x00));
            BTIFile.WriteByte((byte)(this.EnableEdgeLoD ? 0x01 : 0x00));
            BTIFile.WriteByte((byte)(ClampLODBias ? 0x01 : 0x00));
            BTIFile.WriteByte(MaxAnisotropy);
            BTIFile.WriteByte((byte)MinificationFilter);
            BTIFile.WriteByte((byte)MagnificationFilter);
            BTIFile.WriteByte((byte)(MinLOD * 8));
            BTIFile.WriteByte((byte)(MaxLOD * 8));
            BTIFile.WriteByte((byte)Count);
            BTIFile.WriteByte(0x00);
            BTIFile.WriteBigEndian(BitConverter.GetBytes((short)(LODBias * 100)), 0, 2);
            BTIFile.WriteBigEndian(BitConverter.GetBytes(ImageDataStart), 0, 4);

            long Pauseposition = BTIFile.Position;
            BTIFile.Position = DataOffset;

            BTIFile.Write(PaletteData.ToArray(), 0, PaletteData.Count);
            BTIFile.Write(ImageData.ToArray(), 0, ImageData.Count);
            DataOffset = BTIFile.Position;
            BTIFile.Position = Pauseposition;
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
            TexEntry entry = new TexEntry() { MaxLOD = Source.Length-1 };
            entry.AddRange(Source);
            BTI NewImage = new BTI { entry };
            return NewImage;
        }

        //=====================================================================
    }
}