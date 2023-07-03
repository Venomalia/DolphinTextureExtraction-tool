using AuroraLib.Common;
using AuroraLib.Common.Struct;

namespace AuroraLib.Texture.Formats
{
    public class DDS : JUTTexture, IHasIdentifier, IFileAccess
    {
        public virtual bool CanRead => true;

        public virtual bool CanWrite => false;

        public virtual IIdentifier Identifier => Magic;

        public static readonly Identifier32 Magic = new("DDS ");

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 124 && stream.Match(Magic) && stream.ReadUInt32() == 124;

        protected override void Read(Stream stream)
        {
            Header header = stream.Read<Header>();
            TexEntry tex;
            switch (header.Format.Flag)
            {
                case PixelFormat.Flags.FourCC:
                    AImageFormats format = header.Format.FourCC switch
                    {
                        PixelFormat.FourCCType.DXT1 => AImageFormats.DXT1,
                    };
                    tex = new(stream, format, (int)header.Width, (int)header.Height, (int)header.MipMapCount);
                    break;
                case PixelFormat.Flags.RGBA:
                    if (header.Format.RGBBitCount != 32)
                    {
                        throw new NotSupportedException();
                    }
                    tex = new(stream, (int)header.Width, (int)header.Height, (int)header.MipMapCount, header.Format.RBitMask, header.Format.GBitMask, header.Format.BBitMask, header.Format.ABitMask);
                    break;
                default:
                    throw new NotSupportedException();
            }
            tex.MaxLOD = header.MipMapCount;
            Add(tex);
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        #region Header structs

        public struct Header
        {
            public Identifier32 Magic;
            /// <summary>
            /// Size of structure.This member must be set to 124.
            /// </summary>
            public uint HeaderSize;
            /// <summary>
            /// Flags to indicate which members contain valid data.
            /// </summary>
            public Flags Flag;
            /// <summary>
            /// Surface height (in pixels).
            /// </summary>
            public uint Height;
            /// <summary>
            /// Surface width (in pixels).
            /// </summary>
            public uint Width;
            /// <summary>
            /// The pitch or number of bytes per scan line in an uncompressed texture;
            /// the total number of bytes in the top level texture for a compressed texture.
            /// </summary>
            public uint PitchOrLinearSize;
            /// <summary>
            /// Depth of a volume texture (in pixels), otherwise unused.
            /// </summary>
            public uint Depth;
            /// <summary>
            /// Number of mipmap levels, otherwise unused.
            /// </summary>
            public uint MipMapCount;

            #region Reserved1
            /// <summary>
            /// Unused.
            /// </summary>
            public unsafe Span<uint> Reserved1
            {
                get
                {
                    fixed (uint* Ptr = &_Reserved1_0)
                    {
                        return new Span<uint>(Ptr, 11);
                    }
                }
            }

            private uint _Reserved1_0;
            private uint _Reserved1_1;
            private uint _Reserved1_2;
            private uint _Reserved1_3;

            private uint _Reserved1_4;
            private uint _Reserved1_5;
            private uint _Reserved1_6;
            private uint _Reserved1_7;

            private uint _Reserved1_8;
            private uint _Reserved1_9;
            private uint _Reserved1_10;
            #endregion

            /// <summary>
            /// Pixel format
            /// </summary>
            public PixelFormat Format;
            /// <summary>
            /// Specifies the complexity of the surfaces stored.
            /// </summary>
            public Caps1Flag Caps1;
            /// <summary>
            /// Additional detail about the surfaces stored.
            /// </summary>
            public Caps2Flag Caps2;
            /// <summary>
            /// Unused.
            /// </summary>
            public uint Caps3;
            /// <summary>
            /// Unused.
            /// </summary>
            public uint Caps4;
            /// <summary>
            /// Unused.
            /// </summary>
            public uint Reserved2;

            [Flags]
            public enum Flags : uint
            {
                None = 0x0,
                /// <summary>
                /// Required in every .dds file.
                /// </summary>
                Caps = 0x1,

                /// <summary>
                /// Required in every .dds file.
                /// </summary>
                Height = 0x2,

                /// <summary>
                /// Required in every .dds file.
                /// </summary>
                Width = 0x4,

                /// <summary>
                /// Required when pitch is provided for an uncompressed texture.
                /// </summary>
                Pitch = 0x8,

                /// <summary>
                /// Required in every .dds file.
                /// </summary>
                PixelFormat = 0x1000,

                /// <summary>
                /// Required in a mipmapped texture.
                /// </summary>
                MipMapCount = 0x20000,

                /// <summary>
                /// Required when pitch is provided for a compressed texture.
                /// </summary>
                LinearSize = 0x80000,

                /// <summary>
                /// Required in a depth texture.
                /// </summary>
                Depth = 0x800000
            }

            [Flags]
            public enum Caps1Flag : uint
            {
                None = 0x0,
                /// <summary>
                /// Optional; must be used on any file that contains more than one surface (a mipmap, a cubic environment map, or mipmapped volume texture).
                /// </summary>
                Complex = 0x8,

                /// <summary>
                /// Optional; should be used for a mipmap.
                /// </summary>
                MipMap = 0x400000,

                /// <summary>
                /// Required.
                /// </summary>
                Texture = 0x1000
            }

            [Flags]
            public enum Caps2Flag : uint
            {
                None = 0x0,
                /// <summary>
                /// Required for a cube map.
                /// </summary>
                Cubemap = 0x200,

                /// <summary>
                /// Required when these surfaces are stored in a cube map.
                /// </summary>
                CubemapPositiveX = 0x400,

                /// <summary>
                /// Required when these surfaces are stored in a cube map.
                /// </summary>
                CubemapNegativeX = 0x800,

                /// <summary>
                /// Required when these surfaces are stored in a cube map.
                /// </summary>
                CubemapPositiveY = 0x1000,

                /// <summary>
                /// Required when these surfaces are stored in a cube map.
                /// </summary>
                CubemapNegativeY = 0x2000,

                /// <summary>
                /// Required when these surfaces are stored in a cube map.
                /// </summary>
                CubemapPositiveZ = 0x4000,

                /// <summary>
                /// Required when these surfaces are stored in a cube map.
                /// </summary>
                CubemapNegativeZ = 0x8000,

                /// <summary>
                /// Required for a volume texture.
                /// </summary>
                Volume = 0x200000
            }
        }

        public struct PixelFormat
        {
            /// <summary>
            /// Size of structure.This member must be set to 32 (bytes).
            /// </summary>
            public uint HeaderSize;
            /// <summary>
            /// Values which indicate what type of data is in the surface.
            /// </summary>
            public Flags Flag;
            /// <summary>
            /// Four-character codes for specifying compressed or custom formats.
            /// Possible values include: DXT1, DXT2, DXT3, DXT4, or DXT5. A FourCC of DX10 indicates the prescense of the DDS_HEADER_DXT10 extended header, and the dxgiFormat member of that structure indicates the true format.
            /// When using a four-character code, Flags must include <see cref="Flags.FourCC"/>.
            /// </summary>
            public FourCCType FourCC;
            /// <summary>
            /// Number of bits in an RGB (possibly including alpha) format. Valid when <see cref="Flag"/> includes <see cref="Flags.RGB"/>, <see cref="Flags.Luminance"/>, or <see cref="Flags.YUV"/>.
            /// </summary>
            public uint RGBBitCount;
            /// <summary>
            /// Red (or luminance or Y) mask for reading color data. For instance, given the A8R8G8B8 format, the red mask would be 0x00ff0000.
            /// </summary>
            public uint RBitMask;
            /// <summary>
            /// Green (or U) mask for reading color data. For instance, given the A8R8G8B8 format, the green mask would be 0x0000ff00.
            /// </summary>
            public uint GBitMask;
            /// <summary>
            /// Blue (or V) mask for reading color data. For instance, given the A8R8G8B8 format, the blue mask would be 0x000000ff.
            /// </summary>
            public uint BBitMask;
            /// <summary>
            /// Alpha mask for reading alpha data. <see cref="Flag"/> must include <see cref="Flags.AlphaPixels"/> or <see cref="Flags.Alpha"/>. For instance, given the A8R8G8B8 format, the alpha mask would be 0xff000000.
            /// </summary>
            public uint ABitMask;

            [Flags]
            public enum Flags : uint
            {
                /// <summary>
                /// None.
                /// </summary>
                None = 0,

                /// <summary>
                /// Texture contains alpha data; dwRGBAlphaBitMask contains valid data.
                /// </summary>
                AlphaPixels = 0x1,

                /// <summary>
                /// Used in some older DDS files for alpha channel only uncompressed data (RGBBitCount contains the alpha channel bitcount; ABitMask contains valid data)
                /// </summary>
                Alpha = 0x2,

                /// <summary>
                /// Texture contains compressed RGB data; FourCC contains valid data.
                /// </summary>
                FourCC = 0x4,

                /// <summary>
                /// Texture contains uncompressed RGB data; RGBBitCount and the RGB masks (dwRBitMask, dwRBitMask, dwRBitMask) contain valid data.
                /// </summary>
                RGB = 0x40,

                /// <summary>
                /// Texture contains uncompressed RGB and alpha data; RGBBitCount and all of the masks (RBitMask, RBitMask, RBitMask, RGBAlphaBitMask) contain valid data.
                /// </summary>
                RGBA = RGB | AlphaPixels,

                /// <summary>
                /// Used in some older DDS files for YUV uncompressed data (RGBBitCount contains the YUV bit count; RBitMask contains the Y mask, GBitMask contains the U mask, BBitMask contains the V mask)
                /// </summary>
                YUV = 0x200,

                /// <summary>
                /// Used in some older DDS files for single channel color uncompressed data (RGBBitCount contains the luminance channel bit count; RBitMask contains the channel mask). Can be combined with DDPF_ALPHAPIXELS for a two channel DDS file.
                /// </summary>
                Luminance = 0x20000
            }

            public enum FourCCType : uint
            {
                None = 0x0,
                DXT1 = 827611204,
                DXT2 = 844388420,
                DXT3 = 861165636,
                DXT4 = 877942852,
                DXT5 = 894720068,
                DX10 = 808540228,

                BC4U = 1429488450,
                BC4S = 1395934018,
                BC5U = 1429553986,
                BC5S = 1395999554,
                ATI1 = 826889281,
                ATI2 = 843666497,
                RGBG = 1195525970,
                GRGB = 1111970375,
                UYVY = 1498831189,
                YUY2 = 844715353,
                MET1 = 827606349,

                R16G16B16A16_UNORM = 36,
                R16G16B16A16_SNORM = 110,
                R16_FLOAT = 111,
                R16G16_FLOAT = 112,
                R16G16B16A16_FLOAT = 113,
                R32_FLOAT = 114,
                R32G32_FLOAT = 115,
                R32G32B32A32_FLOAT = 116,
                CxV8U8 = 117,
            }
        }

        public struct DXT10Header
        {
            /// <summary>
            /// The surface pixel format
            /// </summary>
            public DXGIFormats Format;
            /// <summary>
            /// Identifies the type of resource.
            /// </summary>
            public ResourceDimensionType ResourceDimension;
            /// <summary>
            /// Identifies other, less common options for resources.
            /// </summary>
            public MiscFlags Misc;
            /// <summary>
            /// The number of elements in the array.
            /// </summary>
            public uint ArraySize;
            /// <summary>
            /// Contains additional metadata (formerly was reserved). The lower 3 bits indicate the alpha mode of the associated resource. The upper 29 bits are reserved and are typically 0.
            /// </summary>
            public Misc2Flags Misc2;

            public enum ResourceDimensionType : uint
            {
                None = 0x0,
                /// <summary>
                /// Resource is a buffer.
                /// </summary>
                BUFFER = 1,
                /// <summary>
                /// Resource is a 1D texture
                /// </summary>
                TEXTURE1D = 2,
                /// <summary>
                /// Resource is a 2D texture
                /// </summary>
                TEXTURE2D = 3,
                /// <summary>
                /// Resource is a 3D texture
                /// </summary>
                TEXTURE3D = 4
            }

            [Flags]
            public enum MiscFlags : uint
            {
                None = 0x0,
                /// <summary>
                /// Indicates a 2D texture is a cube-map texture.
                /// </summary>
                TEXTURECUBE = 0x4,
            }

            public enum Misc2Flags : uint
            {
                /// <summary>
                /// Alpha channel content is unknown. This is the value for legacy files, which typically is assumed to be 'straight' alpha.
                /// </summary>
                None = 0x0,
                /// <summary>
                /// Any alpha channel content is presumed to use straight alpha.
                /// </summary>
                STRAIGHT = 0x1,
                /// <summary>
                /// Any alpha channel content is using premultiplied alpha. The only legacy file formats that indicate this information are 'DX2' and 'DX4'.
                /// </summary>
                PREMULTIPLIED = 0x2,
                /// <summary>
                /// Any alpha channel content is all set to fully opaque.
                /// </summary>
                OPAQUE = 0x3,
                /// <summary>
                /// Any alpha channel content is being used as a 4th channel and is not intended to represent transparency (straight or premultiplied).
                /// </summary>
                CUSTOM = 0x4,
            }

            public enum DXGIFormats : uint
            {
                None = 0x0,
                R32G32B32A32_TYPELESS = 1,
                R32G32B32A32_FLOAT = 2,
                R32G32B32A32_UINT = 3,
                R32G32B32A32_SINT = 4,
                R32G32B32_TYPELESS = 5,
                R32G32B32_FLOAT = 6,
                R32G32B32_UINT = 7,
                R32G32B32_SINT = 8,
                R16G16B16A16_TYPELESS = 9,
                R16G16B16A16_FLOAT = 10,
                R16G16B16A16_UNORM = 11,
                R16G16B16A16_UINT = 12,
                R16G16B16A16_SNORM = 13,
                R16G16B16A16_SINT = 14,
                R32G32_TYPELESS = 15,
                R32G32_FLOAT = 16,
                R32G32_UINT = 17,
                R32G32_SINT = 18,
                R32G8X24_TYPELESS = 19,
                D32_FLOAT_S8X24_UINT = 20,
                R32_FLOAT_X8X24_TYPELESS = 21,
                X32_TYPELESS_G8X24_UINT = 22,
                R10G10B10A2_TYPELESS = 23,
                R10G10B10A2_UNORM = 24,
                R10G10B10A2_UINT = 25,
                R11G11B10_FLOAT = 26,
                R8G8B8A8_TYPELESS = 27,
                R8G8B8A8_UNORM = 28,
                R8G8B8A8_UNORM_SRGB = 29,
                R8G8B8A8_UINT = 30,
                R8G8B8A8_SNORM = 31,
                R8G8B8A8_SINT = 32,
                R16G16_TYPELESS = 33,
                R16G16_FLOAT = 34,
                R16G16_UNORM = 35,
                R16G16_UINT = 36,
                R16G16_SNORM = 37,
                R16G16_SINT = 38,
                R32_TYPELESS = 39,
                D32_FLOAT = 40,
                R32_FLOAT = 41,
                R32_UINT = 42,
                R32_SINT = 43,
                R24G8_TYPELESS = 44,
                D24_UNORM_S8_UINT = 45,
                R24_UNORM_X8_TYPELESS = 46,
                X24_TYPELESS_G8_UINT = 47,
                R8G8_TYPELESS = 48,
                R8G8_UNORM = 49,
                R8G8_UINT = 50,
                R8G8_SNORM = 51,
                R8G8_SINT = 52,
                R16_TYPELESS = 53,
                R16_FLOAT = 54,
                D16_UNORM = 55,
                R16_UNORM = 56,
                R16_UINT = 57,
                R16_SNORM = 58,
                R16_SINT = 59,
                R8_TYPELESS = 60,
                R8_UNORM = 61,
                R8_UINT = 62,
                R8_SNORM = 63,
                R8_SINT = 64,
                A8_UNORM = 65,
                R1_UNORM = 66,
                R9G9B9E5_SHAREDEXP = 67,
                R8G8_B8G8_UNORM = 68,
                G8R8_G8B8_UNORM = 69,
                BC1_TYPELESS = 70,
                BC1_UNORM = 71,
                BC1_UNORM_SRGB = 72,
                BC2_TYPELESS = 73,
                BC2_UNORM = 74,
                BC2_UNORM_SRGB = 75,
                BC3_TYPELESS = 76,
                BC3_UNORM = 77,
                BC3_UNORM_SRGB = 78,
                BC4_TYPELESS = 79,
                BC4_UNORM = 80,
                BC4_SNORM = 81,
                BC5_TYPELESS = 82,
                BC5_UNORM = 83,
                BC5_SNORM = 84,
                B5G6R5_UNORM = 85,
                B5G5R5A1_UNORM = 86,
                B8G8R8A8_UNORM = 87,
                B8G8R8X8_UNORM = 88,
                R10G10B10_XR_BIAS_A2_UNORM = 89,
                B8G8R8A8_TYPELESS = 90,
                B8G8R8A8_UNORM_SRGB = 91,
                B8G8R8X8_TYPELESS = 92,
                B8G8R8X8_UNORM_SRGB = 93,
                BC6H_TYPELESS = 94,
                BC6H_UF16 = 95,
                BC6H_SF16 = 96,
                BC7_TYPELESS = 97,
                BC7_UNORM = 98,
                BC7_UNORM_SRGB = 99,
                AYUV = 100,
                Y410 = 101,
                Y416 = 102,
                NV12 = 103,
                P010 = 104,
                P016 = 105,
                _420_OPAQUE = 106,
                YUY2 = 107,
                Y210 = 108,
                Y216 = 109,
                NV11 = 110,
                AI44 = 111,
                IA44 = 112,
                P8 = 113,
                A8P8 = 114,
                B4G4R4A4_UNORM = 115,
                P208 = 130,
                V208 = 131,
                V408 = 132,
                SAMPLER_FEEDBACK_MIN_MIP_OPAQUE,
                SAMPLER_FEEDBACK_MIP_REGION_USED_OPAQUE,
                FORCE_UINT = 0xffffffff
            }
        }

        #endregion
    }
}
