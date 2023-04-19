using AuroraLib.Common;
using AuroraLib.Texture;
using AuroraLip.Texture.Interfaces;
using System.Text.RegularExpressions;

namespace AuroraLip.Texture
{
    /// <summary>
    /// Represents a parsed Dolphin texture hash.
    /// </summary>
    public struct DolphinTextureHashInfo : IImage, IDolphinHash
    {
        /// <inheritdoc/>
        public int Width { get; set; }

        /// <inheritdoc/>
        public int Height { get; set; }

        /// <inheritdoc/>
        public ulong Hash { get; set; }

        /// <inheritdoc/>
        public ulong TlutHash { get; set; }

        /// <summary>
        /// The mip-map level of the texture.
        /// </summary>
        public int Mipmap { get; set; }

        /// <summary>
        /// A value indicating whether the texture uses an arbitrary mip-map level.
        /// </summary>
        public bool IsArbitraryMipmap { get; set; }

        /// <summary>
        /// A value indicating whether the texture has mip-maps.
        /// </summary>
        public bool HasMips { get; set; }

        /// <summary>
        /// Format of the image data.
        /// </summary>
        public GXImageFormat Format { get; set; }

        public DolphinTextureHashInfo(int imageWidth, int imageHeight, ulong hash, GXImageFormat format, ulong tlutHash = 0, int mipmap = 0, bool hasMips = false, bool isArbitraryMipmap = false)
        {
            Width = imageWidth;
            Height = imageHeight;
            HasMips = hasMips;
            Hash = hash;
            Format = format;
            TlutHash = tlutHash;
            Mipmap = mipmap;
            IsArbitraryMipmap = isArbitraryMipmap;
        }

        public override string ToString() => Build();

        /// <summary>
        /// Builds a Dolphin texture hash.
        /// </summary>
        /// <returns>The Dolphin texture hash for this <see cref="DolphinTextureHashInfo"/>.</returns>
        public string Build() => Build(Width, Height, Hash, Format, TlutHash, Mipmap, HasMips, IsArbitraryMipmap);

        /// <summary>
        /// Builds a Dolphin texture hash.
        /// </summary>
        /// <param name="ImageWidth">The width of the texture image.</param>
        /// <param name="ImageHeight">The height of the texture image.</param>
        /// <param name="Hash">The hash of the texture data.</param>
        /// <param name="Format">The format of the texture data.</param>
        /// <param name="TlutHash">The optional hash of the texture's palette.</param>
        /// <param name="mipmap">The optional mipmap count for the texture.</param>
        /// <param name="hasMips">Whether the texture has mipmaps.</param>
        /// <param name="IsArbitraryMipmap">Whether the texture's mipmaps have arbitrary sizes.</param>
        /// <returns>The Dolphin texture hash for the provided texture parameters.</returns>
        public static string Build(int ImageWidth, int ImageHeight, ulong Hash, GXImageFormat Format, ulong TlutHash = 0, int mipmap = 0, bool hasMips = false, bool IsArbitraryMipmap = false)
        {
            var builder = new ValueStringBuilder(stackalloc char[56]);

            builder.Append("tex1_");
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
                if (TlutHash == 0)
                    builder.Append('$');
                else
                    builder.Append(TlutHash.ToString("x16"));
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

        /// <summary>
        /// Attempts to parse a Dolphin texture hash string into a <see cref="DolphinTextureHashInfo"/> instance.
        /// </summary>
        /// <param name="dolphinHash">The Dolphin texture hash string to parse.</param>
        /// <param name="dolphinTexture">When this method returns, contains the <see cref="DolphinTextureHashInfo"/> instance parsed from the Dolphin texture hash string, if the parse operation succeeded, or null if the parse operation failed. The parse operation fails if the input is not a valid Dolphin texture hash string.</param>
        /// <returns>true if the parse operation succeeded; otherwise, false.</returns>
        public static bool TryParse(string dolphinHash, out DolphinTextureHashInfo dolphinTexture)
        {
            if (TryParse(dolphinHash, out int imageWidth, out int imageHeight, out ulong hash, out GXImageFormat format, out ulong tlutHash, out int mipmap, out bool hasMips, out bool isArbitraryMipmap))
            {
                dolphinTexture = new DolphinTextureHashInfo(imageWidth, imageHeight, hash, format, tlutHash, mipmap, hasMips, isArbitraryMipmap);
                return true;
            }
            else
            {
                dolphinTexture = default;
                return false;
            }
        }

        /// <summary>
        /// Attempts to parse a Dolphin texture hash string into its individual components.
        /// </summary>
        /// <param name="dolphinHash">The Dolphin texture hash string to parse.</param>
        /// <param name="imageWidth">When this method returns, contains the image width parsed from the Dolphin texture hash string, if the parse operation succeeded; otherwise, the default value for the type.</param>
        /// <param name="imageHeight">When this method returns, contains the image height parsed from the Dolphin texture hash string, if the parse operation succeeded; otherwise, the default value for the type.</param>
        /// <param name="hash">When this method returns, contains the hash parsed from the Dolphin texture hash string, if the parse operation succeeded; otherwise, the default value for the type.</param>
        /// <param name="format">When this method returns, contains the format parsed from the Dolphin texture hash string, if the parse operation succeeded; otherwise, the default value for the type.</param>
        /// <param name="tlutHash">When this method returns, contains the palette hash parsed from the Dolphin texture hash string, if the parse operation succeeded; otherwise, the default value for the type.</param>
        /// <param name="mipmap">When this method returns, contains the mipmap count parsed from the Dolphin texture hash string, if the parse operation succeeded; otherwise, the default value for the type.</param>
        /// <param name="hasMips">When this method returns, contains a value indicating whether the Dolphin texture hash string indicates that the texture has mipmaps.</param>
        /// <param name="isArbitraryMipmap">When this method returns, contains a value indicating whether the Dolphin texture hash string indicates that the texture's mipmaps have arbitrary sizes.</param>
        /// <returns>true if the parse operation succeeded; otherwise,
        public static bool TryParse(string dolphinHash, out int imageWidth, out int imageHeight, out ulong hash, out GXImageFormat format, out ulong tlutHash, out int mipmap, out bool hasMips, out bool isArbitraryMipmap)
        {
            const string pattern = @"tex1_(\d+)x(\d+)(_m)?_((\$|[0-9a-fA-F]{16}))(_(\$|[0-9a-fA-F]{16}))?_(\d+)(_arb)?(._mip(\d+))?";

            var match = Regex.Match(dolphinHash, pattern);
            if (!match.Success)
            {
                imageWidth = imageHeight = mipmap = 0;
                hash = tlutHash = 0;
                format = GXImageFormat.IA4;
                hasMips = isArbitraryMipmap = false;
                return false;
            }
            else
            {
                imageWidth = int.Parse(match.Groups[1].Value);
                imageHeight = int.Parse(match.Groups[2].Value);
                hasMips = match.Groups[3].Success;
                hash = ulong.Parse(match.Groups[4].Value.Replace("_", ""), System.Globalization.NumberStyles.HexNumber);
                tlutHash = match.Groups[5].Success && match.Groups[5].Value != "$" ? ulong.Parse(match.Groups[5].Value.Substring(1), System.Globalization.NumberStyles.HexNumber) : 0;
                format = (GXImageFormat)int.Parse(match.Groups[6].Value);
                isArbitraryMipmap = match.Groups[7].Success;
                mipmap = match.Groups[8].Success ? int.Parse(match.Groups[8].Value) : 0;
                return true;
            }
        }
    }
}
