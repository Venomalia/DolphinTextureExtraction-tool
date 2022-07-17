using AuroraLip.Common;
using System;
using System.IO;

//Heavily based on the SuperBMD Library.
namespace Hack.io.BMD
{
    public partial class BDL : BMD, IFileAccess, IMagicIdentify
    {
        public override string Magic => magic;

        private const string magic = "J3D2bdl4";

        public override bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(Magic);

        public MDL3 MatDisplayList { get; private set; }

        public BDL() : base()
        {
        }

        public BDL(string Filename) : base(Filename)
        {
        }

        public BDL(Stream BDL) : base(BDL)
        {
        }

        protected override void Read(Stream BDLFile)
        {
            if (!BDLFile.ReadString(8).Equals(magic))
                throw new Exception($"Invalid Identifier. Expected \"{magic}\"");

            BDLFile.Position += 0x08 + 16;
            Scenegraph = new INF1(BDLFile, out int VertexCount);
            VertexData = new VTX1(BDLFile, VertexCount);
            SkinningEnvelopes = new EVP1(BDLFile);
            PartialWeightData = new DRW1(BDLFile);
            Joints = new JNT1(BDLFile);
            Shapes = new SHP1(BDLFile);
            SkinningEnvelopes.SetInverseBindMatrices(Joints.FlatSkeleton);
            Shapes.SetVertexWeights(SkinningEnvelopes, PartialWeightData);
            Joints.InitBoneFamilies(Scenegraph);
            Joints.InitBoneMatricies(Scenegraph);
            Materials = new MAT3(BDLFile);
            MatDisplayList = new MDL3(BDLFile);
            Textures = new TEX1(BDLFile);
            Materials.SetTextureNames(Textures);
            //VertexData.StipUnused(Shapes);
        }

        public enum BPRegister : int
        {
            GEN_MODE = 0x00,

            IND_MTXA0 = 0x06,
            IND_MTXB0 = 0x07,
            IND_MTXC0 = 0x08,
            IND_MTXA1 = 0x09,
            IND_MTXB1 = 0x0A,
            IND_MTXC1 = 0x0B,
            IND_MTXA2 = 0x0C,
            IND_MTXB2 = 0x0D,
            IND_MTXC2 = 0x0E,
            IND_IMASK = 0x0F,

            IND_CMD0 = 0x10,
            IND_CMD1 = 0x11,
            IND_CMD2 = 0x12,
            IND_CMD3 = 0x13,
            IND_CMD4 = 0x14,
            IND_CMD5 = 0x15,
            IND_CMD6 = 0x16,
            IND_CMD7 = 0x17,
            IND_CMD8 = 0x18,
            IND_CMD9 = 0x19,
            IND_CMDA = 0x1A,
            IND_CMDB = 0x1B,
            IND_CMDC = 0x1C,
            IND_CMDD = 0x1D,
            IND_CMDE = 0x1E,
            IND_CMDF = 0x1F,

            SCISSOR_0 = 0x20,
            SCISSOR_1 = 0x21,

            SU_LPSIZE = 0x22,
            SU_COUNTER = 0x23,
            RAS_COUNTER = 0x24,

            RAS1_SS0 = 0x25,
            RAS1_SS1 = 0x26,
            RAS1_IREF = 0x27,

            RAS1_TREF0 = 0x28,
            RAS1_TREF1 = 0x29,
            RAS1_TREF2 = 0x2A,
            RAS1_TREF3 = 0x2B,
            RAS1_TREF4 = 0x2C,
            RAS1_TREF5 = 0x2D,
            RAS1_TREF6 = 0x2E,
            RAS1_TREF7 = 0x2F,

            SU_SSIZE0 = 0x30,
            SU_TSIZE0 = 0x31,
            SU_SSIZE1 = 0x32,
            SU_TSIZE1 = 0x33,
            SU_SSIZE2 = 0x34,
            SU_TSIZE2 = 0x35,
            SU_SSIZE3 = 0x36,
            SU_TSIZE3 = 0x37,
            SU_SSIZE4 = 0x38,
            SU_TSIZE4 = 0x39,
            SU_SSIZE5 = 0x3A,
            SU_TSIZE5 = 0x3B,
            SU_SSIZE6 = 0x3C,
            SU_TSIZE6 = 0x3D,
            SU_SSIZE7 = 0x3E,
            SU_TSIZE7 = 0x3F,

            PE_ZMODE = 0x40,
            PE_CMODE0 = 0x41, // dithering / blend mode / color_update / alpha_update / set_dither
            PE_CMODE1 = 0x42, // destination alpha
            PE_CONTROL = 0x43, // comp z location z_comp_loc(0x43000040)pixel_fmt(0x43000041)
            field_mask = 0x44,
            PE_DONE = 0x45,
            clock = 0x46,
            PE_TOKEN = 0x47, // token B (16 bit)
            PE_TOKEN_INT = 0x48, // token A (16 bit)
            EFB_SOURCE_RECT_TOP_LEFT = 0x49,
            EFB_SOURCE_RECT_WIDTH_HEIGHT = 0x4A,
            XFB_TARGET_ADDRESS = 0x4B,


            DISP_COPY_Y_SCALE = 0x4E,
            PE_COPY_CLEAR_AR = 0x4F,
            PE_COPY_CLEAR_GB = 0x50,
            PE_COPY_CLEAR_Z = 0x51,
            PE_COPY_EXECUTE = 0x52,

            SCISSOR_BOX_OFFSET = 0x59,

            TEX_LOADTLUT0 = 0x64,
            TEX_LOADTLUT1 = 0x65,

            TX_SET_MODE0_I0 = 0x80,
            TX_SET_MODE0_I1 = 0x81,
            TX_SET_MODE0_I2 = 0x82,
            TX_SET_MODE0_I3 = 0x83,

            TX_SET_MODE1_I0 = 0x84,
            TX_SET_MODE1_I1 = 0x85,
            TX_SET_MODE1_I2 = 0x86,
            TX_SET_MODE1_I3 = 0x87,

            TX_SETIMAGE0_I0 = 0x88,
            TX_SETIMAGE0_I1 = 0x89,
            TX_SETIMAGE0_I2 = 0x8A,
            TX_SETIMAGE0_I3 = 0x8B,

            TX_SETIMAGE1_I0 = 0x8C,
            TX_SETIMAGE1_I1 = 0x8D,
            TX_SETIMAGE1_I2 = 0x8E,
            TX_SETIMAGE1_I3 = 0x8F,

            TX_SETIMAGE2_I0 = 0x90,
            TX_SETIMAGE2_I1 = 0x91,
            TX_SETIMAGE2_I2 = 0x92,
            TX_SETIMAGE2_I3 = 0x93,

            TX_SETIMAGE3_I0 = 0x94,
            TX_SETIMAGE3_I1 = 0x95,
            TX_SETIMAGE3_I2 = 0x96,
            TX_SETIMAGE3_I3 = 0x97,

            TX_LOADTLUT0 = 0x98,
            TX_LOADTLUT1 = 0x99,
            TX_LOADTLUT2 = 0x9A,
            TX_LOADTLUT3 = 0x9B,


            TX_SET_MODE0_I4 = 0xA0,
            TX_SET_MODE0_I5 = 0xA1,
            TX_SET_MODE0_I6 = 0xA2,
            TX_SET_MODE0_I7 = 0xA3,

            TX_SET_MODE1_I4 = 0xA4,
            TX_SET_MODE1_I5 = 0xA5,
            TX_SET_MODE1_I6 = 0xA6,
            TX_SET_MODE1_I7 = 0xA7,

            TX_SETIMAGE0_I4 = 0xA8,
            TX_SETIMAGE0_I5 = 0xA9,
            TX_SETIMAGE0_I6 = 0xAA,
            TX_SETIMAGE0_I7 = 0xAB,

            TX_SETIMAGE1_I4 = 0xAC,
            TX_SETIMAGE1_I5 = 0xAD,
            TX_SETIMAGE1_I6 = 0xAE,
            TX_SETIMAGE1_I7 = 0xAF,

            TX_SETIMAGE2_I4 = 0xB0,
            TX_SETIMAGE2_I5 = 0xB1,
            TX_SETIMAGE2_I6 = 0xB2,
            TX_SETIMAGE2_I7 = 0xB3,

            TX_SETIMAGE3_I4 = 0xB4,
            TX_SETIMAGE3_I5 = 0xB5,
            TX_SETIMAGE3_I6 = 0xB6,
            TX_SETIMAGE3_I7 = 0xB7,

            TX_SETTLUT_I4 = 0xB8,
            TX_SETTLUT_I5 = 0xB9,
            TX_SETTLUT_I6 = 0xBA,
            TX_SETTLUT_I7 = 0xBB,


            TEV_COLOR_ENV_0 = 0xC0,
            TEV_ALPHA_ENV_0 = 0xC1,

            TEV_COLOR_ENV_1 = 0xC2,
            TEV_ALPHA_ENV_1 = 0xC3,

            TEV_COLOR_ENV_2 = 0xC4,
            TEV_ALPHA_ENV_2 = 0xC5,

            TEV_COLOR_ENV_3 = 0xC6,
            TEV_ALPHA_ENV_3 = 0xC7,

            TEV_COLOR_ENV_4 = 0xC8,
            TEV_ALPHA_ENV_4 = 0xC9,

            TEV_COLOR_ENV_5 = 0xCA,
            TEV_ALPHA_ENV_5 = 0xCB,

            TEV_COLOR_ENV_6 = 0xCC,
            TEV_ALPHA_ENV_6 = 0xCD,

            TEV_COLOR_ENV_7 = 0xCE,
            TEV_ALPHA_ENV_7 = 0xCF,

            TEV_COLOR_ENV_8 = 0xD0,
            TEV_ALPHA_ENV_8 = 0xD1,

            TEV_COLOR_ENV_9 = 0xD2,
            TEV_ALPHA_ENV_9 = 0xD3,

            TEV_COLOR_ENV_A = 0xD4,
            TEV_ALPHA_ENV_A = 0xD5,

            TEV_COLOR_ENV_B = 0xD6,
            TEV_ALPHA_ENV_B = 0xD7,

            TEV_COLOR_ENV_C = 0xD8,
            TEV_ALPHA_ENV_C = 0xD9,

            TEV_COLOR_ENV_D = 0xDA,
            TEV_ALPHA_ENV_D = 0xDB,

            TEV_COLOR_ENV_E = 0xDC,
            TEV_ALPHA_ENV_E = 0xDD,

            TEV_COLOR_ENV_F = 0xDE,
            TEV_ALPHA_ENV_F = 0xDF,

            TEV_REGISTERL_0 = 0xE0,
            TEV_REGISTERH_0 = 0xE1,

            TEV_REGISTERL_1 = 0xE2,
            TEV_REGISTERH_1 = 0xE3,

            TEV_REGISTERL_2 = 0xE4,
            TEV_REGISTERH_2 = 0xE5,

            TEV_REGISTERL_3 = 0xE6,
            TEV_REGISTERH_3 = 0xE7,

            FOG_RANGE = 0xE8,

            TEV_FOG_PARAM_0 = 0xEE,
            TEV_FOG_PARAM_1 = 0xEF,
            TEV_FOG_PARAM_2 = 0xF0,
            TEV_FOG_PARAM_3 = 0xF1,

            TEV_FOG_COLOR = 0xF2,

            TEV_ALPHAFUNC = 0xF3,
            TEV_Z_ENV_0 = 0xF4,
            TEV_Z_ENV_1 = 0xF5,

            TEV_KSEL_0 = 0xF6,
            TEV_KSEL_1 = 0xF7,
            TEV_KSEL_2 = 0xF8,
            TEV_KSEL_3 = 0xF9,
            TEV_KSEL_4 = 0xFA,
            TEV_KSEL_5 = 0xFB,
            TEV_KSEL_6 = 0xFC,
            TEV_KSEL_7 = 0xFD,

            BP_MASK = 0xFE
        }
        public enum XFRegister : int
        {
            /// <summary>
            /// Set the number of Channels
            /// </summary>
            SETNUMCHAN = 0x1009,
            /// <summary>
            /// Set Channel0's Ambient Colour
            /// </summary>
            SETCHAN0_AMBCOLOR = 0x100A,
            /// <summary>
            /// Set Channel0's Material Colour
            /// </summary>
            SETCHAN0_MATCOLOR = 0x100C,
            /// <summary>
            /// Set Channel0's Colour
            /// </summary>
            SETCHAN0_COLOR = 0x100E,
            /// <summary>
            /// Set the number of Texture Generators
            /// </summary>
            SETNUMTEXGENS = 0x103F,
            /// <summary>
            /// Set the Texture Matrix Information
            /// </summary>
            SETTEXMTXINFO = 0x1040,
            /// <summary>
            /// Set the Position Matrix Information
            /// </summary>
            SETPOSMTXINFO = 0x1050,

            SETTEXMTX0 = 0x0078,
            SETTEXMTX1 = 0x0084,
            SETTEXMTX2 = 0x0090,
            SETTEXMTX3 = 0x009C,
            SETTEXMTX4 = 0x00A8,
            SETTEXMTX5 = 0x00B4,
            SETTEXMTX6 = 0x00C0,
            SETTEXMTX7 = 0x00CC,
            SETTEXMTX8 = 0x00D8,
            SETTEXMTX9 = 0x00E4
        }
    }
}
