using Hack.io.RARC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hack.io.J3D.JUtility;

namespace Hack.io.TPL
{
    public class TPL : JUTTexture
    {
        protected readonly byte[] Magic = new byte[4] { 0x00, 0x20, 0xAF, 0x30 };

        public TPL() : base() { }
        public TPL(string filepath) : base(filepath) { }
        public TPL(Stream stream) => Read(stream);

        protected override void Read(Stream TPLFile)
        {
            long HeaderStart = TPLFile.Position;
            {
                byte[] tmp = TPLFile.Read(0, 4);
                if (tmp[0] != Magic[0] || tmp[1] != Magic[1] || tmp[2] != Magic[2] || tmp[3] != Magic[3])
                    throw new Exception("Invalid Identifier. Expected \"0x0020AF30\"");
            }
            int TotalImageCount = BitConverter.ToInt32(TPLFile.ReadReverse(0, 4), 0);
            int ImageTableOffset = BitConverter.ToInt32(TPLFile.ReadReverse(0, 4), 0);

            TPLFile.Position = ImageTableOffset;
            //Key = ImageDataOffset
            //Value = PalleteDataOffset
            List<KeyValuePair<int, int>> ImageHeaderOffset = new List<KeyValuePair<int, int>>();
            for (int i = 0; i < TotalImageCount; i++)
                ImageHeaderOffset.Add(new KeyValuePair<int, int>(BitConverter.ToInt32(TPLFile.ReadReverse(0, 4), 0), BitConverter.ToInt32(TPLFile.ReadReverse(0, 4), 0)));

            for (int i = 0; i < ImageHeaderOffset.Count; i++)
            {
                short PaletteCount = 0;
                uint PaletteDataAddress = 0;
                byte[] PaletteData = null;
                GXPaletteFormat PaletteFormat = GXPaletteFormat.IA8;
                if (ImageHeaderOffset[i].Value != 0)
                {
                    TPLFile.Position = ImageHeaderOffset[i].Value;
                    PaletteCount = BitConverter.ToInt16(TPLFile.ReadReverse(0, 2), 0);
                    TPLFile.Position += 0x02;
                    PaletteFormat = (GXPaletteFormat)BitConverter.ToUInt32(TPLFile.ReadReverse(0, 4), 0);
                    PaletteDataAddress = BitConverter.ToUInt32(TPLFile.ReadReverse(0, 4), 0);
                    TPLFile.Position = HeaderStart + PaletteDataAddress;
                    PaletteData = TPLFile.Read(0, PaletteCount * 2);
                }

                TPLFile.Position = ImageHeaderOffset[i].Key;

                ushort ImageHeight = BitConverter.ToUInt16(TPLFile.ReadReverse(0, 2), 0);
                ushort ImageWidth = BitConverter.ToUInt16(TPLFile.ReadReverse(0, 2), 0);
                GXImageFormat Format = (GXImageFormat)BitConverter.ToUInt32(TPLFile.ReadReverse(0, 4), 0);
                uint ImageDataAddress = BitConverter.ToUInt32(TPLFile.ReadReverse(0, 4), 0);
                uint WrapS = BitConverter.ToUInt32(TPLFile.ReadReverse(0, 4), 0);
                uint WrapT = BitConverter.ToUInt32(TPLFile.ReadReverse(0, 4), 0);
                uint MinFilter = BitConverter.ToUInt32(TPLFile.ReadReverse(0, 4), 0);
                uint MaxFilter = BitConverter.ToUInt32(TPLFile.ReadReverse(0, 4), 0);
                float LODBias = BitConverter.ToSingle(TPLFile.ReadReverse(0, 4), 0);
                bool EnableEdgeLOD = TPLFile.ReadByte() > 0;
                byte MinLOD = (byte)TPLFile.ReadByte();
                byte MaxLOD = (byte)TPLFile.ReadByte();
                byte Unpacked = (byte)TPLFile.ReadByte();
                TexEntry current = new TexEntry()
                {
                    Format = Format,
                    PaletteFormat = PaletteFormat,
                    LODBias = LODBias,
                    MagnificationFilter = (GXFilterMode)MaxFilter,
                    MinificationFilter = (GXFilterMode)MinFilter,
                    WrapS = (GXWrapMode)WrapS,
                    WrapT = (GXWrapMode)WrapT,
                    EnableEdgeLOD = EnableEdgeLOD,
                    MinLOD = MinLOD,
                    MaxLOD = MaxLOD
                };

                TPLFile.Position = ImageDataAddress;
                ushort ogwidth = ImageWidth, ogheight = ImageHeight;
                for (int x = 0; x <= MaxLOD; x++)
                {
                    current.Add(DecodeImage(TPLFile, PaletteData, Format, PaletteFormat, PaletteCount, ImageWidth, ImageHeight, x));

                    //Modification: Required to hash!
                    if (x == 0)
                    {
                        long ImageEndAddress = TPLFile.Position - HeaderStart - ImageDataAddress;
                        TPLFile.Position = HeaderStart + ImageDataAddress;
                        current.RowDate = TPLFile.Read(0, (int)(ImageEndAddress));
                    }
                }
                Add(current);
            }
        }

        protected override void Write(Stream TPLFile)
        {
            long HeaderStart = TPLFile.Position;
            TPLFile.Write(Magic, 0, Magic.Length);

            TPLFile.WriteReverse(BitConverter.GetBytes(Count), 0, 4);
            TPLFile.WriteReverse(BitConverter.GetBytes(0x0C), 0, 4);
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
                    TPLFile.WriteReverse(BitConverter.GetBytes((ushort)(PaletteData.Count / 2)), 0, 2);
                    TPLFile.WriteByte(0x00);
                    TPLFile.WriteByte(0x00);
                    TPLFile.WriteReverse(BitConverter.GetBytes((int)this[i].PaletteFormat), 0, 4);
                    long temp = TPLFile.Position;
                    TPLFile.WriteReverse(BitConverter.GetBytes(-572662307), 0, 4);
                    TPLFile.PadTo(32);
                    long temp2 = TPLFile.Position;
                    TPLFile.Position = temp;
                    TPLFile.WriteReverse(BitConverter.GetBytes((int)(temp2 - HeaderStart) + 0), 0, 4);
                    TPLFile.Position = temp2;
                    TPLFile.Write(PaletteData.ToArray(), 0, PaletteData.Count);
                    TPLFile.PadTo(32);
                }

                int ImageHeader = ImageHeader = (int)(TPLFile.Position - HeaderStart);

                TPLFile.WriteReverse(BitConverter.GetBytes((ushort)this[i][0].Height), 0, 2);
                TPLFile.WriteReverse(BitConverter.GetBytes((ushort)this[i][0].Width), 0, 2);
                TPLFile.WriteReverse(BitConverter.GetBytes((uint)this[i].Format), 0, 4);
                long comebackhere = TPLFile.Position;
                TPLFile.WriteReverse(BitConverter.GetBytes(-572662307), 0, 4);
                TPLFile.WriteReverse(BitConverter.GetBytes((uint)this[i].WrapS), 0, 4);
                TPLFile.WriteReverse(BitConverter.GetBytes((uint)this[i].WrapT), 0, 4);
                TPLFile.WriteReverse(BitConverter.GetBytes((uint)this[i].MinificationFilter), 0, 4);
                TPLFile.WriteReverse(BitConverter.GetBytes((uint)this[i].MagnificationFilter), 0, 4);
                TPLFile.WriteReverse(BitConverter.GetBytes(this[i].LODBias), 0, 4);
                TPLFile.WriteByte((byte)(this[i].EnableEdgeLOD ? 0x01 : 0x00));
                TPLFile.WriteByte(0x00);
                TPLFile.WriteByte((byte)(mips.Count-1));
                //Unpacked - Rii told me to leave this one as 0 :)
                TPLFile.WriteByte(0x00);
                TPLFile.PadTo(32);
                long PausePosition = TPLFile.Position;
                TPLFile.Position = comebackhere;
                TPLFile.WriteReverse(BitConverter.GetBytes((int)(PausePosition - HeaderStart)), 0, 4);
                TPLFile.Position = PausePosition;
                TPLFile.Write(ImageData.ToArray(), 0, ImageData.Count);
                TPLFile.PadTo(32);

                ImageHeaderOffset.Add(new KeyValuePair<int, int>(ImageHeader, PaletteHeader));
            }
            TPLFile.Position = OffsetLocation;
            for (int i = 0; i < Count; i++)
            {
                TPLFile.WriteReverse(BitConverter.GetBytes(ImageHeaderOffset[i].Key), 0, 4);
                TPLFile.WriteReverse(BitConverter.GetBytes(ImageHeaderOffset[i].Value), 0, 4);
            }
        }

        //public class TPLTexture
        //{
        //    private List<Bitmap> mipmaps = new List<Bitmap>();
        //    internal List<Bitmap> GetMipmaps() => mipmaps;
        //    public GXImageFormat Format { get; set; }
        //    public GXPaletteFormat PaletteFormat { get; set; }
        //    public GXWrapMode WrapS { get; set; }
        //    public GXWrapMode WrapT { get; set; }
        //    public GXFilterMode MagnificationFilter { get; set; }
        //    public GXFilterMode MinificationFilter { get; set; }
        //    public float LODBias { get; set; }
        //    public bool EnableEdgeLoD { get; set; }

        //    #region Auto Properties
        //    /// <summary>
        //    /// The Amount of images inside this BTI. Basically the mipmap count
        //    /// </summary>
        //    public int ImageCount => mipmaps == null ? -1 : mipmaps.Count;
        //    public Bitmap this[int MipmapLevel]
        //    {
        //        get => mipmaps[MipmapLevel];
        //        set
        //        {
        //            if (MipmapLevel < 0)
        //                throw new ArgumentOutOfRangeException("MipmapLevel");
        //            if (MipmapLevel == 0)
        //            {
        //                if (mipmaps.Count == 0)
        //                    mipmaps.Add(value);
        //                else
        //                    mipmaps[0] = value;
        //            }
        //            else
        //            {
        //                int requiredmipwidth = mipmaps[0].Width, requiredmipheight = mipmaps[0].Height;
        //                for (int i = 0; i < MipmapLevel; i++)
        //                {
        //                    if (requiredmipwidth == 1 || requiredmipwidth == 1)
        //                        throw new Exception($"The provided Mipmap Level is too high and will provide image dimensions less than 1x1. Currently, the Max Mipmap Level is {i - 1}");
        //                    requiredmipwidth /= 2;
        //                    requiredmipheight /= 2;
        //                }
        //                if (value.Width != requiredmipwidth || value.Height != requiredmipheight)
        //                    throw new Exception($"The dimensions of the provided mipmap are supposed to be {requiredmipwidth}x{requiredmipheight}");

        //                if (MipmapLevel == mipmaps.Count)
        //                    mipmaps.Add(value);
        //                else if (MipmapLevel > mipmaps.Count)
        //                {
        //                    while (mipmaps.Count - 1 != MipmapLevel)
        //                        mipmaps.Add(new Bitmap(mipmaps[mipmaps.Count - 1], new Size(mipmaps[mipmaps.Count - 1].Width / 2, mipmaps[mipmaps.Count - 1].Height / 2)));
        //                    mipmaps.Add(value);
        //                }
        //                else
        //                    mipmaps[MipmapLevel] = value;
        //            }
        //        }
        //    }
        //    #endregion

        //    public TPLTexture() { }
        //    internal TPLTexture(Bitmap Image, GXImageFormat ImageFormat = GXImageFormat.CMPR, GXPaletteFormat PaletteFormat = GXPaletteFormat.IA8, GXWrapMode WrapS = 0, GXWrapMode WrapT = 0, GXFilterMode MagFilter = 0, GXFilterMode MinFilter = 0, float Bias = 0.0f, bool EdgeLoD = false)
        //    {
        //        mipmaps = new List<Bitmap>() { Image };
        //        Format = ImageFormat;
        //        this.PaletteFormat = PaletteFormat;
        //        this.WrapS = WrapS;
        //        this.WrapT = WrapT;
        //        MagnificationFilter = MagFilter;
        //        MinificationFilter = MinFilter;
        //        LODBias = Bias;
        //        EnableEdgeLoD = EdgeLoD;
        //    }
        //    internal TPLTexture(Bitmap[] Images, GXImageFormat ImageFormat = GXImageFormat.CMPR, GXPaletteFormat PaletteFormat = GXPaletteFormat.IA8, GXWrapMode WrapS = 0, GXWrapMode WrapT = 0, GXFilterMode MagFilter = 0, GXFilterMode MinFilter = 0, float Bias = 0.0f, bool EdgeLoD = false)
        //    {
        //        mipmaps = Images.ToList();
        //        Format = ImageFormat;
        //        this.PaletteFormat = PaletteFormat;
        //        this.WrapS = WrapS;
        //        this.WrapT = WrapT;
        //        MagnificationFilter = MagFilter;
        //        MinificationFilter = MinFilter;
        //        LODBias = Bias;
        //        EnableEdgeLoD = EdgeLoD;
        //    }

        //}

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

        /// <summary>
        /// Cast a RARCFile to a TPL
        /// </summary>
        /// <param name="x"></param>
        public static implicit operator TPL(RARC.RARC.File x) => new TPL((MemoryStream)x) { FileName = x.Name };
    }
}
