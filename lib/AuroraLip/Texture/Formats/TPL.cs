using AuroraLib.Common;
using System.Drawing;

namespace AuroraLib.Texture.Formats
{
    /*
    * Super Hackio Incorporated
    * "Copyright © Super Hackio Incorporated 2020-2021"
    * https://github.com/SuperHackio/Hack.io
    */

    public class TPL : JUTTexture, IFileAccess
    {
        public static readonly byte[] Magic = new byte[4] { 0x00, 0x20, 0xAF, 0x30 };

        public bool CanRead => true;

        public bool CanWrite => true;

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
            => stream.Length > 12 && stream.ReadByte() == Magic[0] && stream.ReadByte() == Magic[1] && stream.ReadByte() == Magic[2] && stream.ReadByte() == Magic[3];

        public TPL() : base() { }
        public TPL(string filepath) : base(filepath) { }
        public TPL(Stream stream) => Read(stream);

        // Helper function to read a TPL stream
        // exposed so other classes that wish to read
        // a TPL can do so
        public static void ProcessStream(Stream stream, long HeaderStart, List<TexEntry> textures)
        {
            int TotalImageCount = stream.ReadInt32(Endian.Big);
            int ImageTableOffset = stream.ReadInt32(Endian.Big);

            stream.Seek(HeaderStart + ImageTableOffset, SeekOrigin.Begin);

            List<KeyValuePair<int, int>> ImageHeaderOffset = new List<KeyValuePair<int, int>>();
            for (int i = 0; i < TotalImageCount; i++)
                ImageHeaderOffset.Add(new KeyValuePair<int, int>(stream.ReadInt32(Endian.Big), stream.ReadInt32(Endian.Big)));

            for (int i = 0; i < ImageHeaderOffset.Count; i++)
            {
                short PaletteCount = 0;
                uint PaletteDataAddress = 0;
                byte[] PaletteData = null;
                GXPaletteFormat PaletteFormat = GXPaletteFormat.IA8;
                if (ImageHeaderOffset[i].Value != 0)
                {
                    stream.Seek(HeaderStart + ImageHeaderOffset[i].Value, SeekOrigin.Begin);
                    PaletteCount = stream.ReadInt16(Endian.Big);
                    stream.Position += 0x02;
                    PaletteFormat = (GXPaletteFormat)stream.ReadUInt32(Endian.Big);
                    PaletteDataAddress = stream.ReadUInt32(Endian.Big);
                    stream.Seek(HeaderStart + PaletteDataAddress, SeekOrigin.Begin);
                    PaletteData = stream.Read(PaletteCount * 2);
                }

                stream.Seek(HeaderStart + ImageHeaderOffset[i].Key, SeekOrigin.Begin);

                ushort ImageHeight = stream.ReadUInt16(Endian.Big);
                ushort ImageWidth = stream.ReadUInt16(Endian.Big);
                GXImageFormat Format = (GXImageFormat)stream.ReadUInt32(Endian.Big);
                uint ImageDataAddress = stream.ReadUInt32(Endian.Big);
                uint WrapS = stream.ReadUInt32(Endian.Big);
                uint WrapT = stream.ReadUInt32(Endian.Big);
                uint MinFilter = stream.ReadUInt32(Endian.Big);
                uint MaxFilter = stream.ReadUInt32(Endian.Big);
                float LODBias = stream.ReadSingle(Endian.Big);
                bool EnableEdgeLOD = stream.ReadByte() > 0;
                byte MinLOD = (byte)stream.ReadByte();
                byte MaxLOD = (byte)stream.ReadByte();
                byte Unpacked = (byte)stream.ReadByte();

                stream.Seek(HeaderStart + ImageDataAddress, SeekOrigin.Begin);
                TexEntry current = new TexEntry(stream, PaletteData, Format, PaletteFormat, PaletteCount, ImageWidth, ImageHeight, MaxLOD)
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
                textures.Add(current);
            }
        }

        protected override void Read(Stream stream)
        {
            long HeaderStart = stream.Position;

            if (!IsMatch(stream))
                throw new InvalidIdentifierException("0x0020AF30");

            ProcessStream(stream, HeaderStart, this);
        }

        protected override void Write(Stream stream)
        {
            long HeaderStart = stream.Position;
            stream.Write(Magic, 0, Magic.Length);

            stream.WriteBigEndian(BitConverter.GetBytes(Count), 4);
            stream.WriteBigEndian(BitConverter.GetBytes(0x0C), 4);
            long OffsetLocation = stream.Position;
            //Placeholders...Not my usual 0xDD placeholder because there's no way I'm able to calculate the byte to use
            //Well, there is but eeehhhh it's slow(?)
            stream.Write(new byte[Count * 8], 0, Count * 8);
            //This will hold the information that we're gonna write back here
            //Key = ImageDataOffset
            //Value = PalleteDataOffset
            List<KeyValuePair<int, int>> ImageHeaderOffset = new List<KeyValuePair<int, int>>();

            //TPLFile.PadTo(32);
            for (int i = 0; i < Count; i++)
            {
                bool IsPalette = this[i].Format.IsPaletteFormat();
                int PaletteHeader = !IsPalette ? 0 : (int)(stream.Position - HeaderStart);

                //List<byte> ImageData = new List<byte>();
                //List<Bitmap> mips = this[i];
                //GetImageAndPaletteData(ref ImageData, out byte[] PaletteData, mips, this[i].Format, this[i].PaletteFormat);

                if (IsPalette)
                {
                    stream.WriteBigEndian(BitConverter.GetBytes((ushort)(this[i].Palettes.Sum(p => p.Size) / 2)), 2);
                    stream.WriteByte(0x00);
                    stream.WriteByte(0x00);
                    stream.WriteBigEndian(BitConverter.GetBytes((int)this[i].PaletteFormat), 4);
                    long temp = stream.Position;
                    stream.WriteBigEndian(BitConverter.GetBytes(-572662307), 4);
                    stream.WritePadding(32);
                    long temp2 = stream.Position;
                    stream.Position = temp;
                    stream.WriteBigEndian(BitConverter.GetBytes((int)(temp2 - HeaderStart) + 0), 4);
                    stream.Position = temp2;
                    foreach (var Palette in this[i].Palettes)
                        stream.Write(Palette.GetBytes());
                    stream.WritePadding(32);
                }

                int ImageHeader = ImageHeader = (int)(stream.Position - HeaderStart);

                stream.WriteBigEndian(BitConverter.GetBytes((ushort)this[i].ImageHeight), 2);
                stream.WriteBigEndian(BitConverter.GetBytes((ushort)this[i].ImageWidth), 2);
                stream.WriteBigEndian(BitConverter.GetBytes((uint)this[i].Format), 4);
                long comebackhere = stream.Position;
                stream.WriteBigEndian(BitConverter.GetBytes(-572662307), 4);
                stream.WriteBigEndian(BitConverter.GetBytes((uint)this[i].WrapS), 4);
                stream.WriteBigEndian(BitConverter.GetBytes((uint)this[i].WrapT), 4);
                stream.WriteBigEndian(BitConverter.GetBytes((uint)this[i].MinificationFilter), 4);
                stream.WriteBigEndian(BitConverter.GetBytes((uint)this[i].MagnificationFilter), 4);
                stream.WriteBigEndian(BitConverter.GetBytes(this[i].LODBias), 4);
                stream.WriteByte((byte)(this[i].EnableEdgeLOD ? 0x01 : 0x00));
                stream.WriteByte(0x00);
                stream.WriteByte((byte)(this[i].Count - 1));
                //Unpacked - Rii told me to leave this one as 0 :)
                stream.WriteByte(0x00);
                stream.WritePadding(32);
                long PausePosition = stream.Position;
                stream.Position = comebackhere;
                stream.WriteBigEndian(BitConverter.GetBytes((int)(PausePosition - HeaderStart)), 4);
                stream.Position = PausePosition;
                foreach (var bytes in this[0].RawImages)
                {
                    stream.Write(bytes);
                }
                stream.WritePadding(32);

                ImageHeaderOffset.Add(new KeyValuePair<int, int>(ImageHeader, PaletteHeader));
            }
            stream.Position = OffsetLocation;
            for (int i = 0; i < Count; i++)
            {
                stream.WriteBigEndian(BitConverter.GetBytes(ImageHeaderOffset[i].Key), 4);
                stream.WriteBigEndian(BitConverter.GetBytes(ImageHeaderOffset[i].Value), 4);
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
        public static TPL Create(Bitmap Image, GXImageFormat ImageFormat = GXImageFormat.CMPR, GXPaletteFormat PaletteFormat = GXPaletteFormat.IA8)
        {
            TPL NewTPL = new TPL
            {
                new TexEntry((Bitmap)Image.Clone(), ImageFormat, PaletteFormat)
            };
            return NewTPL;
        }
        /// <summary>
        /// Creates a TPL using a Bitmap Array (Multiple Textures, No Mipmaps)
        /// </summary>
        /// <param name="Images"></param>
        /// <param name="ImageFormat"></param>
        /// <param name="PaletteFormat"></param>
        /// <returns></returns>
        public static TPL Create(Bitmap[] Images, GXImageFormat ImageFormat = GXImageFormat.CMPR, GXPaletteFormat PaletteFormat = GXPaletteFormat.IA8)
        {
            TPL NewTPL = new TPL();
            for (int i = 0; i < Images.Length; i++)
                NewTPL.Add(new TexEntry((Bitmap)Images[i].Clone(), ImageFormat, PaletteFormat));
            return NewTPL;
        }

    }
}
