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

    public class TPL : JUTTexture, IFileFormat
    {

        public FileType FileType => FileType.Texture;

        public string Description => description;

        private const string description = "Texture Palette Library";

        public string Extension => ".tpl";

        public bool CanRead => true;

        public bool CanWrite => true;

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 12 && stream.ReadByte() == Magic[0] && stream.ReadByte() == Magic[1] && stream.ReadByte() == Magic[2] && stream.ReadByte() == Magic[3];

        protected readonly byte[] Magic = new byte[4] { 0x00, 0x20, 0xAF, 0x30 };

        public TPL() : base() { }
        public TPL(string filepath) : base(filepath) { }
        public TPL(Stream stream) => Read(stream);

        protected override void Read(Stream TPLFile)
        {
            long HeaderStart = TPLFile.Position;

            if (!IsMatch(TPLFile)) throw new Exception("Invalid Identifier. Expected \"0x0020AF30\"");

            int TotalImageCount = BitConverter.ToInt32(TPLFile.ReadBigEndian(0, 4), 0);
            int ImageTableOffset = BitConverter.ToInt32(TPLFile.ReadBigEndian(0, 4), 0);

            TPLFile.Position = ImageTableOffset;

            List<KeyValuePair<int, int>> ImageHeaderOffset = new List<KeyValuePair<int, int>>();
            for (int i = 0; i < TotalImageCount; i++)
                ImageHeaderOffset.Add(new KeyValuePair<int, int>(BitConverter.ToInt32(TPLFile.ReadBigEndian(0, 4), 0), BitConverter.ToInt32(TPLFile.ReadBigEndian(0, 4), 0)));

            for (int i = 0; i < ImageHeaderOffset.Count; i++)
            {
                short PaletteCount = 0;
                uint PaletteDataAddress = 0;
                byte[] PaletteData = null;
                GXPaletteFormat PaletteFormat = GXPaletteFormat.IA8;
                if (ImageHeaderOffset[i].Value != 0)
                {
                    TPLFile.Position = ImageHeaderOffset[i].Value;
                    PaletteCount = BitConverter.ToInt16(TPLFile.ReadBigEndian(0, 2), 0);
                    TPLFile.Position += 0x02;
                    PaletteFormat = (GXPaletteFormat)BitConverter.ToUInt32(TPLFile.ReadBigEndian(0, 4), 0);
                    PaletteDataAddress = BitConverter.ToUInt32(TPLFile.ReadBigEndian(0, 4), 0);
                    TPLFile.Position = HeaderStart + PaletteDataAddress;
                    PaletteData = TPLFile.Read(0, PaletteCount * 2);
                }

                TPLFile.Position = ImageHeaderOffset[i].Key;

                ushort ImageHeight = BitConverter.ToUInt16(TPLFile.ReadBigEndian(0, 2), 0);
                ushort ImageWidth = BitConverter.ToUInt16(TPLFile.ReadBigEndian(0, 2), 0);
                GXImageFormat Format = (GXImageFormat)BitConverter.ToUInt32(TPLFile.ReadBigEndian(0, 4), 0);
                uint ImageDataAddress = BitConverter.ToUInt32(TPLFile.ReadBigEndian(0, 4), 0);
                uint WrapS = BitConverter.ToUInt32(TPLFile.ReadBigEndian(0, 4), 0);
                uint WrapT = BitConverter.ToUInt32(TPLFile.ReadBigEndian(0, 4), 0);
                uint MinFilter = BitConverter.ToUInt32(TPLFile.ReadBigEndian(0, 4), 0);
                uint MaxFilter = BitConverter.ToUInt32(TPLFile.ReadBigEndian(0, 4), 0);
                float LODBias = BitConverter.ToSingle(TPLFile.ReadBigEndian(0, 4), 0);
                bool EnableEdgeLOD = TPLFile.ReadByte() > 0;
                byte MinLOD = (byte)TPLFile.ReadByte();
                byte MaxLOD = (byte)TPLFile.ReadByte();
                byte Unpacked = (byte)TPLFile.ReadByte();

                TPLFile.Position = ImageDataAddress;
                TexEntry current = new TexEntry(TPLFile, PaletteData, Format, PaletteFormat, PaletteCount, ImageWidth, ImageHeight, MaxLOD)
                {
                    LODBias = LODBias,
                    MagnificationFilter = (GXFilterMode)MaxFilter,
                    MinificationFilter = (GXFilterMode)MinFilter,
                    WrapS = (GXWrapMode)WrapS,
                    WrapT = (GXWrapMode)WrapT,
                    EnableEdgeLOD = EnableEdgeLOD,
                    MinLOD = MinLOD,
                    MaxLOD = MaxLOD
                };
                Add(current);
            }
        }

        protected override void Write(Stream TPLFile)
        {
            long HeaderStart = TPLFile.Position;
            TPLFile.Write(Magic, 0, Magic.Length);

            TPLFile.WriteBigEndian(BitConverter.GetBytes(Count), 0, 4);
            TPLFile.WriteBigEndian(BitConverter.GetBytes(0x0C), 0, 4);
            long OffsetLocation = TPLFile.Position;
            //Placeholders...Not my usual 0xDD placeholder because there's no way I'm able to calculate the byte to use
            //Well, there is but eeehhhh it's slow(?)
            TPLFile.Write(new byte[Count * 8], 0, Count * 8);
            //This will hold the information that we're gonna write back here
            //Key = ImageDataOffset
            //Value = PalleteDataOffset
            List<KeyValuePair<int, int>> ImageHeaderOffset = new List<KeyValuePair<int, int>>();

            //TPLFile.PadTo(32);
            for (int i = 0; i < Count; i++)
            {
                bool IsPalette = this[i].Format.IsPaletteFormat();
                int PaletteHeader = !IsPalette ? 0 : (int)(TPLFile.Position - HeaderStart);
                List<byte> ImageData = new List<byte>();
                List<byte> PaletteData = new List<byte>();
                List<Bitmap> mips = this[i];
                GetImageAndPaletteData(ref ImageData, ref PaletteData, mips, this[i].Format, this[i].PaletteFormat);
                if (IsPalette)
                {
                    TPLFile.WriteBigEndian(BitConverter.GetBytes((ushort)(PaletteData.Count / 2)), 0, 2);
                    TPLFile.WriteByte(0x00);
                    TPLFile.WriteByte(0x00);
                    TPLFile.WriteBigEndian(BitConverter.GetBytes((int)this[i].PaletteFormat), 0, 4);
                    long temp = TPLFile.Position;
                    TPLFile.WriteBigEndian(BitConverter.GetBytes(-572662307), 0, 4);
                    TPLFile.PadTo(32);
                    long temp2 = TPLFile.Position;
                    TPLFile.Position = temp;
                    TPLFile.WriteBigEndian(BitConverter.GetBytes((int)(temp2 - HeaderStart) + 0), 0, 4);
                    TPLFile.Position = temp2;
                    TPLFile.Write(PaletteData.ToArray(), 0, PaletteData.Count);
                    TPLFile.PadTo(32);
                }

                int ImageHeader = ImageHeader = (int)(TPLFile.Position - HeaderStart);

                TPLFile.WriteBigEndian(BitConverter.GetBytes((ushort)this[i][0].Height), 0, 2);
                TPLFile.WriteBigEndian(BitConverter.GetBytes((ushort)this[i][0].Width), 0, 2);
                TPLFile.WriteBigEndian(BitConverter.GetBytes((uint)this[i].Format), 0, 4);
                long comebackhere = TPLFile.Position;
                TPLFile.WriteBigEndian(BitConverter.GetBytes(-572662307), 0, 4);
                TPLFile.WriteBigEndian(BitConverter.GetBytes((uint)this[i].WrapS), 0, 4);
                TPLFile.WriteBigEndian(BitConverter.GetBytes((uint)this[i].WrapT), 0, 4);
                TPLFile.WriteBigEndian(BitConverter.GetBytes((uint)this[i].MinificationFilter), 0, 4);
                TPLFile.WriteBigEndian(BitConverter.GetBytes((uint)this[i].MagnificationFilter), 0, 4);
                TPLFile.WriteBigEndian(BitConverter.GetBytes(this[i].LODBias), 0, 4);
                TPLFile.WriteByte((byte)(this[i].EnableEdgeLOD ? 0x01 : 0x00));
                TPLFile.WriteByte(0x00);
                TPLFile.WriteByte((byte)(mips.Count - 1));
                //Unpacked - Rii told me to leave this one as 0 :)
                TPLFile.WriteByte(0x00);
                TPLFile.PadTo(32);
                long PausePosition = TPLFile.Position;
                TPLFile.Position = comebackhere;
                TPLFile.WriteBigEndian(BitConverter.GetBytes((int)(PausePosition - HeaderStart)), 0, 4);
                TPLFile.Position = PausePosition;
                TPLFile.Write(ImageData.ToArray(), 0, ImageData.Count);
                TPLFile.PadTo(32);

                ImageHeaderOffset.Add(new KeyValuePair<int, int>(ImageHeader, PaletteHeader));
            }
            TPLFile.Position = OffsetLocation;
            for (int i = 0; i < Count; i++)
            {
                TPLFile.WriteBigEndian(BitConverter.GetBytes(ImageHeaderOffset[i].Key), 0, 4);
                TPLFile.WriteBigEndian(BitConverter.GetBytes(ImageHeaderOffset[i].Value), 0, 4);
            }
        }

        /// <summary>
        /// Creates a TPL using a Bitmap (Single Texture, No Mipmaps)
        /// </summary>
        /// <param name="Image"></param>
        /// <param name="ImageFormat"></param>
        /// <param name="PaletteFormat"></param>
        /// <param name="WrapS"></param>
        /// <param name="WrapT"></param>
        /// <param name="MagFilter"></param>
        /// <param name="MinFilter"></param>
        /// <param name="Bias"></param>
        /// <param name="EdgeLoD"></param>
        /// <returns></returns>
        public static TPL Create(Bitmap Image, GXImageFormat ImageFormat = GXImageFormat.CMPR, GXPaletteFormat PaletteFormat = GXPaletteFormat.IA8, GXWrapMode WrapS = GXWrapMode.CLAMP, GXWrapMode WrapT = GXWrapMode.CLAMP, GXFilterMode MagFilter = GXFilterMode.Linear, GXFilterMode MinFilter = GXFilterMode.Linear, float Bias = 0.0f, bool EdgeLoD = false)
        {
            TPL NewTPL = new TPL
            {
                new TexEntry((Bitmap)Image.Clone(), ImageFormat, PaletteFormat, WrapS, WrapT, MagFilter, MinFilter, Bias, EdgeLoD)
            };
            return NewTPL;
        }
        /// <summary>
        /// Creates a TPL using a Bitmap Array (Multiple Textures, No Mipmaps)
        /// </summary>
        /// <param name="Images"></param>
        /// <param name="ImageFormat"></param>
        /// <param name="PaletteFormat"></param>
        /// <param name="WrapS"></param>
        /// <param name="WrapT"></param>
        /// <param name="MagFilter"></param>
        /// <param name="MinFilter"></param>
        /// <param name="Bias"></param>
        /// <param name="EdgeLoD"></param>
        /// <returns></returns>
        public static TPL Create(Bitmap[] Images, GXImageFormat ImageFormat = GXImageFormat.CMPR, GXPaletteFormat PaletteFormat = GXPaletteFormat.IA8, GXWrapMode WrapS = GXWrapMode.CLAMP, GXWrapMode WrapT = GXWrapMode.CLAMP, GXFilterMode MagFilter = GXFilterMode.Linear, GXFilterMode MinFilter = GXFilterMode.Linear, float Bias = 0.0f, bool EdgeLoD = false)
        {
            TPL NewTPL = new TPL();
            for (int i = 0; i < Images.Length; i++)
                NewTPL.Add(new TexEntry((Bitmap)Images[i].Clone(), ImageFormat, PaletteFormat, WrapS, WrapT, MagFilter, MinFilter, Bias, EdgeLoD));
            return NewTPL;
        }
        /// <summary>
        /// Creates a TPL using Arrays of Bitmap Arrays (Multiple Textures, Mipmap support)
        /// </summary>
        /// <param name="Images"></param>
        /// <param name="ImageFormat"></param>
        /// <param name="PaletteFormat"></param>
        /// <param name="WrapS"></param>
        /// <param name="WrapT"></param>
        /// <param name="MagFilter"></param>
        /// <param name="MinFilter"></param>
        /// <param name="Bias"></param>
        /// <param name="EdgeLoD"></param>
        /// <returns></returns>
        public static TPL Create(Bitmap[][] Images, GXImageFormat ImageFormat = GXImageFormat.CMPR, GXPaletteFormat PaletteFormat = GXPaletteFormat.IA8, GXWrapMode WrapS = GXWrapMode.CLAMP, GXWrapMode WrapT = GXWrapMode.CLAMP, GXFilterMode MagFilter = GXFilterMode.Linear, GXFilterMode MinFilter = GXFilterMode.Linear, float Bias = 0.0f, bool EdgeLoD = false)
        {
            TPL NewTPL = new TPL();

            for (int i = 0; i < Images.Length; i++)
            {
                TexEntry temp = new TexEntry() { Format = ImageFormat, PaletteFormat = PaletteFormat, WrapS = WrapS, WrapT = WrapT, MagnificationFilter = MagFilter, MinificationFilter = MinFilter, LODBias = Bias, EnableEdgeLOD = EdgeLoD };
                for (int x = 0; x < Images[i].Length; x++)
                    temp.Add((Bitmap)Images[i][x].Clone());
                NewTPL.Add(temp);
            }

            return NewTPL;
        }

    }
}
