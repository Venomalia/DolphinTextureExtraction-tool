using AuroraLib.Common;
using AuroraLib.Common.Struct;
using AuroraLib.Texture;
using OpenTK.Graphics.OpenGL;

//Heavily based on the SuperBMD Library.
namespace Hack.io
{
    public partial class BMD : IFileAccess, IHasIdentifier
    {

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier64 _identifier = new("J3D2bmd3");

        public virtual bool CanRead => true;

        public virtual bool CanWrite => true;

        public virtual bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier);


        public string FileName { get; set; }
        public INF1 Scenegraph { get; protected set; }
        public VTX1 VertexData { get; protected set; }
        public EVP1 SkinningEnvelopes { get; protected set; }
        public DRW1 PartialWeightData { get; protected set; }
        public JNT1 Joints { get; protected set; }
        public SHP1 Shapes { get; protected set; }
        public MAT3 Materials { get; protected set; }
        public TEX1 Textures { get; protected set; }

        public static readonly string Padding = "Hack.io © Super Hackio Incorporated 2018-2021";

        public BMD() { }
        public BMD(string Filename)
        {
            FileStream FS = new FileStream(Filename, FileMode.Open);
            Read(FS);
            FS.Close();
            FileName = Filename;
        }
        public BMD(Stream BMD) => Read(BMD);

        public virtual void Save(string Filename)
        {
            FileStream FS = new FileStream(Filename, FileMode.Create);
            Write(FS);
            FS.Close();
            FileName = Filename;
        }
        public virtual void Save(Stream BMD) => Write(BMD);

        protected virtual void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);

            stream.Position += 0x08 + 16;
            Scenegraph = new INF1(stream, out int VertexCount);
            VertexData = new VTX1(stream, VertexCount);
            SkinningEnvelopes = new EVP1(stream);
            PartialWeightData = new DRW1(stream);
            Joints = new JNT1(stream);
            Shapes = new SHP1(stream);
            SkinningEnvelopes.SetInverseBindMatrices(Joints.FlatSkeleton);
            Shapes.SetVertexWeights(SkinningEnvelopes, PartialWeightData);
            Joints.InitBoneFamilies(Scenegraph);
            Joints.InitBoneMatricies(Scenegraph);
            Materials = new MAT3(stream);
            if (stream.ReadInt32(Endian.Big) == 0x4D444C33)
            {
                int mdl3Size = stream.ReadInt32(Endian.Big);
                stream.Position += mdl3Size - 0x08;
            }
            else
                stream.Position -= 0x04;
            Textures = new TEX1(stream);
            Materials.SetTextureNames(Textures);
            //VertexData.StipUnused(Shapes);
        }

        protected virtual void Write(Stream BMD)
        {
            BMD.Match(_identifier);
            bool IsBDL = false;
            BMD.Write(new byte[8] { 0xDD, 0xDD, 0xDD, 0xDD, 0x00, 0x00, 0x00, (byte)(IsBDL ? 0x09 : 0x08) }, 0, 8);
            BMD.Write(new byte[16], 0, 16);

            Scenegraph.Write(BMD, Shapes, VertexData);
            VertexData.Write(BMD);
            SkinningEnvelopes.Write(BMD);
            PartialWeightData.Write(BMD);
            Joints.Write(BMD);
            Shapes.Write(BMD);
            Textures.UpdateTextures(Materials);
            Materials.Write(BMD);
            Textures.Write(BMD);

            BMD.Position = 0x08;
            BMD.WriteBigEndian(BitConverter.GetBytes((int)BMD.Length), 4);
        }

        public enum GXVertexAttribute
        {
            PositionMatrixIdx = 0,
            Tex0Mtx = 1,
            Tex1Mtx = 2,
            Tex2Mtx = 3,
            Tex3Mtx = 4,
            Tex4Mtx = 5,
            Tex5Mtx = 6,
            Tex6Mtx = 7,
            Tex7Mtx = 8,
            Position = 9,
            Normal = 10,
            Color0 = 11,
            Color1 = 12,
            Tex0 = 13,
            Tex1 = 14,
            Tex2 = 15,
            Tex3 = 16,
            Tex4 = 17,
            Tex5 = 18,
            Tex6 = 19,
            Tex7 = 20,
            PositionMatrixArray = 21,
            NormalMatrixArray = 22,
            TextureMatrixArray = 23,
            LitMatrixArra = 24,
            NormalBinormalTangent = 25,
            MaxAttr = 26,
            Null = 255
        }
        public enum GXDataType
        {
            Unsigned8, RGB565 = 0x0,
            Signed8, RGB8 = 0x1,
            Unsigned16, RGBX8 = 0x2,
            Signed16, RGBA4 = 0x3,
            Float32, RGBA6 = 0x4,
            RGBA8 = 0x5
        }
        public enum GXComponentCount
        {
            Position_XY = 0,
            Position_XYZ,

            Normal_XYZ = 0,
            Normal_NBT,
            Normal_NBT3,

            Color_RGB = 0,
            Color_RGBA,

            TexCoord_S = 0,
            TexCoord_ST
        }
        public enum Vtx1OffsetIndex
        {
            PositionData,
            NormalData,
            NBTData,
            Color0Data,
            Color1Data,
            TexCoord0Data,
            TexCoord1Data,
            TexCoord2Data,
            TexCoord3Data,
            TexCoord4Data,
            TexCoord5Data,
            TexCoord6Data,
            TexCoord7Data,
        }
        /// <summary>
        /// Determines how the position and normal matrices are calculated for a shape
        /// </summary>
        public enum DisplayFlags
        {
            /// <summary>
            /// Use a Single Matrix
            /// </summary>
            SingleMatrix,
            /// <summary>
            /// Billboard along all axis
            /// </summary>
            Billboard,
            /// <summary>
            /// Billboard Only along the Y axis
            /// </summary>
            BillboardY,
            /// <summary>
            /// Use Multiple Matrixies (Skinned models)
            /// </summary>
            MultiMatrix
        }
        public enum VertexInputType
        {
            None,
            Direct,
            Index8,
            Index16
        }
        public enum GXPrimitiveType
        {
            Points = 0xB8,
            Lines = 0xA8,
            LineStrip = 0xB0,
            Triangles = 0x90,
            TriangleStrip = 0x98,
            TriangleFan = 0xA0,
            Quads = 0x80,
        }
        public static PrimitiveType FromGXToOpenTK(GXPrimitiveType Type)
            => Enum.Parse<PrimitiveType>(Type.ToString());

        public static TextureWrapMode FromGXToOpenTK(GXWrapMode Type)
            => Enum.Parse<TextureWrapMode>(Type.ToString());

        public static TextureMinFilter FromGXToOpenTK_Min(GXFilterMode Type)
            => Enum.Parse<TextureMinFilter>(Type.ToString());

        public static TextureMagFilter FromGXToOpenTK_Mag(GXFilterMode Type)
        {
            switch (Type)
            {
                case GXFilterMode.Nearest:
                    return OpenTK.Graphics.OpenGL.TextureMagFilter.Nearest;
                case GXFilterMode.Linear:
                    return OpenTK.Graphics.OpenGL.TextureMagFilter.Linear;
                case GXFilterMode.NearestMipmapNearest:
                case GXFilterMode.NearestMipmapLinear:
                case GXFilterMode.LinearMipmapNearest:
                case GXFilterMode.LinearMipmapLinear:
                    break;
            }
            throw new Exception("Bruh moment!!");
        }
        public static CullFaceMode? FromGXToOpenTK(MAT3.CullMode Type)
        {
            switch (Type)
            {
                case MAT3.CullMode.None:
                    return null;
                case MAT3.CullMode.Front:
                    return CullFaceMode.Back;
                case MAT3.CullMode.Back:
                    return CullFaceMode.Front;
                case MAT3.CullMode.All:
                    return CullFaceMode.FrontAndBack;
            }
            throw new Exception("Bruh moment!!");
        }
        public static BlendingFactor FromGXToOpenTK(MAT3.Material.BlendMode.BlendModeControl Factor)
        {
            switch (Factor)
            {
                case MAT3.Material.BlendMode.BlendModeControl.Zero:
                    return BlendingFactor.Zero;
                case MAT3.Material.BlendMode.BlendModeControl.One:
                    return BlendingFactor.One;
                case MAT3.Material.BlendMode.BlendModeControl.SrcColor:
                    return BlendingFactor.SrcColor;
                case MAT3.Material.BlendMode.BlendModeControl.InverseSrcColor:
                    return BlendingFactor.OneMinusSrcColor;
                case MAT3.Material.BlendMode.BlendModeControl.SrcAlpha:
                    return BlendingFactor.SrcAlpha;
                case MAT3.Material.BlendMode.BlendModeControl.InverseSrcAlpha:
                    return BlendingFactor.OneMinusSrcAlpha;
                case MAT3.Material.BlendMode.BlendModeControl.DstAlpha:
                    return BlendingFactor.DstAlpha;
                case MAT3.Material.BlendMode.BlendModeControl.InverseDstAlpha:
                    return BlendingFactor.OneMinusDstAlpha;
                default:
                    Events.NotificationEvent?.Invoke(NotificationType.Warning, $"Unsupported BlendModeControl: \"{Factor}\" in FromGXToOpenTK!");
                    return OpenTK.Graphics.OpenGL.BlendingFactor.SrcAlpha;

            }
        }
        public static PixelInternalFormat FromGXToOpenTK_InternalFormat(GXImageFormat imageformat)
        {
            switch (imageformat)
            {
                case GXImageFormat.I4:
                case GXImageFormat.I8:
                    return PixelInternalFormat.Intensity;
                case GXImageFormat.IA4:
                case GXImageFormat.IA8:
                    return PixelInternalFormat.Luminance8Alpha8;
                default:
                    return PixelInternalFormat.Four;
            }
        }
        public static PixelFormat FromGXToOpenTK_PixelFormat(GXImageFormat imageformat)
        {
            switch (imageformat)
            {
                case GXImageFormat.I4:
                case GXImageFormat.I8:
                    return PixelFormat.Luminance;
                case GXImageFormat.IA4:
                case GXImageFormat.IA8:
                    return PixelFormat.LuminanceAlpha;
                default:
                    return PixelFormat.Bgra;
            }
        }
    }
}
