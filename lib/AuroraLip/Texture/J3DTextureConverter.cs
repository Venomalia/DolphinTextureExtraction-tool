using AuroraLib.Common;
using System.Drawing;
using static AuroraLib.Texture.J3DColorConverter;

namespace AuroraLib.Texture
{

    /*
    * Super Hackio Incorporated
    * "Copyright © Super Hackio Incorporated 2020-2021"
    * https://github.com/SuperHackio/Hack.io
    */

    public static partial class J3DTextureConverter
    {
        #region Decode

        public static Bitmap DecodeImage(Stream TextureFile, ReadOnlySpan<byte> PaletteData, GXImageFormat Format, GXPaletteFormat? PaletteFormat, int? PaletteCount, int ImageWidth, int ImageHeight, int Mipmap = 0)
        {
            ImageWidth = Math.Max(1, ImageWidth >> Mipmap);
            ImageHeight = Math.Max(1, ImageHeight >> Mipmap);
            return DecodeImage(TextureFile.Read(Format.GetCalculatedDataSize(ImageWidth, ImageHeight)), PaletteData, Format, PaletteFormat, PaletteCount, ImageWidth, ImageHeight);
        }

        public static Bitmap DecodeImage(ReadOnlySpan<byte> ImageData, ReadOnlySpan<byte> PaletteData, GXImageFormat Format, GXPaletteFormat? PaletteFormat, int? PaletteCount, int ImageWidth, int ImageHeight)
        {
            Color[] PaletteColours = null;

            if (Format.IsPaletteFormat())
            {
                if (PaletteData != null && PaletteData.Length > 0)
                {
                    PaletteColours = DecodePalette(PaletteData, PaletteFormat, PaletteCount);
                }
                else
                {
                    Events.NotificationEvent?.Invoke(NotificationType.Warning, $"No pallet associate with this Texture, it will be rendered in grayscale! Format:{Format} PaletteFormat:{PaletteFormat}");
                    Format -= 8;
                }
            }
            return DecodeImage(ImageData, PaletteColours, Format, ImageWidth, ImageHeight);
        }

        public static Bitmap DecodeImage(ReadOnlySpan<byte> ImageData, ReadOnlySpan<Color> PaletteColours, GXImageFormat Format, int Width, int Height)
        {

            if (Format.IsPaletteFormat() && PaletteColours.Length == 0)
            {
                Format -= 8;
                Events.NotificationEvent?.Invoke(NotificationType.Warning, $"No Colors associate with this Palettet Texture, it will be rendered in {Format} grayscale!");
            }

            var (BlockWidth, BlockHeight) = Format.GetBlockSize();
            int BlockDataSize = Format.GetBlockDataSize(), offset = 0, BlockX = 0, BlockY = 0, XInBlock = 0, YInBlock = 0;

            byte[] Pixels = new byte[Width * Height * 4];
            while (BlockY < Height)
            {
                Color[] PixelData = DecodeBlock(ImageData, Format, offset, PaletteColours);

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

        public static Color[] DecodePalette(ReadOnlySpan<byte> PaletteData, GXPaletteFormat? PaletteFormat, int? Count)
        {
            List<Color> Colours = new();
            int offset = 0;
            for (int i = 0; i < Count; i++)
            {
                ushort Raw = BitConverter.ToUInt16(PaletteData.Slice(offset, 2)).Swap();
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
                        throw new FormatException($"Invalid {nameof(GXPaletteFormat)}:{PaletteFormat}");
                }
            }

            return Colours.ToArray();
        }

        public static byte[] EncodePalette(in IEnumerable<Color> colors, GXPaletteFormat PaletteFormat)
        {
            MemoryStream bytes = new();

            foreach (Color color in colors)
            {
                switch (PaletteFormat)
                {
                    case GXPaletteFormat.IA8:
                        bytes.Write(color.ToIA8(), Endian.Big);
                        break;
                    case GXPaletteFormat.RGB565:
                        bytes.Write(color.ToRGB565(), Endian.Big);
                        break;
                    case GXPaletteFormat.RGB5A3:
                        bytes.Write(color.ToRGB5A3(), Endian.Big);
                        break;
                    default:
                        throw new FormatException($"Invalid {nameof(GXPaletteFormat)}:{PaletteFormat}");
                }
            }
            return bytes.ToArray();
        }

        public static Color[] DecodeBlock(ReadOnlySpan<byte> ImageData, GXImageFormat Format, int Offset, ReadOnlySpan<Color> Colours)
        {
            int BlockSize = Format.GetBlockDataSize();
            List<Color> Result = new();
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
                            Result.Add(IA8ToColor(BitConverter.ToUInt16(ImageData.Slice(Offset + i * 2, 2)).Swap()));
                    break;
                case GXImageFormat.RGB565:
                    for (int i = 0; i < BlockSizeHalfed; i++)
                        if (Offset + i * 2 < ImageData.Length)
                            Result.Add(RGB565ToColor(BitConverter.ToUInt16(ImageData.Slice(Offset + i * 2, 2)).Swap()));
                    break;
                case GXImageFormat.RGB5A3:
                    for (int i = 0; i < BlockSizeHalfed; i++)
                        if (Offset + i * 2 < ImageData.Length)
                            Result.Add(RGB5A3ToColor(BitConverter.ToUInt16(ImageData.Slice(Offset + i * 2, 2)).Swap()));
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
                        int ColourIndex = BitConverter.ToUInt16(ImageData.Slice(Offset + i * 2, 2)).Swap();
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

                        Color[] DXT1 = GetInterpolatedDXT1Colours(BitConverter.ToUInt16(ImageData.Slice(subblock_offset, 2)).Swap(), BitConverter.ToUInt16(ImageData.Slice(subblock_offset + 2, 2)).Swap());
                        for (int j = 0; j < 16; j++)
                            Result[subblock_x + subblock_y * 8 + ((int)Math.Floor(j / 4.0)) * 8 + (j % 4)] = DXT1[(BitConverter.ToInt32(ImageData.Slice(subblock_offset + 4, 4)).Swap() >> ((15 - j) * 2)) & 3];
                        subblock_offset += 8;
                    }

                    break;
                default:
                    throw new FormatException($"Invalid {nameof(GXImageFormat)}:{Format}");
            }
            return Result.ToArray();
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
        public static List<byte[]> GetImageAndPaletteData(out byte[] PaletteData, List<Bitmap> Images, GXImageFormat Format, GXPaletteFormat PaletteFormat)
        {
            List<byte[]> ImageData = new();
            Tuple<Dictionary<Color, int>, ushort[]> Palette = CreatePalette(Images, Format, PaletteFormat);
            PaletteData = EncodePalette(Palette.Item2, Format);
            for (int i = 0; i < Images.Count; i++)
                ImageData.Add(EncodeImage(Images[i], Format, Palette.Item1));
            return ImageData;
        }
        /// <summary>
        /// Use in the context of no mipmaps
        /// </summary>
        /// <param name="ImageData"></param>
        /// <param name="PaletteData"></param>
        /// <param name="Image"></param>
        /// <param name="Format"></param>
        /// <param name="PaletteFormat"></param>
        public static byte[] GetImageAndPaletteData(out byte[] PaletteData, Bitmap Image, GXImageFormat Format, GXPaletteFormat PaletteFormat)
        {
            Tuple<Dictionary<Color, int>, ushort[]> Palette = Format.IsPaletteFormat() ? CreatePalette(Image, Format, PaletteFormat) : new Tuple<Dictionary<Color, int>, ushort[]>(new Dictionary<Color, int>(), null);
            PaletteData = EncodePalette(Palette.Item2, Format);
            return EncodeImage(Image, Format, Palette.Item1);
        }

        public static byte[] EncodeImage(Bitmap Image, GXImageFormat Format, Dictionary<Color, int> ColourIndicies)
        {
            List<byte> ImageData = new();
            var (BlockWidth, BlockHeight) = Format.GetBlockSize();
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
            return ImageData.ToArray();
        }

        public static Tuple<Dictionary<Color, int>, ushort[]> CreatePalette(in List<Bitmap> Images, GXImageFormat Format, GXPaletteFormat PaletteFormat)
        {
            if (!(Format == GXImageFormat.C4 || Format == GXImageFormat.C8 || Format == GXImageFormat.C14X2))
                return new Tuple<Dictionary<Color, int>, ushort[]>(null, null);

            List<byte[]> ImageData = new();
            List<ushort> encoded_colors = new();
            Dictionary<Color, int> colors_to_color_indexes = new();
            List<Color> colours = new();
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

            if (encoded_colors.Count > Format.GetMaxPaletteColours())
            {
                // If the image has more colors than the selected image format can support, we automatically reduce the number of colors.
                // For C4 and C8, the colors should have already been reduced by Pillow's quantize method.
                // So the maximum number of colors can only be exceeded for C14X2.

                Color[] LimitedPalette = CreateLimitedPalette(Images, Format.GetMaxPaletteColours(), PaletteFormat != GXPaletteFormat.RGB565);
                encoded_colors = new List<ushort>();
                colors_to_color_indexes = new Dictionary<Color, int>();
                for (int i = 0; i < Images.Count; i++)
                    for (int y = 0; y < Images[i].Height; y++)
                    {
                        for (int x = 0; x < Images[i].Width; x++)
                        {
                            int z = ((y * Images[i].Width) + x) * 4;
                            Color Col = Color.FromArgb(ImageData[i][z + 3], ImageData[i][z + 2], ImageData[i][z + 1], ImageData[i][z]);
                            ushort ColEncoded = EncodeColour(Col.GetNearestColour(LimitedPalette), PaletteFormat);
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

            List<byte> ImageData = new();
            List<ushort> encoded_colors = new();
            Dictionary<Color, int> colors_to_color_indexes = new();

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

            if (encoded_colors.Count > Format.GetMaxPaletteColours())
            {
                // If the image has more colors than the selected image format can support, we automatically reduce the number of colors.
                //For C4 and C8, the colors should have already been reduced by Pillow's quantize method.
                // So the maximum number of colors can only be exceeded for C14X2.

                Color[] LimitedPalette = CreateLimitedPalette(Image, Format.GetMaxPaletteColours(), PaletteFormat != GXPaletteFormat.RGB565);
                encoded_colors = new List<ushort>();
                colors_to_color_indexes = new Dictionary<Color, int>();

                for (int y = 0; y < Image.Height; y++)
                {
                    for (int x = 0; x < Image.Width; x++)
                    {
                        int z = ((y * Image.Width) + x) * 4;
                        Color Col = Color.FromArgb(ImageData[z + 3], ImageData[z + 2], ImageData[z + 1], ImageData[z]);
                        ushort ColEncoded = EncodeColour(Col.GetNearestColour(LimitedPalette), PaletteFormat);
                        if (!encoded_colors.Contains(ColEncoded))
                            encoded_colors.Add(ColEncoded);
                        if (!colors_to_color_indexes.ContainsKey(Col))
                            colors_to_color_indexes.Add(Col, encoded_colors.IndexOf(ColEncoded));
                    }
                }
            }
            return new Tuple<Dictionary<Color, int>, ushort[]>(colors_to_color_indexes, encoded_colors.ToArray());
        }

        public static byte[] EncodeBlock(GXImageFormat Format, ReadOnlySpan<byte> Pixels, Bitmap Image, Dictionary<Color, int> ColourIndicies, int BlockX, int BlockY)
        {
            byte[] EncodedBlock = new byte[Format.GetBlockDataSize()];
            var (CurrentBlockWidth, CurrentBlockHeight) = Format.GetBlockSize();
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
                        List<Color> AllSubBlockColours = new();
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
                            ColorIDs |= Array.IndexOf(CMPRColours, CMPRColours.Contains(Col) ? Col : Col.GetNearestColour(CMPRColours)) << ((15 - i) * 2);
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
                    throw new FormatException($"Invalid {nameof(GXImageFormat)}:{Format}");
            }

            return EncodedBlock;
        }

        public static byte[] EncodePalette(ushort[] RawColours, GXImageFormat Format)
        {
            if (!Format.IsPaletteFormat())
                return Array.Empty<byte>();

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
                throw new FormatException($"Invalid {nameof(GXPaletteFormat)}:{PaletteFormats}");
        }

        public static Color[] CreateLimitedPalette(List<Bitmap> Images, int MaxColours, bool Alpha = true)
        {
            List<byte[]> ImageData = new();
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
                throw new PaletteException($"Unsupported maximum number of colors to generate a palette for: {MaxColours}");

            List<Color> all_pixel_colors = new();
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
            List<byte> ImageData = new();
            ImageData.AddRange(Image.ToByteArray());

            int depth;
            if (MaxColours == 16)
                depth = 4;
            else if (MaxColours == 256)
                depth = 8;
            else if (MaxColours == 16384)
                depth = 14;
            else
                throw new PaletteException($"Unsupported maximum number of colors to generate a palette for: {MaxColours}");

            List<Color> all_pixel_colors = new();
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

    }
}
