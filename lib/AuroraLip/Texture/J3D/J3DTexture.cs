using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace AuroraLip.Texture.J3D
{

    /*
    * Super Hackio Incorporated
    * "Copyright © Super Hackio Incorporated 2020-2021"
    * https://github.com/SuperHackio/Hack.io
    */

    public static partial class JUtility
    {
        #region FormatInfo

        private static readonly int[] BlockWidth = { 8, 8, 8, 4, 4, 4, 4, 0, 8, 8, 4, 0, 0, 0, 8 };
        private static readonly int[] BlockHeight = { 8, 4, 4, 4, 4, 4, 4, 0, 8, 4, 4, 0, 0, 0, 8 };
        private static readonly int[] Bpp = { 4, 8, 8, 16, 16, 16, 32, 0, 4, 8, 16, 0, 0, 0, 4 };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static void GetBlockSize(GXImageFormat format, out int blockWidth, out int blockHeight)
        {
            blockWidth = BlockWidth[(int)format];
            blockHeight = BlockHeight[(int)format];
        }

        public static (int, int) GetBlockSize(GXImageFormat format) => (BlockWidth[(int)format], BlockHeight[(int)format]);
        public static int GetBlockWidth(GXImageFormat format) => BlockWidth[(int)format];
        public static int GetBlockHeight(GXImageFormat format) => BlockHeight[(int)format];
        public static int GetBpp(GXImageFormat Format) { return Bpp[(int)Format]; }

        public static bool IsPaletteFormat(this GXImageFormat Format) => Format == GXImageFormat.C4 || Format == GXImageFormat.C8 || Format == GXImageFormat.C14X2;

        public static int GetCalculatedDataSize(GXImageFormat Format, int Width, int Height)
        {
            while ((Width % BlockWidth[(int)Format]) != 0) Width++;
            while ((Height % BlockHeight[(int)Format]) != 0) Height++;
            return Width * Height * GetBpp(Format) / 8;
        }

        public static int GetMipmapsFromSize(GXImageFormat Format, int Size, int Width, int Height)
        {
            int mips = 0;
            while (Size > 0 && Width != 0 && Height != 0)
            {
                Size -= GetCalculatedDataSize(Format, Width, Height);
                Width /= 2;
                Height /= 2;
                mips++;
            }
            return mips-1;
        }

        public static int GetMaxColours(this GXImageFormat Format)
        {
            switch (Format)
            {
                case GXImageFormat.C4:
                    return 1 << 4; //16
                case GXImageFormat.C8:
                    return 1 << 8; //256
                case GXImageFormat.C14X2:
                    return 1 << 14; //16384
                default:
                    throw new Exception("Not a Palette format!");
            }
        }
        #endregion

        #region Decode

        public static Bitmap DecodeImage(Stream TextureFile, in byte[] PaletteData, GXImageFormat Format, GXPaletteFormat? PaletteFormat, int? PaletteCount, int ImageWidth, int ImageHeight, int Mipmap = 0)
        {
            for (int i = 0; i < Mipmap; i++)
            {
                ImageWidth /= 2;
                ImageHeight /= 2;
            }
            return DecodeImage(TextureFile.Read(GetCalculatedDataSize(Format, ImageWidth, ImageHeight)), PaletteData, Format, PaletteFormat, PaletteCount, ImageWidth, ImageHeight);
        }

        public static Bitmap DecodeImage(in byte[] ImageData, in byte[] PaletteData, GXImageFormat Format, GXPaletteFormat? PaletteFormat, int? PaletteCount, int ImageWidth, int ImageHeight)
        {
            Color[] PaletteColours = null;
            if (IsPaletteFormat(Format) && PaletteData != null && PaletteData.Length > 0)
            {
                PaletteColours = DecodePalette(PaletteData, PaletteFormat, PaletteCount, Format);
            }
            return DecodeImage(ImageData, PaletteColours, Format, ImageWidth, ImageHeight);
        }

        public static Bitmap DecodeImage(in byte[] ImageData, Color[] PaletteColours, GXImageFormat Format, int Width, int Height)
        {
            var (BlockWidth, BlockHeight) = GetBlockSize(Format);
            int BlockDataSize = Format == GXImageFormat.RGBA32 ? 64 : 32, offset = 0, BlockX = 0, BlockY = 0, XInBlock = 0, YInBlock = 0;

            byte[] Pixels = new byte[Width * Height * 4];
            while (BlockY < Height)
            {
                Color[] PixelData = DecodeBlock(ImageData, Format, offset, BlockDataSize, PaletteColours);

                for (int i = 0; i < PixelData.Length; i++)
                {
                    XInBlock = (i % BlockWidth);
                    YInBlock = i / BlockWidth;
                    int xpos = BlockX + XInBlock;
                    int ypos = BlockY + YInBlock;
                    if (xpos >= Width || ypos >= Height)
                        continue;
                    int Start = ((ypos * Width) + xpos) * 4;

                    Pixels[Start] = PixelData[i].B;
                    Pixels[Start + 1] = PixelData[i].G;
                    Pixels[Start + 2] = PixelData[i].R;
                    Pixels[Start + 3] = PixelData[i].A;
                }
                offset += BlockDataSize;
                BlockX += BlockWidth;
                if (BlockX >= Width)
                {
                    BlockX = 0;
                    BlockY += BlockHeight;
                }
            }
            return Pixels.ToBitmap(Width, Height);
        }

        public static Color[] DecodePalette(in byte[] PaletteData, GXPaletteFormat? PaletteFormat, int? Count, GXImageFormat Format)
        {
            List<Color> Colours = new List<Color>();
            int offset = 0;
            for (int i = 0; i < Count; i++)
            {
                ushort Raw = BitConverter.ToUInt16(new byte[2] { PaletteData[offset + 1], PaletteData[offset] }, 0);
                offset += 2;
                switch (PaletteFormat)
                {
                    case GXPaletteFormat.IA8:
                        Colours.Add(IA8ToColor(Raw));
                        break;
                    case GXPaletteFormat.RGB565:
                        Colours.Add(RGB565ToColor(Raw));
                        break;
                    case GXPaletteFormat.RGB5A3:
                        Colours.Add(RGB5A3ToColor(Raw));
                        break;
                    default:
                        throw new Exception("Bad Palette format");
                }
            }

            return Colours.ToArray();
        }

        public static Color[] DecodeBlock(in byte[] ImageData, GXImageFormat Format, int Offset, int BlockSize, Color[] Colours)
        {
            List<Color> Result = new List<Color>();
            if (Offset >= ImageData.Length)
                return Result.ToArray();

            int BlockSizeHalfed = (int)Math.Floor(BlockSize / 2.0);
            switch (Format)
            {
                case GXImageFormat.I4:
                    for (int i = 0; i < BlockSize; i++)
                        for (int nibble = 0; nibble < 2; nibble++)
                            if (Offset + i < ImageData.Length)
                                Result.Add(I4ToColor((byte)((ImageData[Offset + i] >> (1 - nibble) * 4) & 0xF)));
                    break;
                case GXImageFormat.I8:
                    for (int i = 0; i < BlockSize; i++)
                        if (Offset + i < ImageData.Length)
                            Result.Add(I8ToColor(ImageData[Offset + i]));
                    break;
                case GXImageFormat.IA4:
                    for (int i = 0; i < BlockSize; i++)
                        if (Offset + i < ImageData.Length)
                            Result.Add(IA4ToColor(ImageData[Offset + i]));
                    break;
                case GXImageFormat.IA8:
                    for (int i = 0; i < BlockSizeHalfed; i++)
                        if (Offset + i * 2 < ImageData.Length)
                            Result.Add(IA8ToColor(BitConverter.ToUInt16(new byte[2] { ImageData[(Offset + i * 2) + 1], ImageData[Offset + i * 2] }, 0)));
                    break;
                case GXImageFormat.RGB565:
                    for (int i = 0; i < BlockSizeHalfed; i++)
                        if (Offset + i * 2 < ImageData.Length)
                            Result.Add(RGB565ToColor(BitConverter.ToUInt16(new byte[2] { ImageData[(Offset + i * 2) + 1], ImageData[Offset + i * 2] }, 0)));
                    break;
                case GXImageFormat.RGB5A3:
                    for (int i = 0; i < BlockSizeHalfed; i++)
                        if (Offset + i * 2 < ImageData.Length)
                            Result.Add(RGB5A3ToColor(BitConverter.ToUInt16(new byte[2] { ImageData[(Offset + i * 2) + 1], ImageData[Offset + i * 2] }, 0)));
                    break;
                case GXImageFormat.RGBA32:
                    for (int i = 0; i < 16; i++)
                        Result.Add(Color.FromArgb(ImageData[Offset + (i * 2)], ImageData[Offset + (i * 2) + 1], ImageData[Offset + (i * 2) + 32], ImageData[Offset + (i * 2) + 33]));
                    break;
                case GXImageFormat.C4:
                    for (int i = 0; i < BlockSize; i++)
                        for (int nibble = 0; nibble < 2; nibble++)
                        {
                            int Value = (ImageData[Offset + i] >> (1 - nibble) * 4) & 0xF;
                            Result.Add(Value >= Colours.Length ? Color.Black : Colours[Value]);
                        }
                    break;
                case GXImageFormat.C8:
                    for (int i = 0; i < BlockSize; i++)
                        Result.Add(ImageData[Offset + i] >= Colours.Length ? Color.Black : Colours[ImageData[Offset + i]]);
                    break;
                case GXImageFormat.C14X2:
                    for (int i = 0; i < BlockSizeHalfed; i++)
                    { 
                        int ColourIndex = BitConverter.ToUInt16(new byte[2] { ImageData[(Offset + i * 2) + 1], ImageData[Offset + i * 2] }, 0);
                        if (ColourIndex > Colours.Length)
                            ColourIndex = 0;
                        Result.Add(Colours[ColourIndex]);
                    }
                    break;
                case GXImageFormat.CMPR:
                    Result.AddRange(new Color[64]);
                    int subblock_offset = Offset;
                    for (int i = 0; i < 4; i++)
                    {
                        int subblock_x = (i % 2) * 4;
                        int subblock_y = ((int)Math.Floor(i / 2.0)) * 4;

                        Colours = GetInterpolatedDXT1Colours(BitConverter.ToUInt16(new byte[2] { ImageData[subblock_offset + 1], ImageData[subblock_offset] }, 0), BitConverter.ToUInt16(new byte[2] { ImageData[subblock_offset + 3], ImageData[subblock_offset + 2] }, 0));
                        for (int j = 0; j < 16; j++)
                            Result[subblock_x + subblock_y * 8 + ((int)Math.Floor(j / 4.0)) * 8 + (j % 4)] = Colours[(((BitConverter.ToInt32(new byte[4] { ImageData[subblock_offset + 7], ImageData[subblock_offset + 6], ImageData[subblock_offset + 5], ImageData[subblock_offset + 4] }, 0)) >> ((15 - j) * 2)) & 3)];
                        subblock_offset += 8;
                    }

                    break;
                default:
                    throw new Exception("Invalid Image Format");
            }
            return Result.ToArray();
        }

        public static Color I4ToColor(byte Raw)
        {
            int val = (Raw << 4) | (Raw);
            return Color.FromArgb(val, val, val, val);
        }

        public static Color IA4ToColor(byte Raw)
        {
            int low_nibble = ((Raw & 0xF) << 4) | Raw & 0xF;
            return Color.FromArgb((((Raw >> 4) & 0xF) << 4) | ((Raw >> 4) & 0xF), low_nibble, low_nibble, low_nibble);
        }

        public static Color I8ToColor(byte Raw) => Color.FromArgb(Raw, Raw, Raw, Raw);

        public static Color IA8ToColor(ushort Raw)
        {
            int low_byte = Raw & 0xFF;
            return Color.FromArgb((Raw >> 8) & 0xFF, low_byte, low_byte, low_byte);
        }

        public static Color RGB565ToColor(ushort Raw)
        {
            int Red, Green, Blue, Value;
            Value = ((Raw >> 11) & 0x1F);
            Red = (Value << 3) | (Value >> 2);
            Value = ((Raw >> 5) & 0x3F);
            Green = (Value << 2) | (Value >> 4);
            Value = ((Raw >> 0) & 0x1F);
            Blue = (Value << 3) | (Value >> 2);
            return Color.FromArgb(Red, Green, Blue);
        }

        public static Color RGB5A3ToColor(ushort Raw)
        {
            int Red, Green, Blue, Value;
            if ((Raw & 0x8000) == 0)
            {
                Value = ((Raw >> 8) & 0xF);
                Red = (Value << 4) | (Value >> 0);
                Value = ((Raw >> 4) & 0xF);
                Green = (Value << 4) | (Value >> 0);
                Value = ((Raw >> 0) & 0xF);
                Blue = (Value << 4) | (Value >> 0);
                Value = ((Raw >> 12) & 0x7);
                int Alpha = (Value << 5) | (Value << 2) | (Value >> 1);
                return Color.FromArgb(Alpha, Red, Green, Blue);
            }
            else
            {
                Value = ((Raw >> 10) & 0x1F);
                Red = (Value << 3) | (Value >> 2);
                Value = ((Raw >> 5) & 0x1F);
                Green = (Value << 3) | (Value >> 2);
                Value = ((Raw >> 0) & 0x1F);
                Blue = (Value << 3) | (Value >> 2);
                return Color.FromArgb(Red, Green, Blue);
            }
        }

        public static Color[] GetInterpolatedDXT1Colours(ushort RawLeft, ushort RawRight)
        {
            Color Left = RGB565ToColor(RawLeft), Right = RGB565ToColor(RawRight);
            Color InterpA, InterpB;

            if (RawLeft > RawRight)
            {
                InterpA = Color.FromArgb((int)Math.Floor((2 * Left.R + 1 * Right.R) / 3.0), (int)Math.Floor((2 * Left.G + 1 * Right.G) / 3.0), (int)Math.Floor((2 * Left.B + 1 * Right.B) / 3.0));
                InterpB = Color.FromArgb((int)Math.Floor((1 * Left.R + 2 * Right.R) / 3.0), (int)Math.Floor((1 * Left.G + 2 * Right.G) / 3.0), (int)Math.Floor((1 * Left.B + 2 * Right.B) / 3.0));
            }
            else
            {
                InterpA = Color.FromArgb((int)Math.Floor(Left.R / 2.0) + (int)Math.Floor(Right.R / 2.0), (int)Math.Floor(Left.G / 2.0) + (int)Math.Floor(Right.G / 2.0), (int)Math.Floor(Left.B / 2.0) + (int)Math.Floor(Right.B / 2.0));
                InterpB = Color.FromArgb(1, (int)Math.Floor((1 * Left.R + 2 * Right.R) / 3.0), (int)Math.Floor((1 * Left.G + 2 * Right.G) / 3.0), (int)Math.Floor((1 * Left.B + 2 * Right.B) / 3.0));
            }

            return new Color[4] { Left, Right, InterpA, InterpB };
        }
        #endregion

        /// <summary>
        /// Use in the context of mipmaps
        /// </summary>
        /// <param name="ImageData"></param>
        /// <param name="PaletteData"></param>
        /// <param name="Images"></param>
        /// <param name="Format"></param>
        /// <param name="PaletteFormat"></param>
        /// <param name="AlphaMode"></param>
        public static void GetImageAndPaletteData(ref List<byte> ImageData, ref List<byte> PaletteData, List<Bitmap> Images, GXImageFormat Format, GXPaletteFormat PaletteFormat)
        {
            Tuple<Dictionary<Color, int>, ushort[]> Palette = CreatePalette(Images, Format, PaletteFormat);
            PaletteData = EncodePalette(Palette.Item2, Format).ToList();
            for (int i = 0; i < Images.Count; i++)
                EncodeImage(ref ImageData, Images[i], Format, Palette.Item1);
        }
        /// <summary>
        /// Use in the context of no mipmaps
        /// </summary>
        /// <param name="ImageData"></param>
        /// <param name="PaletteData"></param>
        /// <param name="Image"></param>
        /// <param name="Format"></param>
        /// <param name="PaletteFormat"></param>
        public static void GetImageAndPaletteData(ref List<byte> ImageData, ref List<byte> PaletteData, Bitmap Image, GXImageFormat Format, GXPaletteFormat PaletteFormat)
        {
            Tuple<Dictionary<Color, int>, ushort[]> Palette = Format.IsPaletteFormat() ? CreatePalette(Image, Format, PaletteFormat) : new Tuple<Dictionary<Color, int>, ushort[]>(null, null);
            PaletteData = EncodePalette(Palette.Item2, Format).ToList();
            EncodeImage(ref ImageData, Image, Format, Palette.Item1);
        }

        public static void EncodeImage(ref List<byte> ImageData, Bitmap Image, GXImageFormat Format, Dictionary<Color, int> ColourIndicies)
        {
            var (BlockWidth, BlockHeight) = GetBlockSize(Format);
            int block_x = 0, block_y = 0;
            byte[] Pixels = Image.ToByteArray();

            while (block_y < Image.Height)
            {
                byte[] block_data = EncodeBlock(Format, Pixels, Image, ColourIndicies, block_x, block_y);

                ImageData.AddRange(block_data);

                block_x += BlockWidth;
                if (block_x >= Image.Width)
                {
                    block_x = 0;
                    block_y += BlockHeight;
                }
            }
        }

        public static Tuple<Dictionary<Color, int>, ushort[]> CreatePalette(List<Bitmap> Images, GXImageFormat Format, GXPaletteFormat PaletteFormat)
        {
            if (!(Format == GXImageFormat.C4 || Format == GXImageFormat.C8 || Format == GXImageFormat.C14X2))
                return new Tuple<Dictionary<Color, int>, ushort[]>(null, null);

            List<byte[]> ImageData = new List<byte[]>();
            List<ushort> encoded_colors = new List<ushort>();
            Dictionary<Color, int> colors_to_color_indexes = new Dictionary<Color, int>();
            List<Color> colours = new List<Color>();
            for (int i = 0; i < Images.Count; i++)
            {
                ImageData.Add(Images[i].ToByteArray());
                for (int y = 0; y < Images[i].Height; y++)
                {
                    for (int x = 0; x < Images[i].Width; x++)
                    {
                        int z = ((y * Images[i].Width) + x) * 4;
                        Color Col = Color.FromArgb(ImageData[i][z + 3], ImageData[i][z + 2], ImageData[i][z + 1], ImageData[i][z]);
                        if (!colours.Contains(Col))
                            colours.Add(Col);
                    }
                }
            }
            colours = colours.OrderBy(C => C.R).ToList();
            for (int i = 0; i < colours.Count; i++)
            {
                Color Col = colours[i];
                ushort ColEncoded = EncodeColour(Col, PaletteFormat);
                if (!encoded_colors.Contains(ColEncoded))
                    encoded_colors.Add(ColEncoded);
                if (!colors_to_color_indexes.ContainsKey(Col))
                    colors_to_color_indexes.Add(Col, encoded_colors.IndexOf(ColEncoded));
            }

            if (encoded_colors.Count > GetMaxColours(Format))
            {
                // If the image has more colors than the selected image format can support, we automatically reduce the number of colors.
                // For C4 and C8, the colors should have already been reduced by Pillow's quantize method.
                // So the maximum number of colors can only be exceeded for C14X2.

                Color[] LimitedPalette = CreateLimitedPalette(Images, GetMaxColours(Format), PaletteFormat != GXPaletteFormat.RGB565);
                encoded_colors = new List<ushort>();
                colors_to_color_indexes = new Dictionary<Color, int>();
                for (int i = 0; i < Images.Count; i++)
                    for (int y = 0; y < Images[i].Height; y++)
                    {
                        for (int x = 0; x < Images[i].Width; x++)
                        {
                            int z = ((y * Images[i].Width) + x) * 4;
                            Color Col = Color.FromArgb(ImageData[i][z + 3], ImageData[i][z + 2], ImageData[i][z + 1], ImageData[i][z]);
                            ushort ColEncoded = EncodeColour(GetNearestColour(Col, LimitedPalette), PaletteFormat);
                            if (!encoded_colors.Contains(ColEncoded))
                                encoded_colors.Add(ColEncoded);
                            if (!colors_to_color_indexes.ContainsKey(Col))
                                colors_to_color_indexes.Add(Col, encoded_colors.IndexOf(ColEncoded));
                        }
                    }
            }
            return new Tuple<Dictionary<Color, int>, ushort[]>(colors_to_color_indexes, encoded_colors.ToArray());
        }

        public static Tuple<Dictionary<Color, int>, ushort[]> CreatePalette(Bitmap Image, GXImageFormat Format, GXPaletteFormat PaletteFormat)
        {
            if (!(Format == GXImageFormat.C4 || Format == GXImageFormat.C8 || Format == GXImageFormat.C14X2))
                return new Tuple<Dictionary<Color, int>, ushort[]>(null, null);

            List<byte> ImageData = new List<byte>();
            List<ushort> encoded_colors = new List<ushort>();
            Dictionary<Color, int> colors_to_color_indexes = new Dictionary<Color, int>();

            ImageData.AddRange(Image.ToByteArray());
            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width; x++)
                {
                    int z = ((y * Image.Width) + x) * 4;
                    Color Col = Color.FromArgb(ImageData[z + 3], ImageData[z + 2], ImageData[z + 1], ImageData[z]);
                    ushort ColEncoded = EncodeColour(Col, PaletteFormat);
                    if (!encoded_colors.Contains(ColEncoded))
                        encoded_colors.Add(ColEncoded);
                    if (!colors_to_color_indexes.ContainsKey(Col))
                        colors_to_color_indexes.Add(Col, encoded_colors.IndexOf(ColEncoded));
                }
            }

            if (encoded_colors.Count > GetMaxColours(Format))
            {
                // If the image has more colors than the selected image format can support, we automatically reduce the number of colors.
                //For C4 and C8, the colors should have already been reduced by Pillow's quantize method.
                // So the maximum number of colors can only be exceeded for C14X2.

                Color[] LimitedPalette = CreateLimitedPalette(Image, GetMaxColours(Format), PaletteFormat != GXPaletteFormat.RGB565);
                encoded_colors = new List<ushort>();
                colors_to_color_indexes = new Dictionary<Color, int>();

                for (int y = 0; y < Image.Height; y++)
                {
                    for (int x = 0; x < Image.Width; x++)
                    {
                        int z = ((y * Image.Width) + x) * 4;
                        Color Col = Color.FromArgb(ImageData[z + 3], ImageData[z + 2], ImageData[z + 1], ImageData[z]);
                        ushort ColEncoded = EncodeColour(GetNearestColour(Col, LimitedPalette), PaletteFormat);
                        if (!encoded_colors.Contains(ColEncoded))
                            encoded_colors.Add(ColEncoded);
                        if (!colors_to_color_indexes.ContainsKey(Col))
                            colors_to_color_indexes.Add(Col, encoded_colors.IndexOf(ColEncoded));
                    }
                }
            }
            return new Tuple<Dictionary<Color, int>, ushort[]>(colors_to_color_indexes, encoded_colors.ToArray());
        }

        public static byte[] EncodeBlock(GXImageFormat Format, byte[] Pixels, Bitmap Image, Dictionary<Color, int> ColourIndicies, int BlockX, int BlockY)
        {
            byte[] EncodedBlock = new byte[Format == GXImageFormat.RGBA32 ? 64 : 32];
            var (CurrentBlockWidth, CurrentBlockHeight) = GetBlockSize(Format);
            int Offset = 0;

            int PixelIndex;
            byte[] Value;
            switch (Format)
            {
                case GXImageFormat.I4:
                    #region Encode I4
                    for (int y = BlockY; y < BlockY + CurrentBlockHeight; y++)
                    {
                        for (int x = BlockX; x < BlockX + CurrentBlockWidth; x += 2)
                        {
                            int RawColL, RawColR;
                            if (x >= Image.Width || y >= Image.Height)
                                RawColL = 0xF; //Block Bleeds past image width
                            else
                            {
                                PixelIndex = ((y * Image.Width) + x) * 4;
                                RawColL = Color.FromArgb(Pixels[PixelIndex + 3], Pixels[PixelIndex + 2], Pixels[PixelIndex + 1], Pixels[PixelIndex]).ToI4();
                            }
                            if ((x + 1) >= Image.Width || y >= Image.Height)
                                RawColR = 0xF; //Block Bleeds past image width
                            else
                            {
                                PixelIndex = ((y * Image.Width) + (x + 1)) * 4;
                                RawColR = Color.FromArgb(Pixels[PixelIndex + 3], Pixels[PixelIndex + 2], Pixels[PixelIndex + 1], Pixels[PixelIndex]).ToI4();
                            }
                            EncodedBlock[Offset++] = (byte)(((RawColL & 0xF) << 4) | (RawColR & 0xF));
                        }
                    }
                    #endregion
                    break;
                case GXImageFormat.I8:
                    #region Encode I8
                    for (int y = BlockY; y < BlockY + CurrentBlockHeight; y++)
                    {
                        for (int x = BlockX; x < BlockX + CurrentBlockWidth; x++)
                        {
                            if (x >= Image.Width || y >= Image.Height)
                                EncodedBlock[Offset++] = 0xFF; //Block Bleeds past image width
                            else
                            {
                                PixelIndex = ((y * Image.Width) + x) * 4;
                                EncodedBlock[Offset++] = Color.FromArgb(Pixels[PixelIndex + 3], Pixels[PixelIndex + 2], Pixels[PixelIndex + 1], Pixels[PixelIndex]).ToI8();
                            }
                        }
                    }
                    #endregion
                    break;
                case GXImageFormat.IA4:
                    #region Encode IA4
                    for (int y = BlockY; y < BlockY + CurrentBlockHeight; y++)
                    {
                        for (int x = BlockX; x < BlockX + CurrentBlockWidth; x++)
                        {
                            if (x >= Image.Width || y >= Image.Height)
                                EncodedBlock[Offset++] = 0xFF; //Block Bleeds past image width
                            else
                            {
                                PixelIndex = ((y * Image.Width) + x) * 4;
                                EncodedBlock[Offset++] = Color.FromArgb(Pixels[PixelIndex + 3], Pixels[PixelIndex + 2], Pixels[PixelIndex + 1], Pixels[PixelIndex]).ToIA4();
                            }
                        }
                    }
                    #endregion
                    break;
                case GXImageFormat.IA8:
                    #region Encode IA8
                    for (int y = BlockY; y < BlockY + CurrentBlockHeight; y++)
                    {
                        for (int x = BlockX; x < BlockX + CurrentBlockWidth; x++)
                        {
                            if (x >= Image.Width || y >= Image.Height)
                                Value = new byte[2] { 0xFF, 0xFF }; //Block Bleeds past image width
                            else
                            {
                                PixelIndex = ((y * Image.Width) + x) * 4;
                                Value = BitConverter.GetBytes(Color.FromArgb(Pixels[PixelIndex + 3], Pixels[PixelIndex + 2], Pixels[PixelIndex + 1], Pixels[PixelIndex]).ToIA8());
                            }
                            EncodedBlock[Offset++] = Value[1];
                            EncodedBlock[Offset++] = Value[0];
                        }
                    }
                    #endregion
                    break;
                case GXImageFormat.RGB565:
                    #region Encode RGB565
                    for (int y = BlockY; y < BlockY + CurrentBlockHeight; y++)
                    {
                        for (int x = BlockX; x < BlockX + CurrentBlockWidth; x++)
                        {
                            if (x >= Image.Width || y >= Image.Height)
                                Value = new byte[2] { 0xFF, 0xFF }; //Block Bleeds past image width
                            else
                            {
                                PixelIndex = ((y * Image.Width) + x) * 4;
                                if (PixelIndex >= Pixels.Length)
                                    goto e;
                                Value = BitConverter.GetBytes(Color.FromArgb(Pixels[PixelIndex + 3], Pixels[PixelIndex + 2], Pixels[PixelIndex + 1], Pixels[PixelIndex]).ToRGB565());
                            }
                            EncodedBlock[Offset++] = Value[1];
                            EncodedBlock[Offset++] = Value[0];
                        }
                    }
                e:
                    #endregion
                    break;
                case GXImageFormat.RGB5A3:
                    #region Encode RGB5A3
                    for (int y = BlockY; y < BlockY + CurrentBlockHeight; y++)
                    {
                        for (int x = BlockX; x < BlockX + CurrentBlockWidth; x++)
                        {
                            if (x >= Image.Width || y >= Image.Height)
                                Value = new byte[2] { 0xFF, 0xFF }; //Block Bleeds past image width
                            else
                            {
                                PixelIndex = ((y * Image.Width) + x) * 4;
                                Value = BitConverter.GetBytes(Color.FromArgb(Pixels[PixelIndex + 3], Pixels[PixelIndex + 2], Pixels[PixelIndex + 1], Pixels[PixelIndex]).ToRGB5A3());
                            }
                            EncodedBlock[Offset++] = Value[1];
                            EncodedBlock[Offset++] = Value[0];
                        }
                    }
                    #endregion
                    break;
                case GXImageFormat.RGBA32:
                    #region Encode RGBA32
                    for (int i = 0; i < 16; i++)
                    {
                        int x = BlockX + (i % CurrentBlockWidth),
                            y = BlockY + ((int)Math.Floor((decimal)i / CurrentBlockWidth));

                        PixelIndex = ((y * Image.Width) + x) * 4;
                        if (x >= Image.Width || y > Image.Height || PixelIndex >= Pixels.Length)
                            Value = new byte[4] { 0xFF, 0xFF, 0xFF, 0xFF }; //If you've been reading this whole thing you'd know what this is for
                        else
                        {
                            Value = new byte[4] { Pixels[PixelIndex + 3], Pixels[PixelIndex + 2], Pixels[PixelIndex + 1], Pixels[PixelIndex] };
                        }
                        EncodedBlock[i * 2] = Value[0];
                        EncodedBlock[(i * 2) + 01] = Value[1];
                        EncodedBlock[(i * 2) + 32] = Value[2];
                        EncodedBlock[(i * 2) + 33] = Value[3];
                    }
                    #endregion
                    break;
                case GXImageFormat.C4:
                    #region Encode C4
                    for (int y = BlockY; y < BlockY + CurrentBlockHeight; y++)
                    {
                        for (int x = BlockX; x < BlockX + CurrentBlockWidth; x += 2)
                        {
                            int ColIndexL, ColIndexR;

                            if (x >= Image.Width || y >= Image.Height)
                                ColIndexL = 0xF; //Block Bleeds past image width
                            else
                            {
                                PixelIndex = ((y * Image.Width) + x) * 4;
                                ColIndexL = ColourIndicies[Color.FromArgb(Pixels[PixelIndex + 3], Pixels[PixelIndex + 2], Pixels[PixelIndex + 1], Pixels[PixelIndex])];
                            }

                            if ((x + 1) >= Image.Width || y >= Image.Height)
                                ColIndexR = 0xF; //Block Bleeds past image width
                            else
                            {
                                PixelIndex = ((y * Image.Width) + (x + 1)) * 4;
                                ColIndexR = ColourIndicies[Color.FromArgb(Pixels[PixelIndex + 3], Pixels[PixelIndex + 2], Pixels[PixelIndex + 1], Pixels[PixelIndex])];
                            }

                            EncodedBlock[Offset++] = (byte)(((ColIndexL & 0xF) << 4) | (ColIndexR & 0xF));
                        }
                    }
                    #endregion
                    break;
                case GXImageFormat.C8:
                    #region Encode C8
                    for (int y = BlockY; y < BlockY + CurrentBlockHeight; y++)
                    {
                        for (int x = BlockX; x < BlockX + CurrentBlockWidth; x++)
                        {
                            if (x >= Image.Width || y >= Image.Height)
                                EncodedBlock[Offset++] = 0xFF; //Block Bleeds past image width
                            else
                            {
                                PixelIndex = ((y * Image.Width) + x) * 4;
                                EncodedBlock[Offset++] = (byte)ColourIndicies[Color.FromArgb(Pixels[PixelIndex + 3], Pixels[PixelIndex + 2], Pixels[PixelIndex + 1], Pixels[PixelIndex])];
                            }
                        }
                    }
                    #endregion
                    break;
                case GXImageFormat.C14X2:
                    #region Encode C14X2
                    for (int y = BlockY; y < BlockY + CurrentBlockHeight; y++)
                    {
                        for (int x = BlockX; x < BlockX + CurrentBlockWidth; x++)
                        {
                            if (x >= Image.Width || y >= Image.Height)
                                Value = new byte[2] { 0xFF, 0x3F }; //Block Bleeds past image width
                            else
                            {
                                PixelIndex = ((y * Image.Width) + x) * 4;
                                Value = BitConverter.GetBytes(ColourIndicies[Color.FromArgb(Pixels[PixelIndex + 3], Pixels[PixelIndex + 2], Pixels[PixelIndex + 1], Pixels[PixelIndex])]);
                            }
                            EncodedBlock[Offset++] = Value[1];
                            EncodedBlock[Offset++] = Value[0];
                        }
                    }
                    #endregion
                    break;
                case GXImageFormat.CMPR:
                    #region Encode CMPR
                    for (int SubBlock = 0; SubBlock < 4; SubBlock++)
                    {
                        int subblock_x = BlockX + (SubBlock % 2) * 4, subblock_y = BlockY + (int)Math.Floor(SubBlock / 2.0) * 4;
                        List<Color> AllSubBlockColours = new List<Color>();
                        bool NeedsAlphaColor = false;
                        for (int i = 0; i < 16; i++)
                        {
                            int x = subblock_x + (i % 4), y = subblock_y + (int)Math.Floor(i / 4.0);
                            if (x >= Image.Width || y >= Image.Height)
                                continue;

                            PixelIndex = ((y * Image.Width) + x) * 4;
                            Color Col = Color.FromArgb(Pixels[PixelIndex + 3], Pixels[PixelIndex + 2], Pixels[PixelIndex + 1], Pixels[PixelIndex]);
                            if (/*AlphaMode != JUTTransparency.SPECIAL &&*/ Col.A < 16)
                                NeedsAlphaColor = true;
                            else
                                AllSubBlockColours.Add(Col);
                        }
                        Tuple<Color, Color> KeyCols = GetBestCMPRKeyColours(AllSubBlockColours);
                        ushort RawColor1 = KeyCols.Item1.ToRGB565(), RawColor2 = KeyCols.Item2.ToRGB565();
                        if ((NeedsAlphaColor && RawColor1 > RawColor2) || (!NeedsAlphaColor && RawColor1 < RawColor2))
                        {
                            GenericEx.SwapValues(ref RawColor1, ref RawColor2);
                            GenericEx.SwapValues(ref KeyCols);
                        }
                        Color[] CMPRColours = GetInterpolatedCMPRColours(RawColor1, RawColor2);
                        CMPRColours[0] = KeyCols.Item1;
                        CMPRColours[1] = KeyCols.Item2;
                        Value = BitConverter.GetBytes(RawColor1);
                        EncodedBlock[Offset++] = Value[1];
                        EncodedBlock[Offset++] = Value[0];
                        Value = BitConverter.GetBytes(RawColor2);
                        EncodedBlock[Offset++] = Value[1];
                        EncodedBlock[Offset++] = Value[0];

                        int ColorIDs = 0;
                        for (int i = 0; i < 16; i++)
                        {
                            int x = subblock_x + (i % 4), y = subblock_y + (int)Math.Floor(i / 4.0);
                            if (x >= Image.Width || y >= Image.Height)
                                continue;

                            PixelIndex = ((y * Image.Width) + x) * 4;
                            Color Col = Color.FromArgb(Pixels[PixelIndex + 3], Pixels[PixelIndex + 2], Pixels[PixelIndex + 1], Pixels[PixelIndex]);
                            ColorIDs |= Array.IndexOf(CMPRColours, CMPRColours.Contains(Col) ? Col : GetNearestColour(Col, CMPRColours)) << ((15 - i) * 2);
                        }
                        Value = BitConverter.GetBytes(ColorIDs);
                        EncodedBlock[Offset++] = Value[3];
                        EncodedBlock[Offset++] = Value[2];
                        EncodedBlock[Offset++] = Value[1];
                        EncodedBlock[Offset++] = Value[0];
                    }
                    #endregion
                    break;
                default:
                    throw new Exception("Invalid Image Format");
            }

            return EncodedBlock;
        }

        public static byte[] EncodePalette(ushort[] RawColours, GXImageFormat Format)
        {
            if (!IsPaletteFormat(Format))
                return new byte[0];

            byte[] PaletteData = new byte[RawColours.Length * 2];
            int Offset = 0;
            for (int i = 0; i < RawColours.Length; i++)
            {
                byte[] temp = BitConverter.GetBytes(RawColours[i]);
                PaletteData[Offset++] = temp[1];
                PaletteData[Offset++] = temp[0];
            }
            return PaletteData;
        }

        public static ushort EncodeColour(Color Col, GXPaletteFormat PaletteFormats)
        {
            if (PaletteFormats == GXPaletteFormat.IA8)
                return Col.ToIA8();
            else if (PaletteFormats == GXPaletteFormat.RGB565)
                return Col.ToRGB565();
            else if (PaletteFormats == GXPaletteFormat.RGB5A3)
                return Col.ToRGB5A3();
            else
                throw new Exception("Invalid Palette Format");
        }

        public static Color[] CreateLimitedPalette(List<Bitmap> Images, int MaxColours, bool Alpha = true)
        {
            List<byte[]> ImageData = new List<byte[]>();
            for (int i = 0; i < Images.Count; i++)
                ImageData.Add(Images[i].ToByteArray());

            int depth;
            if (MaxColours == 16)
                depth = 4;
            else if (MaxColours == 256)
                depth = 8;
            else if (MaxColours == 16384)
                depth = 14;
            else
                throw new Exception($"Unsupported maximum number of colors to generate a palette for: {MaxColours}");

            List<Color> all_pixel_colors = new List<Color>();
            bool already_have_zero_alpha_color = false;
            for (int i = 0; i < Images.Count; i++)
                for (int y = 0; y < Images[i].Height; y++)
                {
                    for (int x = 0; x < Images[i].Width; x++)
                    {
                        int z = ((y * Images[i].Width) + x) * 4;
                        Color Col = Color.FromArgb(ImageData[i][z + 3], ImageData[i][z + 2], ImageData[i][z + 1], ImageData[i][z]);
                        if (Col.A == 0)
                        {
                            if (already_have_zero_alpha_color)
                                continue;
                            if (!Alpha)
                                Col = Color.FromArgb(0, 0, 0);
                            already_have_zero_alpha_color = true;
                        }
                        all_pixel_colors.Add(Col);
                    }
                }
            return SplitToBuckets(all_pixel_colors, depth);
        }

        public static Color[] CreateLimitedPalette(Bitmap Image, int MaxColours, bool Alpha = true)
        {
            List<byte> ImageData = new List<byte>();
            ImageData.AddRange(Image.ToByteArray());

            int depth;
            if (MaxColours == 16)
                depth = 4;
            else if (MaxColours == 256)
                depth = 8;
            else if (MaxColours == 16384)
                depth = 14;
            else
                throw new Exception($"Unsupported maximum number of colors to generate a palette for: {MaxColours}");

            List<Color> all_pixel_colors = new List<Color>();
            bool already_have_zero_alpha_color = false;
            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width; x++)
                {
                    int z = ((y * Image.Width) + x) * 4;
                    Color Col = Color.FromArgb(ImageData[z + 3], ImageData[z + 2], ImageData[z + 1], ImageData[z]);
                    if (Col.A == 0)
                    {
                        if (already_have_zero_alpha_color)
                            continue;
                        if (!Alpha)
                            Col = Color.FromArgb(0, 0, 0);
                        already_have_zero_alpha_color = true;
                    }
                    all_pixel_colors.Add(Col);
                }
            }
            return SplitToBuckets(all_pixel_colors, depth);
        }

        private static Color[] SplitToBuckets(List<Color> AllColours, int Depth)
        {
            if (Depth == 0)
                return new Color[1] { AverageColours(AllColours) };

            int RedRange = AllColours.Max(C => C.R) - AllColours.Min(C => C.R);
            int GreenRange = AllColours.Max(C => C.G) - AllColours.Min(C => C.G);
            int BlueRange = AllColours.Max(C => C.B) - AllColours.Min(C => C.B);

            int channel_index_with_highest_range = 0;
            if (GreenRange >= RedRange && GreenRange >= BlueRange)
                channel_index_with_highest_range = 1;
            else if (RedRange >= GreenRange && RedRange >= BlueRange)
                channel_index_with_highest_range = 0;
            else if (BlueRange >= RedRange && BlueRange >= GreenRange)
                channel_index_with_highest_range = 2;

            AllColours = AllColours.OrderBy(C => C.A).ToList();
            AllColours = AllColours.OrderBy(C => channel_index_with_highest_range == 1 ? C.G : (channel_index_with_highest_range == 0 ? C.R : C.B)).ToList();
            List<Color> Palette = new List<Color>();
            int median = (int)Math.Floor(AllColours.Count / 2.0);
            Palette.AddRange(SplitToBuckets(AllColours.GetRange(median, AllColours.Count - median), Depth - 1));
            Palette.AddRange(SplitToBuckets(AllColours.GetRange(0, median), Depth - 1));
            return Palette.ToArray();
        }

        public static Color AverageColours(List<Color> Colours)
        {
            for (int i = 0; i < Colours.Count; i++)
            {
                if (Colours[i].A == 0)
                {
                    // Need to ensure a fully transparent color exists in the final palette if one existed originally.
                    return Colours[i];
                }
            }

            int RedSum = 0, GreenSum = 0, BlueSum = 0, AlphaSum = 0;
            for (int i = 0; i < Colours.Count; i++)
            {
                RedSum += Colours[i].R;
                GreenSum += Colours[i].G;
                BlueSum += Colours[i].B;
                AlphaSum += Colours[i].A;
            }
            return Color.FromArgb((int)Math.Floor(AlphaSum / (double)Colours.Count), (int)Math.Floor(RedSum / (double)Colours.Count), (int)Math.Floor(GreenSum / (double)Colours.Count), (int)Math.Floor(BlueSum / (double)Colours.Count));
        }

        private static Color GetNearestColour(Color Col, Color[] Palette)
        {
            if (Palette.Contains(Col))
                return Col;

            if (Col.A < 16)
                for (int i = 0; i < Palette.Length; i++)
                    if (Palette[i].A == 0)
                        return Palette[i];

            int min_dist = 0x7FFFFFFF;
            Color best_color = Palette[0];

            for (int i = 0; i < Palette.Length; i++)
            {
                int currentdistance = GetColorDistance(Col, Palette[i]);
                if (currentdistance < min_dist)
                {
                    if (currentdistance == 0)
                        return Palette[i];

                    min_dist = currentdistance;
                    best_color = Palette[i];
                }
            }

            return best_color;
        }

        private static int GetColorDistance(Color Col1, Color Col2)
        {
            int r_diff = Col1.R - Col2.R;
            int g_diff = Col1.G - Col2.G;
            int b_diff = Col1.B - Col2.B;
            int a_diff = Col1.A - Col2.A;
            double rgb_dist_sqr = (r_diff * r_diff + g_diff * g_diff + b_diff * b_diff) / 3.0;
            return (int)(a_diff * a_diff / 2.0 + rgb_dist_sqr * Col1.A * Col2.A / (255 * 255));

            //Claimed to be faster, but when benchmarked, it only was faster after the 7th 0 in the loop. (aka being run 10000000 times)
            //The other method gives better quality but I'm leaving this here just in case things change.
            //int Dist = Math.Abs(Col1.R - Col2.R);
            //Dist += Math.Abs(Col1.G - Col2.G);
            //Dist += Math.Abs(Col1.B - Col2.B);
            //Dist += Math.Abs(Col1.A - Col2.A);
            //return Dist;
        }

        private static int GetColorDistanceNoAlpha(Color Col1, Color Col2) => Math.Abs(Col1.R - Col2.R) + Math.Abs(Col1.G - Col2.G) + Math.Abs(Col1.B - Col2.B);

        private static Tuple<Color, Color> GetBestCMPRKeyColours(List<Color> AllColours)
        {
            int MaxDistance = -1;
            Color Col1 = Color.Black, Col2 = Color.White;
            for (int i = 0; i < AllColours.Count; i++)
            {
                for (int j = i + 1; j < AllColours.Count; j++)
                {
                    int curr_dist = GetColorDistance(AllColours[i], AllColours[j]);

                    if (curr_dist > MaxDistance)
                    {
                        MaxDistance = curr_dist;
                        Col1 = Color.FromArgb(AllColours[i].R, AllColours[i].G, AllColours[i].B);
                        Col2 = Color.FromArgb(AllColours[j].R, AllColours[j].G, AllColours[j].B);
                    }
                }
            }
            if (MaxDistance == -1)
            {
                Col1 = Color.FromArgb(0, 0, 0);
                Col2 = Color.FromArgb(255, 255, 255);
            }
            else
            {
                if ((Col1.R >> 3) == (Col2.R >> 3) && (Col1.G >> 2) == (Col2.G >> 2) && (Col1.B >> 3) == (Col2.B >> 3))
                    Col2 = ((Col1.R >> 3) == 0 && (Col1.G >> 2) == 0 && (Col1.B >> 3) == 0) ? Color.FromArgb(255, 255, 255) : Color.FromArgb(0, 0, 0);
            }
            return new Tuple<Color, Color>(Col1, Col2);
        }

        private static Color[] GetInterpolatedCMPRColours(ushort RawColour1, ushort RawColour2)
        {
            Color Col1 = RGB565ToColor(RawColour1), Col2 = RGB565ToColor(RawColour2), Col3, Col4;
            if (RawColour1 > RawColour2)
            {
                Col3 = Color.FromArgb((int)Math.Floor((2 * Col1.R + 1 * Col2.R) / 3.0), (int)Math.Floor((2 * Col1.G + 1 * Col2.G) / 3.0), (int)Math.Floor((2 * Col1.B + 1 * Col2.B) / 3.0));
                Col4 = Color.FromArgb((int)Math.Floor((1 * Col1.R + 2 * Col2.R) / 3.0), (int)Math.Floor((1 * Col1.G + 2 * Col2.G) / 3.0), (int)Math.Floor((1 * Col1.B + 2 * Col2.B) / 3.0));
            }
            else
            {
                Col3 = Color.FromArgb((int)Math.Floor(Col1.R / 2.0) + (int)Math.Floor(Col2.R / 2.0), (int)Math.Floor(Col1.G / 2.0) + (int)Math.Floor(Col2.G / 2.0), (int)Math.Floor(Col1.B / 2.0) + (int)Math.Floor(Col2.B / 2.0));
                Col4 = Color.FromArgb(0, 0, 0, 0);
            }
            return new Color[4] { Col1, Col2, Col3, Col4 };
        }

        #region Colour Converters


        public static byte ToI4(this Color Col) => (byte)((((int)Math.Round(((Col.R * 30) + (Col.G * 59) + (Col.B * 11)) / 100.0)) >> 4) & 0xF);

        public static byte ToI8(this Color Col) => (byte)((int)Math.Round(((Col.R * 30) + (Col.G * 59) + (Col.B * 11)) / 100.0) & 0xFF);

        public static byte ToIA4(this Color Col)
        {
            int Value = (int)Math.Round(((Col.R * 30) + (Col.G * 59) + (Col.B * 11)) / 100.0);
            int Result = 0x00;
            Result |= ((Value >> 4) & 0xF);
            Result |= ((Col.A << 4) & 0xF0);
            return (byte)Result;
        }

        public static ushort ToIA8(this Color Col)
        {
            int Value = (int)Math.Round(((Col.R * 30) + (Col.G * 59) + (Col.B * 11)) / 100.0);
            int Result = 0x0000;
            Result |= Value & 0x00FF;
            Result |= (Col.A << 8) & 0xFF00;
            return (ushort)Result;
        }

        public static ushort ToRGB565(this Color Col)
        {
            int Result = 0x0000;
            Result |= ((Col.R >> 3) & 0x1F) << 11;
            Result |= ((Col.G >> 2) & 0x3F) << 5;
            Result |= (Col.B >> 3) & 0x1F;
            return (ushort)Result;
        }

        public static ushort ToRGB5A3(this Color Col)
        {
            int Result;
            if (Col.A != 255)
            {
                Result = 0x0000;
                Result |= (((Col.A >> 5) & 0x7) << 12);
                Result |= (((Col.R >> 4) & 0xF) << 8);
                Result |= (((Col.G >> 4) & 0xF) << 4);
                Result |= (((Col.B >> 4) & 0xF) << 0);
            }
            else
            {
                Result = 0x8000;
                Result |= (((Col.R >> 3) & 0x1F) << 10);
                Result |= (((Col.G >> 3) & 0x1F) << 5);
                Result |= (((Col.B >> 3) & 0x1F) << 0);
            }
            return (ushort)Result;
        }
        #endregion

        public static bool CompareBitmap(Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1 == null || bmp2 == null)
                return false;
            if (object.Equals(bmp1, bmp2))
                return true;
            if (!bmp1.Size.Equals(bmp2.Size) || !bmp1.PixelFormat.Equals(bmp2.PixelFormat))
                return false;

            int bytes = bmp1.Width * bmp1.Height * (Image.GetPixelFormatSize(bmp1.PixelFormat) / 8);

            bool result = true;
            byte[] b1bytes = new byte[bytes];
            byte[] b2bytes = new byte[bytes];

            BitmapData bitmapData1 = bmp1.LockBits(new Rectangle(0, 0, bmp1.Width, bmp1.Height), ImageLockMode.ReadOnly, bmp1.PixelFormat);
            BitmapData bitmapData2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height), ImageLockMode.ReadOnly, bmp2.PixelFormat);

            Marshal.Copy(bitmapData1.Scan0, b1bytes, 0, bytes);
            Marshal.Copy(bitmapData2.Scan0, b2bytes, 0, bytes);

            for (int n = 0; n <= bytes - 1; n++)
            {
                if (b1bytes[n] != b2bytes[n])
                {
                    result = false;
                    break;
                }
            }

            bmp1.UnlockBits(bitmapData1);
            bmp2.UnlockBits(bitmapData2);

            return result;
        }

    }
}
