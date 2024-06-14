using AuroraLib.Core.Text;
using AuroraLib.Texture.Interfaces;
using System.Text.RegularExpressions;

namespace AuroraLib.Texture
{
    public readonly struct SplitTextureHashInfo : IImageInfo
    {
        /// <inheritdoc/>
        public int Width { get; }

        /// <inheritdoc/>
        public int Height { get;  }

        /// <summary>
        /// The hash of the texture.
        /// </summary>
        public ulong Hash { get; }

        /// <summary>
        /// The hash of the texture look-up table (TLUT), also known as texture palette.
        /// </summary>
        public ulong TlutHash { get; }

        public ulong TlutHash2 { get; }

        /// <summary>
        /// The mip-map level of the texture.
        /// </summary>
        public int Mipmap { get; }

        /// <summary>
        /// A value indicating whether the texture uses an arbitrary mip-map level.
        /// </summary>
        public bool IsArbitraryMipmap { get; }

        /// <summary>
        /// A value indicating whether the texture has mip-maps.
        /// </summary>
        public bool HasMips { get; }

        /// <summary>
        /// Format of the image data.
        /// </summary>
        public GXImageFormat Format { get; }

        public ChannelSplitType SplitType { get; }

        public SplitTextureHashInfo(int width, int height, GXImageFormat format, ulong hash, ulong tlutHash, ulong tlutHash2, int mipmap = 0, bool hasMips = false, bool isArbitraryMipmap = false, ChannelSplitType type = ChannelSplitType.RGBA)
        {
            Width = width;
            Height = height;
            Hash = hash;
            TlutHash = tlutHash;
            TlutHash2 = tlutHash2;
            Mipmap = mipmap;
            IsArbitraryMipmap = isArbitraryMipmap;
            HasMips = hasMips;
            Format = format;
            SplitType = type;
        }

        public SplitTextureHashInfo(DolphinTextureHashInfo dolphinHash, DolphinTextureHashInfo dolphinHash2, ChannelSplitType type = ChannelSplitType.RGBA)
        {
            Width = dolphinHash.Width;
            Height = dolphinHash.Height;
            Hash = dolphinHash.Hash;
            TlutHash = dolphinHash.TlutHash;
            TlutHash2 = dolphinHash2.TlutHash;
            Mipmap = dolphinHash.Mipmap;
            IsArbitraryMipmap = dolphinHash.IsArbitraryMipmap;
            HasMips = dolphinHash.HasMips;
            Format = dolphinHash.Format;
            SplitType = type;
        }

        public (DolphinTextureHashInfo, DolphinTextureHashInfo) ToDolphinTextureHashInfo()
        {
            DolphinTextureHashInfo a = new(Width, Height, Hash, Format, TlutHash, Mipmap, HasMips, IsArbitraryMipmap);
            DolphinTextureHashInfo b = new(Width, Height, Hash, Format, TlutHash2, Mipmap, HasMips, IsArbitraryMipmap);
            return (a, b);
        }

        public override string ToString()
            => Build();

        /// <summary>
        /// Builds a Split texture hash.
        /// </summary>
        /// <param name="mipLevel"></param>
        /// <returns>The Dolphin texture hash for this <see cref="DolphinTextureHashInfo"/>.</returns>
        public string Build(int mipLevel = 0) => Build(Width, Height, Hash, Format, TlutHash, TlutHash2, mipLevel, HasMips, IsArbitraryMipmap, SplitType);

        /// <summary>
        /// Builds a Dolphin split texture hash.
        /// </summary>
        /// <param name="ImageWidth">The width of the texture image.</param>
        /// <param name="ImageHeight">The height of the texture image.</param>
        /// <param name="Hash">The hash of the texture data.</param>
        /// <param name="Format">The format of the texture data.</param>
        /// <param name="TlutHash">The hash of the texture's palette.</param>
        /// <param name="TlutHash2">The hash of the texture's palette.</param>
        /// <param name="mipmap">The optional mipmap count for the texture.</param>
        /// <param name="hasMips">Whether the texture has mipmaps.</param>
        /// <param name="IsArbitraryMipmap">Whether the texture's mipmaps have arbitrary sizes.</param>
        /// <returns>The Dolphin texture hash for the provided texture parameters.</returns>
        public static string Build(int ImageWidth, int ImageHeight, ulong Hash, GXImageFormat Format, ulong TlutHash, ulong TlutHash2, int mipmap = 0, bool hasMips = false, bool IsArbitraryMipmap = false, ChannelSplitType type = ChannelSplitType.RGBA)
        {
            ValueStringBuilder builder = new(stackalloc char[80]);
            builder.Append(type.ToString());
            builder.Append('_');
            builder.Append(ImageWidth.ToString());
            builder.Append('x');
            builder.Append(ImageHeight.ToString());
            builder.Append('_');
            //Has mipmaps
            if (hasMips)
                builder.Append("m_");
            // Hash
            builder.Append(Hash.ToString("x16"));
            builder.Append('_');
            // Tlut Hash
            if (Format.IsPaletteFormat())
            {
                builder.Append(TlutHash.ToString("x16"));
                builder.Append('_');
                builder.Append(TlutHash2.ToString("x16"));
                builder.Append('_');
            }
            // Format
            builder.Append(((int)Format).ToString());
            // is ArbitraryMipmap
            if (IsArbitraryMipmap)
            {
                builder.Append("_arb");
            }
            // Mipmap
            if (mipmap != 0)
            {
                builder.Append("_mip");
                builder.Append(mipmap.ToString());
            }

            string result = builder.AsSpan().ToString();
            builder.Dispose();
            return result;
        }

        public static bool TryParse(string dolphinHash, out SplitTextureHashInfo splitTexture)
        {
            if (TryParse(dolphinHash, out int imageWidth, out int imageHeight, out ulong hash, out GXImageFormat format, out ulong tlutHash, out ulong tlutHash2, out int mipmap, out bool hasMips, out bool isArbitraryMipmap, out ChannelSplitType type))
            {
                splitTexture = new SplitTextureHashInfo(imageWidth, imageHeight, format, hash, tlutHash, tlutHash2, mipmap, hasMips, isArbitraryMipmap, type);
                return true;
            }
            else
            {
                splitTexture = default;
                return false;
            }
        }

        public static bool TryParse(string splitHash, out int imageWidth, out int imageHeight, out ulong hash, out GXImageFormat format, out ulong tlutHash, out ulong tlutHash2, out int mipmap, out bool hasMips, out bool isArbitraryMipmap, out ChannelSplitType type)
        {
            const string pattern = @"(RGBA|BGRA)_(\d+)x(\d+)(_m)?_(\$|[0-9a-fA-F]{16})_(\$|[0-9a-fA-F]{16})(_(\$|[0-9a-fA-F]{16}))?_(\d+)(_arb)?(_mip(\d+))?";

            var match = Regex.Match(splitHash, pattern);
            if (!match.Success)
            {
                imageWidth = imageHeight = mipmap = 0;
                hash = tlutHash = tlutHash2 = 0;
                format = GXImageFormat.IA4;
                hasMips = isArbitraryMipmap = false;
                type = ChannelSplitType.RGBA;
                return false;
            }
            else
            {
                type = Enum.Parse<ChannelSplitType>(match.Groups[1].Value);
                imageWidth = int.Parse(match.Groups[2].Value);
                imageHeight = int.Parse(match.Groups[3].Value);
                hasMips = match.Groups[4].Success;
                hash = ulong.Parse(match.Groups[5].Value, System.Globalization.NumberStyles.HexNumber);
                tlutHash = match.Groups[6].Success && match.Groups[5].Value != "$" ? ulong.Parse(match.Groups[6].Value, System.Globalization.NumberStyles.HexNumber) : 0;
                tlutHash2 = match.Groups[7].Success && match.Groups[8].Value != "$" ? ulong.Parse(match.Groups[8].Value, System.Globalization.NumberStyles.HexNumber) : 0;
                format = (GXImageFormat)int.Parse(match.Groups[9].Value);
                isArbitraryMipmap = match.Groups[10].Success;
                mipmap = match.Groups[11].Success ? int.Parse(match.Groups[12].Value) : 0;
                return true;
            }
        }

        public enum ChannelSplitType : uint
        {
            RGBA,
            BGRA,
        }

    }
}
