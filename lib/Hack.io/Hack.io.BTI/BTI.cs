using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using static Hack.io.J3D.JUtility;

namespace Hack.io.BTI
{
    public class BTI : JUTTexture
    {
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

        public Size LargestSize => base[0].LargestSize;
        public Size SmallestSize => base[0].SmallestSize;
        //private List<Bitmap> mipmaps = new List<Bitmap>();
        //public GXImageFormat Format { get; set; }
        //public GXPaletteFormat PaletteFormat { get; set; }
        public JUTTransparency AlphaSetting { get; set; }
        //public GXWrapMode WrapS { get; set; }
        //public GXWrapMode WrapT { get; set; }
        //public GXFilterMode MagnificationFilter { get; set; }
        //public GXFilterMode MinificationFilter { get; set; }
        //public float MinLOD { get; set; } // Fixed point number, 1/8 = conversion (ToDo: is this multiply by 8 or divide...)
        //public float MaxLOD { get; set; } // Fixed point number, 1/8 = conversion (ToDo: is this multiply by 8 or divide...)
        //public bool EnableEdgeLOD { get; set; }
        //public float LODBias { get; set; } // Fixed point number, 1/100 = conversion
        public bool ClampLODBias { get; set; } = true;
        public byte MaxAnisotropy { get; set; } = 0;

        internal BTI() { }
        public BTI(string filename)
        {
            FileStream BTIFile = new FileStream(filename, FileMode.Open);
            Read(BTIFile);
            BTIFile.Close();
            FileName = filename;
        }
        public BTI(Stream memorystream) => Read(memorystream);

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

        public override void Save(string filename)
        {
            FileStream BTIFile = new FileStream(filename, FileMode.Create);
            long BaseDataOffset = 0x20;
            Write(BTIFile, ref BaseDataOffset);
            BTIFile.Close();
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
            ushort ImageWidth = BitConverter.ToUInt16(BTIFile.ReadReverse(0, 2), 0);
            ushort ImageHeight = BitConverter.ToUInt16(BTIFile.ReadReverse(0, 2), 0);
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
                PaletteCount = BitConverter.ToInt16(BTIFile.ReadReverse(0, 2), 0);
                PaletteDataAddress = BitConverter.ToUInt32(BTIFile.ReadReverse(0, 4), 0);
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
            float LODBias = BitConverter.ToInt16(BTIFile.ReadReverse(0, 2), 0) / 100.0f;
            uint ImageDataAddress = BitConverter.ToUInt32(BTIFile.ReadReverse(0, 4), 0);

            BTIFile.Position = HeaderStart + ImageDataAddress;
            ushort ogwidth = ImageWidth, ogheight = ImageHeight;
            TexEntry current = new TexEntry()
            {
                Format = Format,
                PaletteFormat = PaletteFormat,
                LODBias = LODBias,
                MagnificationFilter = (GXFilterMode)MagnificationFilter,
                MinificationFilter = (GXFilterMode)MinificationFilter,
                WrapS = (GXWrapMode)WrapS,
                WrapT = (GXWrapMode)WrapT,
                EnableEdgeLOD = EnableEdgeLOD,
                MinLOD = MinLOD,
                MaxLOD = MaxLOD
            };
            for (int i = 0; i < TotalImageCount; i++)
            {
                current.Add(DecodeImage(BTIFile, PaletteData, Format, PaletteFormat, PaletteCount, ImageWidth, ImageHeight, i));

                //Modification: Required to hash!
                if (i == 0)
                {
                    long ImageEndAddress = BTIFile.Position - HeaderStart - ImageDataAddress;
                    BTIFile.Position = HeaderStart + ImageDataAddress;
                    current.RowDate = BTIFile.Read(0, (int)(ImageEndAddress));
                }
            }
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
            BTIFile.WriteReverse(BitConverter.GetBytes((ushort)this[0].Width), 0, 2);
            BTIFile.WriteReverse(BitConverter.GetBytes((ushort)this[0].Height), 0, 2);
            BTIFile.WriteByte((byte)WrapS);
            BTIFile.WriteByte((byte)WrapT);
            if (Format.IsPaletteFormat())
            {
                BTIFile.WriteByte(0x01);
                BTIFile.WriteByte((byte)PaletteFormat);
                BTIFile.WriteReverse(BitConverter.GetBytes((ushort)(PaletteData.Count/2)), 0, 2);
                BTIFile.WriteReverse(BitConverter.GetBytes(PaletteDataStart), 0, 4);
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
            BTIFile.WriteReverse(BitConverter.GetBytes((short)(LODBias * 100)), 0, 2);
            BTIFile.WriteReverse(BitConverter.GetBytes(ImageDataStart), 0, 4);

            long Pauseposition = BTIFile.Position;
            BTIFile.Position = DataOffset;

            BTIFile.Write(PaletteData.ToArray(), 0, PaletteData.Count);
            BTIFile.Write(ImageData.ToArray(), 0, ImageData.Count);
            DataOffset = BTIFile.Position;
            BTIFile.Position = Pauseposition;
        }

        //public bool ImageEquals(BTI Other)
        //{
        //    if (mipmaps.Count != Other.mipmaps.Count)
        //        return false;
        //    for (int i = 0; i < mipmaps.Count; i++)
        //    {
        //        if (!Compare(mipmaps[i], Other.mipmaps[i]))
        //            return false;
        //    }

        //   return true;
        //}
        //public override string ToString() => $"{FileName} - {Count} Image(s)";
        //public override bool Equals(object obj)
        //{
        //    return obj is BTI bTI && bTI != null &&
        //           FileName == bTI.FileName &&
        //           ImageEquals(bTI) &&
        //           Format == bTI.Format &&
        //           PaletteFormat == bTI.PaletteFormat &&
        //           AlphaSetting == bTI.AlphaSetting &&
        //           WrapS == bTI.WrapS &&
        //           WrapT == bTI.WrapT &&
        //           MagnificationFilter == bTI.MagnificationFilter &&
        //           MinificationFilter == bTI.MinificationFilter &&
        //           MinLOD == bTI.MinLOD &&
        //           MaxLOD == bTI.MaxLOD &&
        //           EnableEdgeLOD == bTI.EnableEdgeLOD &&
        //           LODBias == bTI.LODBias &&
        //           ClampLODBias == bTI.ClampLODBias &&
        //           MaxAnisotropy == bTI.MaxAnisotropy &&
        //           Count == bTI.Count;
        //}
        //public override int GetHashCode()
        //{
        //    var hashCode = 647188357;
        //    hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FileName);
        //    hashCode = hashCode * -1521134295 + EqualityComparer<List<Bitmap>>.Default.GetHashCode(mipmaps);
        //    hashCode = hashCode * -1521134295 + Format.GetHashCode();
        //    hashCode = hashCode * -1521134295 + PaletteFormat.GetHashCode();
        //    hashCode = hashCode * -1521134295 + AlphaSetting.GetHashCode();
        //    hashCode = hashCode * -1521134295 + WrapS.GetHashCode();
        //    hashCode = hashCode * -1521134295 + WrapT.GetHashCode();
        //    hashCode = hashCode * -1521134295 + MagnificationFilter.GetHashCode();
        //    hashCode = hashCode * -1521134295 + MinificationFilter.GetHashCode();
        //    hashCode = hashCode * -1521134295 + MinLOD.GetHashCode();
        //    hashCode = hashCode * -1521134295 + MaxLOD.GetHashCode();
        //    hashCode = hashCode * -1521134295 + EnableEdgeLOD.GetHashCode();
        //    hashCode = hashCode * -1521134295 + LODBias.GetHashCode();
        //    hashCode = hashCode * -1521134295 + ClampLODBias.GetHashCode();
        //    hashCode = hashCode * -1521134295 + MaxAnisotropy.GetHashCode();
        //    hashCode = hashCode * -1521134295 + Count.GetHashCode();
        //    return hashCode;
        //}
        //public static bool operator ==(BTI bTI1, BTI bTI2) => bTI1.Equals(bTI2);
        //public static bool operator !=(BTI bTI1, BTI bTI2) => !(bTI1 == bTI2);

            //=====================================================================

            /// <summary>
            /// Cast a RARCFile to a BTI
            /// </summary>
            /// <param name="x"></param>
        public static implicit operator BTI(RARC.RARC.File x) => new BTI((MemoryStream)x) { FileName = x.Name };
        /// <summary>
        /// Cast a BTI to a RARCfile
        /// </summary>
        /// <param name="x"></param>
        public static implicit operator RARC.RARC.File(BTI x)
        {
            MemoryStream MS = new MemoryStream();
            long temp = 0;
            x.Write(MS, ref temp);
            return new RARC.RARC.File(x.FileName, MS);
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