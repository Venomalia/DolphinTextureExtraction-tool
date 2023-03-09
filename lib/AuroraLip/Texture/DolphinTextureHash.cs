using System.Text.RegularExpressions;

namespace AuroraLip.Texture
{
    /// <summary>
    /// Gives specific information about the Dolphin textures hash.
    /// </summary>
    public class DolphinTextureHash
    {
        #region Properties

        /// <summary>
        /// The FullHash of the texture, as defined by Dolphin.
        /// </summary>
        public string FullHash
        {
            get => BuildDolphinHash();
            set => ParseDolphinHash(value);
        }

        /// <summary>
        /// Indicates whether the hash corresponds to the Dolphin texture hash format.
        /// </summary>
        public bool IsValid { get; private set; } = false;

        /// <summary>
        /// specifies the width of the original texture, not the actual width of the image file.
        /// </summary>
        public int ImageWidth
        {
            get => imageWidth;
            private set
            {
                if (value > 2048)
                    IsValid = false;
                imageWidth = value;
            }
        }
        private int imageWidth;

        /// <summary>
        /// specifies the height of the original texture, not the actual height of the image file.
        /// </summary>
        public int ImageHeight
        {
            get => imageHeight;
            private set
            {
                if (value > 2048)
                    IsValid = false;
                imageHeight = value;
            }
        }
        private int imageHeight;

        /// <summary>
        /// Indicates whether the texture has Mipmap.
        /// </summary>
        public bool HasMips { get; private set; }

        /// <summary>
        /// Indicates whether the texture has Arbitrary Mipmaps.
        /// </summary>
        public bool HasArb { get; private set; }

        /// <summary>
        /// indicates the level of the mipmap.
        /// 0 = Main file
        /// </summary>
        public int Mips { get; private set; }

        /// <summary>
        /// 64-bit xxHash of the base texture as a 16 character hex.
        /// </summary>
        public string Hash { get; private set; }

        /// <summary>
        /// 64-bit xxHash of the Tlut as a 16 character hex or '$' as placeholder.
        /// </summary>
        public string TlutHash { get; private set; }

        /// <summary>
        /// specifies the used image format.
        /// </summary>
        public GXImageFormat Format { get; private set; }

        #endregion

        #region Constructor

        public DolphinTextureHash() { }

        public DolphinTextureHash(string DolphinHash) { FullHash = DolphinHash; }

        public DolphinTextureHash(int Width, int Height, bool Has_Mipmaps, GXImageFormat format, ulong hash, ulong Tlut = 0, int Mipmap = 0, bool HasArbMips = false) : this(Width, Height, Has_Mipmaps, format, hash.ToString("x").PadLeft(16, '0'), Tlut == 0 ? "$" : Tlut.ToString("x").PadLeft(16, '0'), Mipmap, HasArbMips) { }

        public DolphinTextureHash(int Width, int Height, bool Has_Mipmaps, GXImageFormat format, string hash, string Tlut = "$", int Mipmap = 0, bool HasArbMips = false)
        {
            IsValid = hash.Length == 16 && (Tlut.Length == 16 || Tlut == "$");
            ImageWidth = Width;
            ImageHeight = Height;
            HasMips = Has_Mipmaps;
            HasArb = HasArbMips;
            Mips = Mipmap;
            Hash = hash;
            TlutHash = Tlut;
            Format = format;
        }

        #endregion

        #region Hash functions
        public override string ToString()
            => FullHash;

        private static readonly Regex HashFormat = new Regex(@"tex1_(?'X'\d+)x(?'Y'\d+)_(?'M'm_)?(?'H'(?:_?[0-9a-fA-F]{16})+(?:_[$])?)_(?'F'\d+)(?:(?'A'_arb)?(?:_mip(?'Ms'\d+))?)");

        private string BuildDolphinHash()
        {
            return "tex1_" + ImageWidth + 'x' + ImageWidth + '_'
                //Has mipmaps
                + (HasMips ? "m_" : string.Empty)
                // Hash
                + Hash + '_'
                // Tlut Hash
                + (Format.IsPaletteFormat()
                    ? TlutHash : string.Empty)
                // Format
                + (int)Format
                //Is Arbitrary Mipmap
                + (HasArb ? "_arb" : string.Empty)
                // mipmaps
                + (Mips != 0
                    ? "_mip" + Mips : string.Empty);
        }

        /// <summary>
        /// recognizes the data from the hash
        /// </summary>
        private void ParseDolphinHash(string DolphinHash)
        {
            Match match = HashFormat.Match(DolphinHash);
            IsValid = match.Success;
            if (IsValid)
            {
                ImageWidth = Int32.Parse(match.Groups["X"].Value);
                ImageHeight = Int32.Parse(match.Groups["Y"].Value);
                HasMips = match.Groups["M"].Success;
                HasArb = match.Groups["A"].Success;
                if (HasMips && match.Groups["Ms"].Success)
                {
                    Mips = Int32.Parse(match.Groups["Ms"].Value);
                }
                else
                {
                    Mips = 0;
                }
                Format = (GXImageFormat)Int32.Parse(match.Groups["F"].Value);

                if (match.Groups["H"].Value.Contains('_'))
                {
                    string[] HashValue = match.Groups["H"].Value.Split('_');
                    Hash = HashValue[0];
                    TlutHash = HashValue[1];
                }
                else
                {
                    Hash = match.Groups["H"].Value;
                }

                if (Format.IsPaletteFormat() != match.Groups["H"].Value.Contains('_'))
                    IsValid = false;
            }
            else
            {
                ImageWidth = ImageHeight = Mips = 0;
                HasMips = HasArb = false;
            }
        }

        #endregion

    }
}
