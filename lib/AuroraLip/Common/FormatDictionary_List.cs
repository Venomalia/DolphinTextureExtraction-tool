using AuroraLib.Archives.Formats;
using AuroraLib.Compression.Algorithms;
using AuroraLib.Compression.Formats;
using AuroraLib.Core.Text;
using AuroraLib.Texture;
using AuroraLib.Texture.Formats;

namespace AuroraLib.Common
{
    public static partial class FormatDictionary
    {
        private const string Nin_ = "Nintendo";
        private const string Retro_ = "Retro Studios";
        private const string comp_ = "compressed Archive";

        public static readonly FormatInfo[] Master =
        {
            #region Nintendo

            //Nintendo Archive
            new FormatInfo(".arc", "RARC", FormatType.Archive, "Archive", Nin_,typeof(RARC)),
            new FormatInfo(".arc", "Uª8-", FormatType.Archive, "Archive", Nin_,typeof(U8)),
            new FormatInfo(".szs", "Yaz0", FormatType.Archive, comp_, Nin_,typeof(Yaz0)),
            new FormatInfo(".szs", "Yaz1", FormatType.Archive, comp_, Nin_,typeof(Yaz1)),
            new FormatInfo(".szp", "Yay0", FormatType.Archive, comp_, Nin_,typeof(Yay0)),
            new FormatInfo(".brres","bres", FormatType.Archive, "Wii Resources Archive", Nin_,typeof(Bres)),
            new FormatInfo(".sarc","SARC", FormatType.Archive, "Archive", Nin_),
            new FormatInfo(".mod", FormatType.Archive, "Dolphin 1 Model Archive", Nin_,typeof(MOD)),
            new FormatInfo(".mdl", FormatType.Texture, "Luigi's mansion Model", Nin_,typeof(MDL_LM)),
            new FormatInfo(".bin", 0, new byte[]{2}, FormatType.Texture, "Luigi's mansion Binary Model", Nin_){ IsMatch = BIN_LM_Matcher},
            new FormatInfo(".bnfm","BNFM", FormatType.Archive, "Wiiu Model Archive", Nin_),
            new FormatInfo(".ash","ASH0", FormatType.Archive, comp_,Nin_), //https://github.com/trapexit/wiiqt/blob/master/WiiQt/ash.cpp
            new FormatInfo(".LZOn","LZOn", FormatType.Archive, comp_,Nin_),
            new FormatInfo(".mio","MIO0", FormatType.Archive, comp_,Nin_,typeof(MIO0)),
            new FormatInfo(".vff","VFF ", FormatType.Archive, "Virtual FAT filesystem",Nin_),
            new FormatInfo(".ccf",new Identifier32(4604739), FormatType.Archive, "CCF archive",Nin_),

            //Nintendo Textures
            new FormatInfo(".breft","REFT", FormatType.Texture, "Wii Effect Texture", Nin_,typeof(REFT)),
            new FormatInfo(".TPL", FormatType.Texture, "Texture Palette Library", Nin_,typeof(TPL)),
            new FormatInfo(".TPL", FormatType.Texture, "Texture Palette Library v0", Nin_,typeof(TPL_0)),
            new FormatInfo(".txe", FormatType.Texture, "Dolphin 1 Texture", Nin_,typeof(TXE)),
            new FormatInfo(".bti", FormatType.Texture, "Binary Texture Image", Nin_,typeof(BTI)),
            new FormatInfo(".ctpk","CTPK", FormatType.Texture, "3DS Video Texture Package", Nin_),

            //J3D
            new FormatInfo(".bmd","J3D2bmd2", FormatType.Archive, "J3D Binary Display Lists v2", Nin_),
            new FormatInfo(".bdl","J3D2bdl3", FormatType.Archive, "J3D Binary Display Lists v3", Nin_),
            new FormatInfo(".bdl", "J3D2bdl4", FormatType.Archive, "J3D Binary Display Lists v4", Nin_),
            new FormatInfo(".bmd", "J3D2bmd3", FormatType.Archive, "J3D Binary Model Display v3", Nin_),
            new FormatInfo(".tex1","TEX1", FormatType.Texture, "J3D Texture", Nin_),
            new FormatInfo(".bck", "J3D1bck1", FormatType.Animation, "J3D skeletal transformation animation", Nin_),
            new FormatInfo(".bck", "J3D1bck3", FormatType.Animation, "J3D skeletal transformation v3 animation", Nin_),
            new FormatInfo(".bca", "J3D1bca1", FormatType.Animation, "J3D skeletal transformation animation", Nin_),
            new FormatInfo(".btp", "J3D1btp1", FormatType.Animation, "J3D Texture pattern animation", Nin_),
            new FormatInfo(".bpk", "J3D1bpk1", FormatType.Animation, "J3D color animation", Nin_),
            new FormatInfo(".bpa", "J3D1bpa1", FormatType.Animation, "J3D color animation", Nin_),
            new FormatInfo(".bva", "J3D1bva1", FormatType.Animation, "J3D visibility animation", Nin_),
            new FormatInfo(".blk", "J3D1blk1", FormatType.Animation, "J3D cluster animation", Nin_),
            new FormatInfo(".bla", "J3D1bla1", FormatType.Animation, "J3D cluster animation", Nin_),
            new FormatInfo(".bxk", "J3D1bxk1", FormatType.Animation, "J3D vertex color animation", Nin_),
            new FormatInfo(".bxa", "J3D1bxa1", FormatType.Animation, "J3D vertex color animation", Nin_),
            new FormatInfo(".btk", "J3D1btk1", FormatType.Animation, "J3D texture animation", Nin_),
            new FormatInfo(".brk", "J3D1brk1", FormatType.Animation, "J3D TEV color animation", Nin_),
            new FormatInfo(".bmt", "J3D2bmt3", FormatType.Else, "", Nin_),
            //NW4R
            new FormatInfo(".tex0","TEX0", FormatType.Texture, "NW4R Texture", Nin_,typeof(TEX0)),
            new FormatInfo(".rtex", FormatType.Texture, "NW4R XML Texture", Nin_),
            new FormatInfo(".mdl0", "MDL0", FormatType.Model, "NW4R Model", Nin_),
            new FormatInfo(".chr0", "CHR0", FormatType.Animation, "NW4R Bone animation", Nin_),
            new FormatInfo(".srt0", "SRT0", FormatType.Animation, "NW4R Texture animation", Nin_),
            new FormatInfo(".shp0", "SHP0", FormatType.Animation, "NW4R Vertex Transform", Nin_),
            new FormatInfo(".vis0", "VIS0", FormatType.Animation, "NW4R Visibility animation", Nin_),
            new FormatInfo(".pat0", "PAT0", FormatType.Animation, "NW4R Texture Pattern animation", Nin_),
            new FormatInfo(".clr0", "CLR0", FormatType.Animation, "NW4R Color Pattern animation", Nin_),
            new FormatInfo(".scn0", "SCN0", FormatType.Parameter, "NW4R Scene Settings", Nin_),
            new FormatInfo(".plt0", "PLT0", FormatType.Parameter, "NW4R Color Palettes", Nin_),

            //Audio & Video
            new FormatInfo(".brsar","RSAR", FormatType.Audio, "Wii Sound Archive", Nin_),
            new FormatInfo(".brstm", "RSTM", FormatType.Audio, "Wii Stream", Nin_),
            new FormatInfo(".ast","STRM", FormatType.Audio, "Stream", Nin_),
            new FormatInfo(".bars","BARS", FormatType.Audio, "Audio ReSource archive", Nin_),
            new FormatInfo(".arsl","ARSL", FormatType.Audio, "Audio ReSource LIST", Nin_),
            new FormatInfo(".dsp", FormatType.Audio, "Nintendo ADPCM codec", Nin_),
            new FormatInfo(".dsp", "DSP", FormatType.Audio, "Nintendo ADPCM codec", Nin_),
            new FormatInfo(".idsp","IDSP", FormatType.Audio,"Nintendo ADPCM codec", Nin_),
            new FormatInfo(".baa", FormatType.Audio, "JAudio archive", Nin_),
            new FormatInfo(".aw", FormatType.Audio, "JAudio wave archive", Nin_),
            new FormatInfo(".bms", FormatType.Audio, "JAudio music sequence", Nin_),
            new FormatInfo(".bct", FormatType.Audio, "Wii Remote sound info", Nin_),
            new FormatInfo(".csw", FormatType.Audio, "Wii Remote sound effect", Nin_),
            new FormatInfo(".thp", new Identifier32((byte)'T',(byte)'H',(byte)'P',0), FormatType.Video,"video", Nin_),
            new FormatInfo(".bnk","IBNK",FormatType.Audio,"Instrument Bank",Nin_),
            new FormatInfo(".wsy","WSYS",FormatType.Audio,"Wave System Table",Nin_),
            new FormatInfo(".arc","BARC",FormatType.Audio,"BARC archive",Nin_),
            new FormatInfo(".COL",FormatType.Collision,"model collision",Nin_),
            new FormatInfo(".bst",FormatType.Audio,"JAudio Sound Table",Nin_),
            new FormatInfo(".aaf",FormatType.Audio,"Audio Initialization File",Nin_),
            new FormatInfo(".asn",FormatType.Audio,"Audio Name Table",Nin_),
            new FormatInfo(".cwar","CWAR",FormatType.Audio,"3ds Sound Wave Archive",Nin_),
            new FormatInfo(".bcstm","CSTM",FormatType.Audio,"3ds Sound Wave Streams",Nin_),

            //Text
            new FormatInfo(".bmc","MGCLbmc1", FormatType.Text, "message data", Nin_),
            new FormatInfo(".msbt","MsgStdBn", FormatType.Text, "LMS data", Nin_),
            new FormatInfo(".msbf","MsgFlwBn", FormatType.Text, "LMS flow data", Nin_),
            new FormatInfo(".msbp","MsgPrjBn", FormatType.Text, "LMS Prj data", Nin_),
            new FormatInfo(".bmg","MESGbmg1", FormatType.Text, "Binary message container", Nin_),

            //Banner
            new FormatInfo(".bns", FormatType.Else, "Banner", Nin_),
            new FormatInfo(".bnr",new Identifier32(66,78,82,49), FormatType.Else, "Banner", Nin_),
            new FormatInfo(".bnr",new Identifier32(66,78,82,50), FormatType.Else, "Banner", Nin_),
            new FormatInfo(".bnr", 64,new byte[]{73,77,69,84}, FormatType.Else, "Banner", Nin_),
            new FormatInfo(".pac", FormatType.Else, "Banner", Nin_),
            new FormatInfo(".ico","SMDH", FormatType.Else, "3DS Video Icons", Nin_),
            new FormatInfo(".bnr","CBMD", FormatType.Else, "3DS Video Banner", Nin_),

            //Nintendo Else
            new FormatInfo(".ymp",FormatType.Collision,"Sunshine pollution heightmaps",Nin_),
            new FormatInfo(".blo", "SCRNblo1", FormatType.Layout, "UI Layout", Nin_),
            new FormatInfo(".blo", "SCRNblo2", FormatType.Layout, "UI V2 Layout", Nin_),
            new FormatInfo(".brlan", "RLAN", FormatType.Animation, "Wii layout Animation", Nin_),
            new FormatInfo(".brlyt", "RLYT", FormatType.Layout, "Wii structure Layout", Nin_),
            new FormatInfo(".brlmc", "RLMC", FormatType.Unknown, "Wii MC layout", Nin_), //not exactly known
            new FormatInfo(".brlpa", "RLPA", FormatType.Unknown, "Wii PA layout", Nin_), //not exactly known
            new FormatInfo(".pblm", "PBLM", FormatType.Unknown, "Wii Sport", Nin_), //not exactly known
            new FormatInfo(".brfnt", "RFNT", FormatType.Font, "Wii Font", Nin_),
            new FormatInfo(".pkb",  "RFNA", FormatType.Font, "Wii Font", Nin_),
            new FormatInfo(".bflyt", "FLYT", FormatType.Layout, "Binary caFe LaYouT", Nin_),
            new FormatInfo(".bclyt", "CLYT", FormatType.Layout, "Binary caFe LaYouT", Nin_),
            new FormatInfo(".bflim", "FLIM", FormatType.Texture, "Binary caFe Layout IMage", Nin_),
            new FormatInfo(".fbis", "FVIS", FormatType.Animation, "caFe VISibility animation", Nin_),
            new FormatInfo(".fska", "FSKA", FormatType.Animation, "caFe SKeletal Animation", Nin_),
            new FormatInfo(".bfres","FRES", FormatType.Archive, "Binary CaFe RESource", Nin_),
            new FormatInfo(".bflan","FLAN", FormatType.Animation, "Binary caFe Layout ANimation", Nin_),
            new FormatInfo(".fshu","FSHU", FormatType.Animation, "caFe SHader parameter animation Uber", Nin_),
            new FormatInfo(".fmdl","FMDL", FormatType.Animation, "caFe MoDeL", Nin_),
            new FormatInfo(".zar",new Identifier32((byte)'Z',(byte)'A',(byte)'R',0), FormatType.Archive, "Ocarina 3D Archive", Nin_),
            new FormatInfo(".gar",new Identifier32((byte)'G',(byte)'A',(byte)'R',0), FormatType.Archive, "Majora 3D Archive", Nin_),
            new FormatInfo(".bcsar","CSAR", FormatType.Audio, "3DS Sound Archive", Nin_),
            new FormatInfo(".arc","darc", FormatType.Archive, "3DS Archive", Nin_),
            new FormatInfo(".brplt", FormatType.Else, "Wii Palette", Nin_),
            new FormatInfo(".brcha", FormatType.Else, "Wii Bone", Nin_),
            new FormatInfo(".brsca", FormatType.Else, "Wii Scene Settings", Nin_),
            new FormatInfo(".brtpa", FormatType.Else, "Wii Texture Pattern", Nin_),
            new FormatInfo(".dol", FormatType.Executable, "Main Executable", Nin_),
            new FormatInfo(".REL", FormatType.Executable, "Wii Executable LIB", Nin_),
            new FormatInfo(".dol",new Identifier32(174,15,56,162), FormatType.Executable, "GC Executable", Nin_),
            new FormatInfo(".jpc", "JPAC1-00", FormatType.Effect , "JParticle container", Nin_),
            new FormatInfo(".jpc", "JPAC2-10", FormatType.Effect , "JParticle container", Nin_),
            new FormatInfo(".jpc", "JPAC2-11", FormatType.Effect , "JParticle container", Nin_),
            new FormatInfo(".jpa", "JEFFjpa1", FormatType.Effect , "JParticle", Nin_),
            new FormatInfo(".breff", "REFF", FormatType.Effect, "Wii Effect", Nin_),
            new FormatInfo(".esetlist", "EFTB", FormatType.Effect , "Particle effects", Nin_),
            new FormatInfo(".esetlist", "VFXB", FormatType.Effect , "Particle effects", Nin_),
            new FormatInfo(".branm", FormatType.Animation, "Wii Animation", Nin_),
            new FormatInfo(".brtsa", FormatType.Animation, "Wii Texture Animation", Nin_),
            new FormatInfo(".brsha", FormatType.Animation, "Wii Vertex Morph Animation", Nin_),
            new FormatInfo(".brvia", FormatType.Animation, "Wii Visibility Sequence", Nin_),
            new FormatInfo(".tbl", FormatType.Parameter, "JMap data", Nin_),
            new FormatInfo(".bcam", FormatType.Parameter, "JMap camera data", Nin_),
            new FormatInfo(".canm","ANDO", FormatType.Parameter, "JCameraAnimation", Nin_),
            new FormatInfo(".aamp","AAMP", FormatType.Parameter, "binary resource parameter archives", Nin_),
            new FormatInfo(".sp","SPCE",FormatType.Else,"Sunshine Cutscene Event",Nin_),
            new FormatInfo(".sp","SPCB",FormatType.Else,"Sunshine Cutscene Event",Nin_),
            //new FormatInfo(".brmdl", FormatType.Model, "Wii Model Display Lists", Nin_),

            #endregion Nintendo

            #region Second party developer

            //Retro Studios
            new FormatInfo(".PAK",0,new byte[]{0x0, 0x3, 0x0, 0x5, 0x0, 0x0, 0x0, 0x0 }, FormatType.Archive, "Retro Archive", Retro_,typeof(PAK_Retro)), //GC https://www.metroid2002.com/retromodding/wiki/PAK_(Metroid_Prime)#Header
            new FormatInfo(".PAK",0,new byte[]{0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x40 }, FormatType.Archive, "Retro Wii Archive", Retro_,typeof(PAK_RetroWii)), //Wii https://www.metroid2002.com/retfromodding/wiki/PAK_(Metroid_Prime_3)#Header
            new FormatInfo(".TXTR", FormatType.Texture, "Retro Texture", Retro_,typeof(TXTR)),
            new FormatInfo(".AGSC", FormatType.Audio, "Retro sound effect", Retro_), // https://www.metroid2002.com/retromodding/wiki/AGSC_(File_Format)
            new FormatInfo(".CSMP", FormatType.Audio, "Retro Audio", Retro_), // https://www.metroid2002.com/retromodding/wiki/CSMP_(File_Format)
            new FormatInfo(".PART", FormatType.Effect, "Retro Particle System", Retro_),
            new FormatInfo(".WPSC", FormatType.Effect, "Retro Swoosh Particle System", Retro_),
            new FormatInfo(".DCLN", FormatType.Collision, "Retro Dynamic Collision", Retro_),
            new FormatInfo(".RULE", "RULE", FormatType.Parameter, "Retro Studios Rule Set", Retro_),
            new FormatInfo(".SCAN", "SCAN", FormatType.Else, "Metroid Scan", Retro_),
            new FormatInfo(".FONT", FormatType.Font, "Retro Font", Retro_),
            new FormatInfo(".MLVL", "Þ¯º¾", FormatType.Font, "Retro World Data", Retro_),
            new FormatInfo(".ANIM", FormatType.Animation, "Retro animation", Retro_),
            new FormatInfo(".CSKR", FormatType.Parameter, "Retro Skin Rules", Retro_),
            new FormatInfo(".STRG", FormatType.Text, "Retro String Table", Retro_),
            //Retro Studios WiiU & Switch 
            new FormatInfo("", "RFRM", FormatType.Texture, "Retro Format Descriptor", Retro_),

            //Next Level Games
            new FormatInfo(".dict",new Identifier32(0x5824F3A9),0, FormatType.Archive, "Punch Out Dictionary","Next Level Games",typeof(DICTPO)),
            new FormatInfo(".dict",new Identifier32(0xA9F32458),0, FormatType.Archive, "Archive Dictionary","Next Level Games"),
            new FormatInfo(".rlt",new Identifier32("PTLG"),0, FormatType.Texture, "Strikers Texture","Next Level Games",typeof(PTLG)),
            new FormatInfo(".wrlt",new Identifier32("PTLG"),0x10, FormatType.Texture, "Strikers Gameworld Texture","Next Level Games",typeof(PTLG)),
            new FormatInfo(".TexPO", FormatType.Texture, "Punch Out Texture","Next Level Games",typeof(TexPO)),
            new FormatInfo(".Res", FormatType.Texture, "Strikers RES Texture","Next Level Games",typeof(RES_NLG)),
            new FormatInfo(".sanim", FormatType.Animation, "Striker Skeleton Animation","Next Level Games"),
            new FormatInfo(".nlxwb", FormatType.Audio, "Next Level Wave","Next Level Games"),
            new FormatInfo(".fen","FENL", FormatType.Unknown, "","Next Level Games"),

            //HAL Laboratory & Sora Ltd.
            new FormatInfo(".pac",new Identifier32(65,82,67,0), FormatType.Archive, "Brawl Archive","Sora Ltd.",typeof(ARC0)),
            //new FormatInfo(".dat", FormatType.Archive, "HAL Archive", "HAL Laboratory"), // https://wiki.tockdom.com/wiki/HAL_DAT_(File_Format)
            new FormatInfo(".msbin", FormatType.Text,"Brawl Text"),

            //Intelligent Systems
            new FormatInfo(".pak","pack", FormatType.Archive, "Fire Emblem Archive", "Intelligent Systems",typeof(PAK_FE)),

            // Genius Sonority
            new FormatInfo(".fsys", "FSYS", FormatType.Archive, "Genius Sonority Archive", "Genius Sonority",typeof(FSYS)),
            new FormatInfo(".GTX", FormatType.Texture, "Genius Sonority Texture", "Genius Sonority",typeof(GTX)),
            new FormatInfo(".GSscene", FormatType.Texture, "Genius Sonority Scene File (based on sysdolphin)", "Genius Sonority",typeof(GSScene)),
            new FormatInfo(".FLOORDAT", FormatType.Texture, "Genius Sonority Floor Model", "Genius Sonority",typeof(GSScene)),
            new FormatInfo(".MODELDAT", FormatType.Texture, "Genius Sonority Character Model", "Genius Sonority",typeof(GSScene)),
            new FormatInfo(".GSFILE11",new Identifier32(0x7B,0x1E,0xE3,0xF2), FormatType.Archive, "Genius Sonority Unknown (#0x11)", "Genius Sonority",typeof(GSFILE11)),
            new FormatInfo(".PKX", FormatType.Archive, "Genius Sonority Pokémons", "Genius Sonority",typeof(PKX)),
            new FormatInfo(".WZX", FormatType.Archive, "Genius Sonority Attack (Waza)", "Genius Sonority",typeof(WZX)),
            new FormatInfo(".GSW", FormatType.Archive, "Genius Sonority W?", "Genius Sonority"){ Class = typeof(GSW) },
            new FormatInfo(".GSAGTX", FormatType.Archive, "Genius Sonority Animated Texture", "Genius Sonority",typeof(GSAGTX)),
            new FormatInfo(".GPT", "GPT0", FormatType.Unknown, "Genius Sonority Particle v0", "Genius Sonority"),
            new FormatInfo(".GPT", "GPT1", FormatType.Unknown, "Genius Sonority Particle v1", "Genius Sonority"),
            new FormatInfo(".GPT", 0,new byte[]{ 0x01, 0xF0, 0x5, 0xDA, 0x00, 0x03, 0x00, 0x02 }, FormatType.Unknown, "Genius Sonority Particle (unknown)", "Genius Sonority"),

            // Genius Sonority (non-textures)
            new FormatInfo(".MSG", FormatType.Text, "Genius Sonority Messages", "Genius Sonority"),
            new FormatInfo(".FNT", FormatType.Font, "Genius Sonority Font", "Genius Sonority"), // TODO?
            new FormatInfo(".CCD", FormatType.Collision, "Genius Sonority Collision", "Genius Sonority"),
            new FormatInfo(".SCD", FormatType.Skript, "Genius Sonority Script", "Genius Sonority"),
            new FormatInfo(".ISH", FormatType.Audio, "Genius Sonority Music header", "Genius Sonority"),
            new FormatInfo(".ISD", FormatType.Audio, "Genius Sonority Music data", "Genius Sonority"),
            new FormatInfo(".THH", FormatType.Video, "THP Video header", "Genius Sonority"),
            new FormatInfo(".THD", FormatType.Video, "THP Video data", "Genius Sonority"),

            #endregion Second party developer

            #region Common

            //Common Archives
            new FormatInfo(".rar","Rar!", FormatType.Archive, "Roshal Archive","win.rar GmbH",typeof(SevenZip)),
            new FormatInfo(".zip",new Identifier32((byte)'P',(byte)'K',3,4), FormatType.Archive, "zip Archive","PKWARE, Inc",typeof(SevenZip)),
            new FormatInfo(".zip",new Identifier32((byte)'P',(byte)'K',5,6), FormatType.Dummy, "Empty zip Archive","PKWARE, Inc",typeof(SevenZip)),
            new FormatInfo(".zip",new Identifier32((byte)'P',(byte)'K',7,8), FormatType.Archive, "zip spanned Archive","PKWARE, Inc",typeof(SevenZip)),
            new FormatInfo(".7z",0,new byte[]{55, 122, 188, 175, 39, 28}, FormatType.Archive, "7-Zip archive","Igor Pavlov",typeof(SevenZip)),
            new FormatInfo(".tar","ustar", FormatType.Archive, "Unix Standard TAR","Unix") { Class = typeof(SevenZip)},
            new FormatInfo(".deb","!<arch>␊", FormatType.Archive, "Debian pack","The Debian Projec") { Class = typeof(SevenZip)},
            new FormatInfo(".dmg",0,new byte[]{120, 1, 115, 13, 98, 98, 96}, FormatType.Archive, "Apple Disk Image","Apple Inc.",typeof(SevenZip)),
            new FormatInfo(".rpm",0,new byte[]{237, 171, 238, 219}, FormatType.Archive, "Red Hat Pack","Red Hat",typeof(SevenZip)),
            new FormatInfo(".xar","xar!", FormatType.Archive, "eXtensible ARchive format","OpenDarwin project",typeof(SevenZip)),
            new FormatInfo(".bz2","BZ", FormatType.Archive, "BZip2 compression","Julian Seward",typeof(SevenZip)) { IsMatch = BZip_Matcher},
            new FormatInfo(".lzh","-lh", FormatType.Archive, "LHA compression","Haruyasu Yoshizaki",typeof(SevenZip)),
            new FormatInfo(".sqfs",0,new byte[]{ 104, 115, 113, 115 }, FormatType.Archive, "Squashfs Binary Format","Phillip Lougher",typeof(SevenZip)),
            new FormatInfo(".gz",0,new byte[]{31,139}, FormatType.Archive, "GNU zip","GNU Project",typeof(GZip)),
            //new FormatInfo(".arj",new byte[]{96, 234},0, FormatType.Archive, "Archived by Robert Jung","Robert K. Jung"),
            new FormatInfo(".LZ", "LZSS", FormatType.Archive, "Lempel–Ziv–SS", "Storer–Szymanski",typeof(LZSS)),
            new FormatInfo(".LZ",0, new byte[] { (byte)'L', (byte)'z', (byte)'S', 0 }, FormatType.Archive, "Lempel-Ziv-Stac", "Stac Electronics"),
            new FormatInfo(".Lz00", "LZ00", FormatType.Archive, "Lempel-Ziv 00 "+comp_,string.Empty,typeof(LZ00)),
            new FormatInfo(".Lz01", "LZ01", FormatType.Archive, "Lempel-Ziv 01 "+comp_,string.Empty,typeof(LZ01)),
            new FormatInfo(".lz77","LZ77", FormatType.Archive, "Lempel-Ziv 77 Wii",string.Empty,typeof(LZ77)),
            new FormatInfo(".Comp","COMP", FormatType.Archive, comp_,string.Empty,typeof(COMP)),
            new FormatInfo(".CNX",new Identifier32((byte)'C',(byte)'N',(byte)'X',0x2), FormatType.Archive, comp_,string.Empty,typeof(CNX2)),
            new FormatInfo(".CXLZ","CXLZ", FormatType.Archive, comp_,string.Empty,typeof(CXLZ)),
            new FormatInfo(".LZ", FormatType.Archive, "Lempel-Ziv " + comp_),
            new FormatInfo(".ZS",new Identifier32(4247762216), FormatType.Archive, "Zstandard " + comp_,string.Empty,typeof(Zstd)),
            //new FormatInfo(".zlib", FormatType.Archive, comp_,"Mark Adle",typeof(ZLib)),
            new FormatInfo(".tar","KIJ=H", FormatType.Archive, "tape archive"),

            //Common Textures
            new FormatInfo(".PNG", 0,new byte[]{137,80,78,71,13,10,26,10}, FormatType.Texture, "Portable Network Graphics"),
            new FormatInfo(".Gif", "GIF87a", FormatType.Texture, "Graphics Interchange Format"),
            new FormatInfo(".Gif", "GIF89a", FormatType.Texture, "Graphics Interchange Format"),
            new FormatInfo(".Jpg", 0,new byte[]{255,216,255,224}, FormatType.Texture, "Joint Photographic Group"),
            new FormatInfo(".tga", FormatType.Texture, "Truevision Graphic Advanced","Truevision", typeof(TGA)),
            new FormatInfo(".psd", new Identifier32("8BPS"), FormatType.Texture, "Photoshop Document file", "Adobe Inc."),
            
            //Common Audio
            new FormatInfo(".ogg",new Identifier32("0ggS"), FormatType.Audio,"Ogg Vorbis audio", "Xiph.Org Foundation"),
            new FormatInfo(".mp3","ID3", FormatType.Audio, "MPEG Audio Layer III"),
            new FormatInfo(".mid","MThd", FormatType.Audio,"Musical Instrument Digital Interface"),

            //Commen Else
            new FormatInfo(".elf",new Identifier32(127,(byte)'E',(byte)'L',(byte)'F'), FormatType.Executable,"Executable and Linkable Format", "Unix System Laboratories"),
            new FormatInfo(".class",new Identifier32(0xCA,0xFE,0xBA,0xBE), FormatType.Executable,"Java class file", "Sun Microsystems"),
            new FormatInfo(".pdf","%PDF",FormatType.Text,"Portable Document Format","Adobe Inc."),
            new FormatInfo(".json",FormatType.Text,"JavaScript Object Notation"),
            new FormatInfo(".py",FormatType.Skript,"Python Skript","Python Software"),
            new FormatInfo(".bat",FormatType.Skript,"Batch file"),
            new FormatInfo(".t",FormatType.Text),
            new FormatInfo(".htm", FormatType.Else, "Hypertext Markup"),
            new FormatInfo(".MAP", FormatType.Else, "Debugger infos"),
            new FormatInfo(".lua", FormatType.Skript, "Script"),
            new FormatInfo(".cpp", FormatType.Skript, "C++ Source code"),
            new FormatInfo(".h",FormatType.Text,"Header file"),
            new FormatInfo(".pdb","BSJB",FormatType.Else,"PDB Symboles"),

            //Microsoft
            new FormatInfo(".cab","MSCF", FormatType.Archive, "Cabinet Archive", "Microsoft",typeof(SevenZip)),
            new FormatInfo(".vhd","conectix", FormatType.Archive, "Virtual Hard Disk","Microsoft",typeof(SevenZip)),
            new FormatInfo(".chm","ITSF", FormatType.Archive, "Compiled HTML Help"," Microsoft",typeof(SevenZip)),
            new FormatInfo(".bmp", "BM", FormatType.Texture,"BitMap Picture", "Microsoft"),
            new FormatInfo(".DDS", "DDS ", FormatType.Texture, "Direct Draw Surface", "Microsoft",typeof(DDS)),
            new FormatInfo(".exe", 0,new byte[]{77,90,144,0}, FormatType.Executable, "Windows Executable", "Microsoft"),

            //Text
            new FormatInfo(".txt", FormatType.Text,"Text file"),
            new FormatInfo(".log", FormatType.Text, "Log file"),
            new FormatInfo(".xml", FormatType.Text,"eXtensible Markup Language file"),
            new FormatInfo(".csv", FormatType.Text,"Comma Separated Values"),
            new FormatInfo(".inf", FormatType.Text, "info file"),
            new FormatInfo(".ini", FormatType.Text, "Configuration file"),

            //Roms & Iso
            new FormatInfo(".gba", 0,new byte[]{46,0,0,234,36,255,174,81,105,154,162,33,61,132,130}, FormatType.Rom, "GBA Rom", Nin_),
            new FormatInfo(".nes", 0,new byte[]{78,69,83,26} , FormatType.Rom, "Rom", Nin_),
            new FormatInfo(".rvz", 0,new byte[]{82,86,90,1} , FormatType.Iso, "Dolphin Iso", "Dolphin Team"),
            new FormatInfo(".WIA", 0,new byte[]{87,73,65,1} , FormatType.Iso, "Wii ISO Archive","Wiimm"),
            new FormatInfo(".wad",new Identifier32((byte)'I', (byte)'s', 0, 0),4, FormatType.Iso, "Wii WAD",Nin_,typeof(WAD)),
            new FormatInfo(".ciso", FormatType.Iso, "Compact ISO"),
            new FormatInfo(".iso", FormatType.Iso, "Gamecube Mini Disc Image",Nin_,typeof(GCDisk)),
            new FormatInfo(".iso", FormatType.Iso, "Wii Disc Image",Nin_,typeof(WiiDisk)),
            new FormatInfo(".iso", "CD001", FormatType.Iso, "ISO-9660 table","",typeof(SevenZip)),
            new FormatInfo(".nsp", "PFS0", FormatType.Iso, "Switch Partition",Nin_),
            new FormatInfo(".WDF", FormatType.Iso, "Wii Disc Format","Wiimm"),
            new FormatInfo(".GCZ", FormatType.Iso, "GameCube Zip"),
            new FormatInfo(".wbfs", FormatType.Iso, "Wii Backup File System"),

            #endregion Common

            #region Mixed

            //Sting Entertainment
            new FormatInfo(".PAC", FormatType.Archive, "Archive", "Sting Entertainment",typeof(PAC)),
            new FormatInfo(".PIM", FormatType.Texture, "Image", "Sting Entertainment",typeof(PIM)),
            new FormatInfo(".PIL", FormatType.Texture, "Image Conteiner", "Sting Entertainment",typeof(PIL)),

            //CRIWARE
            new FormatInfo(".cpk", "CPK ", FormatType.Archive, "Compact Archive", "CRIWARE",typeof(CPK)),
            new FormatInfo(".CCRI", "CRILAYLA" , FormatType.Archive, "Compact Compressed", "CRIWARE",typeof(CRILAYLA)),
            new FormatInfo(".afs", new Identifier32((byte)'A',(byte)'F',(byte)'S',(byte)' '), FormatType.Archive, "File Archive", "CRIWARE",typeof(AFS)),
            new FormatInfo(".afs", new Identifier32((byte)'A',(byte)'F',(byte)'S',0), FormatType.Archive, "File Archive", "CRIWARE",typeof(AFS)),
            new FormatInfo(".adx", "€", FormatType.Audio, "CRI Audio", "CRIWARE"){ IsMatch = ADX_Matcher},
            new FormatInfo(".aix", FormatType.Audio, "CRI Audio Archive", "CRIWARE"),
            new FormatInfo(".sfd", 2, new byte[] { 1, 186, 33 } , FormatType.Video, "SofDec Video", "CRIWARE"),
            
            //	Atlus
            new FormatInfo(".tpx", FormatType.Texture, "Texture Palette Xtension", "Atlus"){ Class = typeof(TPX)},

            //Capcom
            new FormatInfo("", FormatType.Archive, "MT Framework Archive", "Capcom"){ Class = typeof(ARC_MTF)},

            //Disney
            new FormatInfo(".pak", FormatType.Archive, "Junction Point Studios Archive", "Junction Point Studios"){ Class = typeof(PAK_JPS)},

            //Gamebase
            new FormatInfo(".nif_wii", FormatType.Texture, "Gamebryo File Format", "Gamebase"),

            //UbiSoft
            new FormatInfo(".bf",new Identifier32((byte)'B',(byte)'U',(byte)'G',0), FormatType.Archive, "UbiSoft Archive","UbiSoft",typeof(BUG)),
            new FormatInfo(".bf", new Identifier32((byte)'B',(byte)'I',(byte)'G',0), FormatType.Archive, "UbiSoft Archive","UbiSoft",typeof(BIG)),
            new FormatInfo(".waa","RIFF", FormatType.Audio, "UbiSoft Audio"),

            //Keen Games
            new FormatInfo("", FormatType.Archive, "Dawn of Discovery Archive","Keen Games",typeof(PAKb)),

            //Namco Bandai
            new FormatInfo(".dkz", "DKZF", FormatType.Archive, "Donkey Konga"),
            new FormatInfo(".olk", 4,"olnk".GetBytes(), FormatType.Archive, "Archive", "Namco"), //https://forum.xentax.com/viewtopic.php?t=22500
            new FormatInfo(".nut", "NUTC", FormatType.Texture, "Namco Universal Texture", "Namco",typeof(NUTC)),

            //Silicon Knights
            new FormatInfo(".cmp", "*SK_ASC*", FormatType.Archive,"Compressed Eternal Darkness Data", "Silicon Knights"),

            //The Behemoth
            new FormatInfo(".BREC", "RSND", FormatType.Archive,"Encrypted Shockwave Flash?", "The Behemoth"),
            new FormatInfo(".BREC", "NREC", FormatType.Archive,"Encrypted Shockwave Flash", "The Behemoth"),

            //SEGA
            new FormatInfo(".one", FormatType.Archive, "Sonic Storybook Series Archive", "SEGA",typeof(ONE_SB)),
            new FormatInfo(".one","one.", FormatType.Archive, "Sonic Unleashed Archive", "SEGA",typeof(ONE_UN)),
            new FormatInfo(".one", FormatType.Archive, "Shadow The Hedgehog Archive", "SEGA",typeof(ONE_SH)){IdentifierOffset = 0xC},
            new FormatInfo(".one", FormatType.Archive, "Sonic Archive", "SEGA"),
            new FormatInfo(".TXD", "TXAG", FormatType.Archive, "Sonic Storybook Texture Archive", "SEGA",typeof(TXAG)),
            new FormatInfo(".gvm", "GVMH", FormatType.Archive, "SEGA Texture archive", "SEGA",typeof(GVMH)),
            new FormatInfo(".gvr", "GBIX", FormatType.Texture, "VR Texture", "SEGA",typeof(GBIX)),
            new FormatInfo(".gvr", "GCIX", FormatType.Texture, "VR Texture", "SEGA",typeof(GCIX)),
            new FormatInfo(".gvrt","GVRT", FormatType.Texture, "VR Texture", "SEGA",typeof(GVRT)),
            new FormatInfo(".pvr","PVRT", FormatType.Texture, "VR Texture", "SEGA"),
            new FormatInfo(".NEP", FormatType.Archive, "NEP Archive", "SEGA",typeof(NEP)),
            new FormatInfo(".GNM","NGIF", FormatType.Model, "GNM Model", "SEGA"),
            new FormatInfo(".pef","0BEP", FormatType.Effect, "PEF Effect", "SEGA"),
            //new FormatInfo("", 0, new byte[] { 128, 0, 0, 1, 0 }, FormatType.Archive, "Sonic Riders lzss", "SEGA"), //https://github.com/romhack/sonic_riders_lzss
            new FormatInfo(".rvm","CVMH", FormatType.Archive, "Sonic Riders Archive", "SEGA"),
            new FormatInfo(".XVRs", FormatType.Texture, "Sonic Riders Texture", "SEGA"), //https://github.com/Sewer56/SonicRiders.Index/tree/master/Source
            new FormatInfo(".fmi","BD@M", FormatType.Unknown, "", "SEGA"),
            //SEGA Hitmaker
            new FormatInfo(".bin","PK_0", FormatType.Archive, "Zip "+comp_, "Hitmaker"),
            
            //Sega AM1 Overworks
            new FormatInfo(".dat", "AKLZ~?Qd=ÌÌÍ", FormatType.Archive,"Skies of Arcadia Legends","Overworks",typeof(AKLZ)),

            //Imageepoch
            new FormatInfo(".vol", "RTDP", FormatType.Archive, "Arc Rise Archive", "Imageepoch",typeof(RTDP)),
            new FormatInfo(".wtm", "WTMD", FormatType.Texture, "Arc Rise Texture", "Imageepoch",typeof(WTMD)),

            //Natsume
            new FormatInfo(".pBin", "pBin", FormatType.Archive, "Harvest Moon Archive", "Natsume",typeof(pBin)),
            new FormatInfo(".tex", FormatType.Texture, "Harvest Moon Texture", "Natsume",typeof(FIPAFTEX)),

            //Neverland
            new FormatInfo(".bin", "FBTI", FormatType.Archive, "Rune Factory Archive", "Neverland",typeof(FBTI)),
            new FormatInfo(".bin", "NLCM", FormatType.Archive, "Rune Factory Archive Header", "Neverland",typeof(NLCM)),
            new FormatInfo("", "NLCL", FormatType.Archive, "Rune Factory Archive 2", "Neverland",typeof(NLCL)),
            new FormatInfo("", "MEDB", FormatType.Archive, "Rune Factory Archive 3", "Neverland",typeof(MEDB)),
            new FormatInfo(".hvt", "HXTB", FormatType.Texture, "Rune Factory Texture", "Neverland",typeof(HXTB)),
            new FormatInfo(".hxcb", "HXCB", FormatType.Parameter, "Rune Factory Camera", "Neverland"),
            new FormatInfo(".hxab", "HXAB", FormatType.Animation, "Rune Factory Animation", "Neverland"),
            new FormatInfo(".hxhb", "HXHB", FormatType.Parameter, "Rune Factory Bone", "Neverland"),
            new FormatInfo(".hxtp", "HXTP", FormatType.Parameter, "Rune Factory Parameter", "Neverland"),
            new FormatInfo(".hxgb", "HXGB", FormatType.Model, "Rune Factory Model", "Neverland"),
            new FormatInfo(".hxmb", "HXMB", FormatType.Parameter, "Rune Factory data", "Neverland"),

            //Square Enix FFCC Crystal Bearers
            new FormatInfo(".pos","POSD", FormatType.Archive, "Crystal Bearers Archive Header","Square Enix",typeof(POSD)),
            new FormatInfo(".FREB","FREB", FormatType.Archive, "Crystal Bearers Archive", "Square Enix",typeof(FREB)),
            new FormatInfo(".MPD", 0, new byte[] {(byte) 'M',(byte) 'P',(byte) 'D', 0 }, FormatType.Archive, "Crystal Bearers data", "Square Enix",typeof(MPD)),
            new FormatInfo(".FEFF", "FEFF", FormatType.Texture, "Crystal Bearers Model", "Square Enix"){IdentifierOffset = 0x10},
            //Square Enix FINAL FANTASY Crystal Chronicles
            new FormatInfo(".cmd", "CAM ", FormatType.Parameter, "FFCC Camera data","Square Enix"),
            new FormatInfo(".cha", "CHA ", FormatType.Parameter, "FFCC Character data","Square Enix"),
            new FormatInfo(".chm", "CHM ", FormatType.Parameter, "FFCC Object data","Square Enix"),
            new FormatInfo(".tex", "TEX ", FormatType.Archive, "FFCC Texture Archive","Square Enix", typeof(TSET)),
            new FormatInfo(".otm", "OTM ", FormatType.Archive, "FFCC Camera data","Square Enix", typeof(TSET)),
            new FormatInfo(".ptx", "TSET", FormatType.Archive, "FFCC Texture Table","Square Enix", typeof(TSET)),
            new FormatInfo(".txtr", "TXTR", FormatType.Texture, "FFCC Texture data","Square Enix", typeof(TXTRCC)),
            new FormatInfo(".mid", "MID ", FormatType.Unknown, "FFCC mid data","Square Enix"),
            new FormatInfo(".mpl", "MESH", FormatType.Model, "FFCC Model data","Square Enix"),
            new FormatInfo(".pdt", "PDT ", FormatType.Collision, "FFCC Collision data","Square Enix"),
            new FormatInfo(".bgm", "BGM ", FormatType.Audio, "FFCC Background music data","Square Enix"),
            new FormatInfo(".mcd", "MCD ", FormatType.Skript, "FFCC GBA data","Square Enix"),
            new FormatInfo(".cfd", "CFLD", FormatType.Text, "FFCC String Table data","Square Enix"),
            new FormatInfo(".fnt", "FONT", FormatType.Archive, "FFCC Font data","Square Enix",typeof(FONT)),
            new FormatInfo(".sep",0, new byte[] {0x53, 0x65, 0x53, 0x65, 0x70, 0x00, 0x00, 0x00, 0x00}, FormatType.Audio, "Sound Effect data","Square Enix"),
            new FormatInfo(".seb",0, new byte[] {0x53, 0x65, 0x42, 0x6C, 0x6F, 0x63, 0x6B, 0x00, 0x00}, FormatType.Audio, "Sound Block data","Square Enix"),
            new FormatInfo(".str",0, new byte[] {0x53, 0x54, 0x52, 0x00}, FormatType.Audio, "Audio Stream","Square Enix"),
            new FormatInfo(".wd",0, new byte[] {0x57, 0x44, 0x00, 0x00}, FormatType.Audio, "Wav Audio","Square Enix"),

            //Grasshopper Manufacture
            new FormatInfo(".RSL","RMHG", FormatType.Archive, "Grasshopper Archive", "Grasshopper Manufacture",typeof(RMHG)),
            new FormatInfo(".bin","GCT0", FormatType.Texture, "Grasshopper Texture", "Grasshopper Manufacture",typeof(GCT0)),
            new FormatInfo(".bin","CGMG", FormatType.Model, "Grasshopper Model", "Grasshopper Manufacture"),

            //Treasure
            new FormatInfo(".RSC", FormatType.Archive, "Wario World archive", "Treasure",typeof(RSC)),
            new FormatInfo(".arc", "NARC", FormatType.Archive, "Sin and Punishment archive", "Treasure",typeof(NARC)),
            new FormatInfo(".nj", "NJTL", FormatType.Model, "Ninja Model", "Treasure"),//https://gitlab.com/dashgl/ikaruga/-/snippets/2046285

            //Tri-Crescendo
            new FormatInfo(".csl", "CSL ", FormatType.Archive, "Fragile Dreams "+comp_, "Tri-Crescendo"),

            //Vanillaware
            new FormatInfo(".fcmp", "FCMP", FormatType.Archive, "Muramasa "+comp_,"Vanillaware",typeof(FCMP)),//Muramasa - The Demon Blade Decompressor http://www.jaytheham.com/code/
            new FormatInfo(".ftx", "FTEX", FormatType.Archive, "Muramasa Texture Archive","Vanillaware",typeof(FTEX)),
            new FormatInfo(".mbs", "FMBS", FormatType.Model, "Muramasa Model","Vanillaware"),
            new FormatInfo(".nms", "NMSB", FormatType.Text, "Muramasa Text","Vanillaware"),
            new FormatInfo(".nsb", "NSBD", FormatType.Skript, "Muramasa Skript","Vanillaware"),
            new FormatInfo(".abf", "MLIB", FormatType.Parameter, "Muramasa Skript","Vanillaware"),
            new FormatInfo(".esb", "EMBP", FormatType.Parameter, "Muramasa Skript","Vanillaware"),
            new FormatInfo(".otb", "OTB ", FormatType.Audio, "SE"),
            new FormatInfo(".nsi", "NSI ", FormatType.Audio, "SE Info"),

            //Krome Studios
            new FormatInfo(".rkv", "RKV2", FormatType.Archive, "Star Wars Force Unleashed", "Krome Studios",typeof(RKV2)),
            new FormatInfo(".tex", FormatType.Texture, "Star Wars Force Unleashed", "Krome Studios",typeof(TEX_KS)),

            //Red Fly Studios
            new FormatInfo("", FormatType.Texture, "Star Wars Force Unleashed 2", "Red Fly Studios",typeof(TEX_RFS)),
            new FormatInfo(".POD", "POD5", FormatType.Archive, "Star Wars Force Unleashed 2", "Red Fly Studios",typeof(POD5)),
            
            //Criterion
            new FormatInfo(".PSW",0, new byte[] {0x30,0xAF,0x20}, FormatType.Texture, "Texture Palette Library"),
            new FormatInfo(".TXD", FormatType.Texture, "TextureDictionary", "Criterion",typeof(TXD)),

            //H.a.n.d.
            new FormatInfo(".fbc", FormatType.Archive, "Fables Chocobo archive", "H.a.n.d.",typeof(FBC)),

            //Cing
            new FormatInfo(".pac", "PCKG", FormatType.Archive, "Little King's Story Archive", "Cing",typeof(PCKG)),  // also pcha

            //Victor Interactive
            new FormatInfo(".clz",new Identifier32((byte)'C',(byte)'L',(byte)'Z',0), FormatType.Archive, comp_, "Victor Interactive",typeof(CLZ0)),

            //Ganbarion
            new FormatInfo(".apf", FormatType.Archive,"One Piece FSM Archive", "Ganbarion"), //One Piece: Grand Adventure

            //Rare
            new FormatInfo(".ZLB",new Identifier32((byte)'Z',(byte)'L',(byte)'B',0x0), FormatType.Archive, comp_,"",typeof(ZLB)),

            //From Software
            new FormatInfo(".gtx",new Identifier32("GTX1"),4, FormatType.Texture, "Graphics Texture Extension", "From Software",typeof(GTX1)),
            new FormatInfo(".tex", FormatType.Archive, "Texture archive", "From Software",typeof(ARC_FS)),
            new FormatInfo(".ctm", FormatType.Archive, "Model Archive", "From Software",typeof(ARC_FS)),
            new FormatInfo(".ptm", FormatType.Archive, "archive", "From Software",typeof(ARC_FS)),

            //Idea Factory
            new FormatInfo(".s3g", FormatType.Texture, string.Empty, "Idea Factory",typeof(S3G)),
            new FormatInfo(".ONE", "ONE1", FormatType.Archive, "Generation of Chaos Archive", "Idea Factory"),

            //Aqualead. use in Pandora's Tower
            new FormatInfo(".aar","ALAR", FormatType.Archive, "Archive", "Aqualead",typeof(ALAR)), // https://zenhax.com/viewtopic.php?t=16613
            new FormatInfo(".act","ALCT", FormatType.Archive, "Container", "Aqualead"),
            new FormatInfo(".aar","ALLZ", FormatType.Archive, "AL LZSS Compressed", "Aqualead",typeof(ALLZ)), //https://github.com/Brolijah/Aqualead_LZSS
            new FormatInfo(".atx","ALTX", FormatType.Texture, "Texture", "Aqualead",typeof(ALTX)),
            new FormatInfo(".aig","ALIG", FormatType.Texture, "Image", "Aqualead",typeof(ALIG)),
            new FormatInfo(".ams","ALMS", FormatType.Collision, "Mesh Collision", "Aqualead"),
            new FormatInfo(".asn","ALSN", FormatType.Audio, "Sound", "Aqualead"),
            new FormatInfo(".amt","ALMT", FormatType.Animation, "Motion", "Aqualead"),
            new FormatInfo(".atm","ALTM", FormatType.Layout, "Tile Map", "Aqualead"),
            new FormatInfo(".asd","ALSD", FormatType.Shader, "Shader", "Aqualead"),
            new FormatInfo(".aft","ALFT", FormatType.Font, "Font", "Aqualead"),
            new FormatInfo(".apt","ALPT", FormatType.Parameter, "Pad Trace", "Aqualead"),
            new FormatInfo(".aod","ALOD", FormatType.Else, "Object Definition", "Aqualead"),
            new FormatInfo(".atb","ALTB", FormatType.Else, "Table", "Aqualead"),
            new FormatInfo(".ard","ALRD", FormatType.Else, "Record Prop", "Aqualead"),
            new FormatInfo(".GCLz","GCLZ", FormatType.Archive, comp_,"",typeof(GCLZ)),

            //Hudson Soft
            new FormatInfo(".bin", FormatType.Archive, "Mario Party Archive", "Hudson Soft",typeof(BIN_MP)),
            new FormatInfo(".hsf",new Identifier64(15537406417982280), FormatType.Texture,"Mario Party Model","Hudson Soft",typeof(HSF)),
            new FormatInfo(".atb", FormatType.Texture,"Mario Party texture","Hudson Soft",typeof(ATB)),
            new FormatInfo(".h3m", "HVQM3 1." , FormatType.Video,"Video","Hudson Soft"),
            new FormatInfo(".h4m", "HVQM4 1." , FormatType.Video,"Video","Hudson Soft"),

            //Radical Entertainment
            new FormatInfo(".rcf", "ATG CORE CEMENT LIBRARY", FormatType.Archive,"","Radical Entertainment"),
            new FormatInfo(".rcf", "RADCORE CEMENT LIBRARY", FormatType.Archive,"","Radical Entertainment"),
            new FormatInfo(".p3d", "P3DZ", FormatType.Archive,"Compressed Pure3D file","Radical Entertainment"),
            new FormatInfo(".p3d", "P3Dÿ", FormatType.Archive,"Pure3D file","Radical Entertainment"),
            new FormatInfo(".p3d", new Identifier32((byte)'R',(byte)'Z',0,0) , FormatType.Archive,"Pure3D RZ file","Radical Entertainment"),
            
            //Eurocom
            new FormatInfo(".000", FormatType.Archive, "Eurocom Archive","Eurocom", typeof(Filelist)), //https://github.com/eurotools/eurochef
            new FormatInfo(".csb","MUSX", FormatType.Audio, "Eurocom Audio","Eurocom"),
            new FormatInfo(".edb","GEOM", FormatType.Archive, "Eurocom","Eurocom"),

            //EA
            new FormatInfo(".viv","BIG4", FormatType.Audio,"BIG Audio","EA"),
            new FormatInfo(".vpa","VPAK", FormatType.Unknown,"V Archive","EA"),
            new FormatInfo(".vp6","MVhd", FormatType.Video,"VP6","EA"),
            new FormatInfo(".moi","MOIR", FormatType.Unknown,"","EA"),
            new FormatInfo(".abk","ABKC", FormatType.Audio,"audio bank","EA"),
            new FormatInfo(".bnk","BNKb", FormatType.Audio,"audio","EA"),
            new FormatInfo(".asf","SCHl", FormatType.Audio,"audio","EA"),
            new FormatInfo(".loc","LOCH", FormatType.Unknown,"","EA"),
            new FormatInfo(".pfd","PFDx", FormatType.Unknown,"","EA"),
            
            //Rebellion
            new FormatInfo(".asrBE","AsuraZlb", FormatType.Archive,comp_, "Rebellion",typeof(AsuraZlb)),
            new FormatInfo(".asrBE","Asura   ", FormatType.Archive,"Asura Archive", "Rebellion",typeof(Asura)),
            new FormatInfo(".txth","TXTH", FormatType.Text,"Asura Text", "Rebellion"),
            new FormatInfo(".fcsr","FCSR", FormatType.Archive,"Asura archive entry", "Rebellion"),
            new FormatInfo(".txtt","TXTT", FormatType.Parameter,"Asura texture info", "Rebellion"),
            new FormatInfo(".ofnf","OFNF", FormatType.Parameter,"Asura archive info", "Rebellion"),
            new FormatInfo(".lfsr","LFSR", FormatType.Parameter,"", "Rebellion"),
            new FormatInfo(".msds","MSDS", FormatType.Parameter,"", "Rebellion"),
            new FormatInfo(".veld","VELD", FormatType.Parameter,"", "Rebellion"),

            //Red Entertainment
            new FormatInfo(".pak", FormatType.Archive, "Tengai Makyō II Archive", "Red Entertainment",typeof(PAK_TM2)),
            new FormatInfo(".cns","@CNS", FormatType.Archive, "CNS Compressed", "Red Entertainment",typeof(CNS)),
            new FormatInfo(".hps"," HALPST", FormatType.Audio, "Audio", "Red Entertainment"),
            new FormatInfo(".iwf"," FWS4", FormatType.Unknown, "?", "Red Entertainment"),
            new FormatInfo(".exi"," MROF", FormatType.Unknown, "?", "Red Entertainment"),
            
            //AQ Interactive
            new FormatInfo(".pk", FormatType.Archive, "Archive","AQ Interactive",typeof(PK_AQ)),
            new FormatInfo(".texture",0, new byte[] { 0x63, 0x68, 0x6E, 0x6B, 0x64, 0x61, 0x74, 0x61, 0x00, 0x00, 0x00, 0x00, 0x77, 0x69, 0x69, 0x20, 0x74, 0x65, 0x78, 0x74 }, FormatType.Texture, "Texture data","AQ Interactive",typeof(text_AQ)),
            new FormatInfo(".model",0, new byte[] { 0x63, 0x68, 0x6E, 0x6B, 0x64, 0x61, 0x74, 0x61, 0x00, 0x00, 0x00, 0x00, 0x77, 0x69, 0x69, 0x20, 0x6D, 0x6F, 0x64, 0x6C }, FormatType.Model, "Model data","AQ Interactive"),
            new FormatInfo(".model",0, new byte[] { 0x63, 0x68, 0x6E, 0x6B, 0x64, 0x61, 0x74, 0x61, 0x00, 0x00, 0x00, 0x00, 0x77, 0x69, 0x69, 0x20, 0x6F, 0x63, 0x63, 0x74}, FormatType.Unknown, "Model Occt data","AQ Interactive"),
            new FormatInfo(".motion",0, new byte[] { 0x63, 0x68, 0x6E, 0x6B, 0x64, 0x61, 0x74, 0x61, 0x00, 0x00, 0x00, 0x00, 0x77, 0x69, 0x69, 0x20, 0x61, 0x6E, 0x69, 0x6D }, FormatType.Animation, "Animation data","AQ Interactive"),
            new FormatInfo(".locator",0, new byte[] { 0x63, 0x68, 0x6E, 0x6B, 0x64, 0x61, 0x74, 0x61, 0x00, 0x00, 0x00, 0x00, 0x77, 0x69, 0x69, 0x20, 0x6C, 0x6F, 0x63, 0x74 }, FormatType.Parameter, "Locator data","AQ Interactive"),
            new FormatInfo(".locator",0, new byte[] { 0x63, 0x68, 0x6E, 0x6B, 0x64, 0x61, 0x74, 0x61, 0x00, 0x00, 0x00, 0x00, 0x77, 0x69, 0x69, 0x20, 0x6C, 0x6F, 0x63, 0x73 }, FormatType.Unknown, "Locator data","AQ Interactive"),
            new FormatInfo(".hocb"," COH@", FormatType.Collision, "collision data","AQ Interactive"),
            new FormatInfo(".eff"," @EFF", FormatType.Effect, "effect data","AQ Interactive"),
            new FormatInfo(".xsca"," @FSX", FormatType.Unknown, "data","AQ Interactive"),
            new FormatInfo(".hcb"," BCH@", FormatType.Unknown, "data","AQ Interactive"),
            
            //Activision & Shaba Games & Treyarch
            new FormatInfo(".DIR", FormatType.Archive, "Shrek SuperSlam Dir","Shaba Games",typeof(ShrekDir)),
            new FormatInfo(".texpack","TXPK", FormatType.Texture, "Shrek Texture","Shaba Games"),
            new FormatInfo(".cmn", FormatType.Archive,"","Treyarch",typeof(CMN)), //http://wiki.xentax.com/index.php/NHL_2K3_CMN
            new FormatInfo(".gct","GCNT", FormatType.Texture, "GameCube Texture","Activision",typeof(GCNT)), //http://wiki.xentax.com/index.php/GCT_Image
            new FormatInfo(".gct","GCNT", FormatType.Texture, "GameCube Texture","Activision",typeof(GCNT)){ IdentifierOffset=8 },
            new FormatInfo(".snd","SOND", FormatType.Audio, "Sond","Activision"),
            new FormatInfo(".aud", FormatType.Audio, "GameCube Audio","Activision"),

            //Edge of Reality
            new FormatInfo(".lfxt","LFXT", FormatType.Texture, "Pitfall Texture","Edge of Reality",typeof(LFXT)),
            new FormatInfo(".txfl","TXFL", FormatType.Texture, "Pitfall Texture","Edge of Reality"),
            new FormatInfo(".arc", FormatType.Archive, "Pitfall Archive","Edge of Reality",typeof(ARC_Pit)),

            //Eighting
            new FormatInfo(".fpk", FormatType.Archive, "Archive","Eighting",typeof(FPK)),

            //Tecmo
            new FormatInfo(".gsr", FormatType.Texture, "Pangya Texture","Tecmo",typeof(GSR)),
            new FormatInfo(".gsb", FormatType.Audio, "Pangya Audio","Tecmo"),

            //mix
            //new FormatInfo(".cmpr","CMPR", FormatType.Archive, "compressed Data"),
            new FormatInfo(".dir", FormatType.Else, "Archive Info"),
            new FormatInfo(".dict", 0,new byte[]{169,243,36,88,6,1}, FormatType.Archive),


            //Audio
            new FormatInfo(".mul", FormatType.Audio),
            new FormatInfo(".csb","@UTF", FormatType.Audio),
            new FormatInfo(".fsb","FSB3", FormatType.Audio),
            new FormatInfo(".dsp","Cstr", FormatType.Audio),
            new FormatInfo(".aix","AIXF", FormatType.Audio),
            new FormatInfo(".whd", FormatType.Audio,"zack and wiki Audio"),
            //new FormatInfo("","FJF", FormatType.Audio),
            new FormatInfo(".wt", FormatType.Audio, "Wave"),
            new FormatInfo(".bwav", FormatType.Audio, "Wave"),
            new FormatInfo(".wav","RIFX", FormatType.Audio, "Wave"),
            new FormatInfo(".afc", FormatType.Audio, "Stream"),
            new FormatInfo(".cit", FormatType.Audio, "Chord information table"),
            new FormatInfo(".cbd", FormatType.Audio, "data"),
            new FormatInfo(".rsd", FormatType.Audio, "MADWORLD"),
            new FormatInfo(".c3d", FormatType.Else, "3D Audio Position"),
            new FormatInfo(".chd",new Identifier32((byte)'C',(byte)'H',(byte)'D',0), FormatType.Else),
            //Video
            new FormatInfo(".dat", "MOC5", FormatType.Video,"Mobiclip"),
            new FormatInfo(".mds", "MDSV", FormatType.Video,"zack and wiki Video"),
            new FormatInfo(".bik", "BIKi", FormatType.Video,"Bink","Epic Game"),

            //Font
            new FormatInfo(".bfn", "FONTbfn1", FormatType.Font),
            //Model
            new FormatInfo(".HGO","HGOF", FormatType.Model),
            new FormatInfo(".CMDL", FormatType.Model),
            new FormatInfo(".MREA", FormatType.Model, "Area"),
            new FormatInfo(".fpc", FormatType.Model, "pac file container"),
            //else
            new FormatInfo(".ssf","SEC ",FormatType.Archive,"Deadly Alliance"),
            new FormatInfo(".kxe","KXER",FormatType.Skript,"Kuribo Mod","riidefi"),
            new FormatInfo(".bas", FormatType.Animation, "Sound Animation"),
            new FormatInfo(".blight", "LGHT", FormatType.Effect, "Light"),
            new FormatInfo(".bfog", "FOGM", FormatType.Else, "Fog"),
            new FormatInfo(".cam", FormatType.Else, "Camera data"),
            new FormatInfo(".bin", "BTGN", FormatType.Else, "Materials"),
            new FormatInfo(".pac", "NPAC", FormatType.Else, "Star Fox Assault"),
            new FormatInfo(".blmap", "LMAP", FormatType.Else, "Light Map"),
            new FormatInfo(".idb", "looc", FormatType.Else, "Debugger infos"),
            new FormatInfo(".pkb", "SB  ", FormatType.Skript, "Skript"),
            new FormatInfo(".efc", 0, new byte[] { 114, 117, 110, 108, 101, 110, 103, 116, 104, 32, 99, 111, 109, 112, 46 }, FormatType.Unknown),

            //dummys
            new FormatInfo(".empty", FormatType.Dummy, "empty file"){IsMatch = Empty_Matcher},
            new FormatInfo(".zero", FormatType.Dummy, "dummy file"){IsMatch = Zero_Matcher},
            new FormatInfo(".dummy", "dummy", FormatType.Dummy, "dummy file"),
            new FormatInfo(".zzz", FormatType.Dummy, "place holder"),

            #endregion Mixed
        };

        #region Help Matcher

        //Is needed to detect ADX files reliably.
        private static bool ADX_Matcher(Stream stream, ReadOnlySpan<char> extension = default)
        {
            if (stream.ReadByte() != 128 || stream.ReadByte() != 0)
                return false;
            ushort CopyrightOffset = stream.ReadUInt16(Endian.Big);
            if (CopyrightOffset < 8) return false;
            stream.Seek(CopyrightOffset - 2, SeekOrigin.Begin);
            return stream.Match("(c)CRI");
        }

        private static bool BZip_Matcher(Stream stream, ReadOnlySpan<char> extension = default)
        {
            Span<byte> data = stackalloc byte[4];
            stream.Read(data);
            return stream.Length > 4 && data[0] == 66 && data[1] == 90 && (data[2] == 104 || data[2] == 0) && data[3] >= 49 && data[3] <= 57;
        }

        private static bool LZH_Matcher(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 5 && stream.Match("-lz") && stream.ReadByte() > 32 && stream.Match("-");

        private static bool BIN_LM_Matcher(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.ReadUInt8() == 2 && extension.Contains(".bin", StringComparison.InvariantCultureIgnoreCase) && stream.ReadString(2).Length == 2;

        private static bool Empty_Matcher(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length == 0;

        private static bool Zero_Matcher(Stream stream, ReadOnlySpan<char> extension = default)
        {
            while (stream.Length < 0x40)
            {
                int i8 = stream.ReadByte();
                if (i8 != 0)
                {
                    return false;
                }
                if (i8 == -1)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
