using AuroraLip.Compression.Formats;
using AuroraLip.Texture.Formats;

namespace AuroraLip.Common
{
    public static partial class FormatDictionary
    {
        private const string Nin_ = "Nintendo";
        private const string Retro_ = "Retro Studios";

        public static readonly FormatInfo[] Master =
        {
            #region Nintendo
            //Nintendo Archive
            new FormatInfo(".arc", "RARC", FormatType.Archive,"Resources Archive", Nin_),
            new FormatInfo(".arc", "Uª8-", FormatType.Archive, "U8 Archive", Nin_),
            new FormatInfo(".szs", "Yaz0", FormatType.Archive, "compressed Archive", Nin_),
            new FormatInfo(".szp", "Yay0", FormatType.Archive, "compressed Archive", Nin_),
            new FormatInfo(".brres","bres", FormatType.Archive, "Wii Resources Archive", Nin_),

            //Nintendo Textures
            new FormatInfo(".breft","REFT", FormatType.Texture, "Wii Effect Texture", Nin_),
            new FormatInfo(".TPL", new byte[]{32,175,48}, 1, FormatType.Texture, "Texture Palette Library", Nin_){ Class = typeof(TPL) },
            new FormatInfo(".txe", FormatType.Texture, "Dolphin 1 Texture", Nin_){ Class = typeof(TXE) },
            new FormatInfo(".bti", FormatType.Texture, "Binary Texture Image", Nin_){ Class = typeof(BTI) },

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
            new FormatInfo(".bmt", "J3D2bmt3", FormatType.Else),
            //NW4R
            new FormatInfo(".tex0","TEX0", FormatType.Texture, "NW4R Texture", Nin_),
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
            new FormatInfo(".dsp", FormatType.Audio, "Nintendo ADPCM codec", Nin_),
            new FormatInfo(".idsp","IDSP", FormatType.Audio,"Nintendo ADPCM codec", Nin_),
            new FormatInfo(".baa", FormatType.Audio, "JAudio archive", Nin_),
            new FormatInfo(".aw", FormatType.Audio, "JAudio wave archive", Nin_),
            new FormatInfo(".bms", FormatType.Audio, "JAudio music sequence", Nin_),
            new FormatInfo(".bct", FormatType.Audio, "Wii Remote sound info", Nin_),
            new FormatInfo(".csw", FormatType.Audio, "Wii Remote sound effect", Nin_),
            new FormatInfo(".thp", "THP", FormatType.Video,"", Nin_),
            
            //Text
            new FormatInfo(".bmc","MGCLbmc1", FormatType.Text, "message data", Nin_),
            new FormatInfo(".msbt","MsgStdBn", FormatType.Text, "LMS data", Nin_),
            new FormatInfo(".msbf","MsgFlwBn", FormatType.Text, "LMS flow data", Nin_),
            new FormatInfo(".msbp","MsgPrjBn", FormatType.Text, "LMS Prj data", Nin_),
            new FormatInfo(".bmg","MESGbmg1", FormatType.Text, "Binary message container", Nin_),

            //Banner
            new FormatInfo(".bns", FormatType.Else, "Banner", Nin_),
            new FormatInfo(".bnr", new byte[]{66,78,82,49}, 0, FormatType.Else, "Banner", Nin_),
            new FormatInfo(".bnr", new byte[]{66,78,82,50}, 0, FormatType.Else, "Banner", Nin_),
            new FormatInfo(".bnr", new byte[]{73,77,69,84},64, FormatType.Else, "Banner", Nin_),
            new FormatInfo(".pac", FormatType.Else, "Banner", Nin_),

            //Nintendo Else
            new FormatInfo(".blo", "SCRNblo1", FormatType.Layout, "UI Layout", Nin_),
            new FormatInfo(".blo", "SCRNblo2", FormatType.Layout, "UI V2 Layout", Nin_),
            new FormatInfo(".brlan", "RLAN", FormatType.Animation, "Wii layout Animation"),
            new FormatInfo(".brlyt", "RLYT", FormatType.Layout, "Wii structure Layout", Nin_),
            new FormatInfo(".brfnt", "RFNT", FormatType.Font, "Wii Font", Nin_),
            new FormatInfo(".brplt", FormatType.Else, "Wii Palette", Nin_),
            new FormatInfo(".brcha", FormatType.Else, "Wii Bone", Nin_),
            new FormatInfo(".brsca", FormatType.Else, "Wii Scene Settings", Nin_),
            new FormatInfo(".brtpa", FormatType.Else, "Wii Texture Pattern", Nin_),
            new FormatInfo(".dol", FormatType.Executable, "Main Executable", Nin_),
            new FormatInfo(".REL", FormatType.Executable, "Wii Executable LIB", Nin_),
            new FormatInfo(".dol", new byte[]{174,15,56,162},0 , FormatType.Executable, "GC Executable", Nin_),
            new FormatInfo(".elf", new byte[]{127,69,76,70,1,2,1 },0 , FormatType.Executable,"Executable", Nin_),
            new FormatInfo(".jpc", "JPAC1-00", FormatType.Effect , "JParticle container", Nin_),
            new FormatInfo(".jpc", "JPAC2-10", FormatType.Effect , "JParticle container", Nin_),
            new FormatInfo(".jpc", "JPAC2-11", FormatType.Effect , "JParticle container", Nin_),
            new FormatInfo(".jpa", "JEFFjpa1", FormatType.Effect , "JParticle", Nin_),
            new FormatInfo(".breff", "REFF", FormatType.Effect, "Wii Effect", Nin_),
            new FormatInfo(".branm", FormatType.Animation, "Wii Animation", Nin_),
            new FormatInfo(".brtsa", FormatType.Animation, "Wii Texture Animation", Nin_),
            new FormatInfo(".brsha", FormatType.Animation, "Wii Vertex Morph Animation", Nin_),
            new FormatInfo(".brvia", FormatType.Animation, "Wii Visibility Sequence", Nin_),
            new FormatInfo(".tbl", FormatType.Parameter, "JMap data", Nin_),
            new FormatInfo(".bcam", FormatType.Parameter, "JMap camera data", Nin_),
            //new FormatInfo(".brmdl", FormatType.Model, "Wii Model Display Lists", Nin_),
            #endregion

            #region Second party developer
            //Retro Studios
            new FormatInfo(".PAK", FormatType.Archive, "Retro Archive", Retro_), //GC https://www.metroid2002.com/retromodding/wiki/PAK_(Metroid_Prime)#Header Wii https://www.metroid2002.com/retfromodding/wiki/PAK_(Metroid_Prime_3)
            new FormatInfo(".TXTR", FormatType.Texture, "Retro Texture", Retro_){ Class = typeof(TXTR) },
            new FormatInfo(".AGSC", FormatType.Audio, "Retro sound effect", Retro_), // https://www.metroid2002.com/retromodding/wiki/AGSC_(File_Format)
            new FormatInfo(".CSMP", FormatType.Audio, "Retro Audio", Retro_), // https://www.metroid2002.com/retromodding/wiki/CSMP_(File_Format)
            new FormatInfo(".PART", FormatType.Effect, "Retro Particle System", Retro_),
            new FormatInfo(".WPSC", FormatType.Effect, "Retro Swoosh Particle System", Retro_),
            new FormatInfo(".DCLN", FormatType.Collision, "Retro Dynamic Collision", Retro_),
            new FormatInfo(".RULE", "RULE", FormatType.Parameter, "Retro Studios Rule Set", Retro_),
            new FormatInfo(".SCAN", "SCAN", FormatType.Else, "Metroid Scan", Retro_),
            new FormatInfo(".FONT", "FONT", FormatType.Font, "Retro Font", Retro_),
            new FormatInfo(".ANIM", FormatType.Animation, "Retro animation", Retro_),
            new FormatInfo(".CSKR", FormatType.Parameter, "Retro Skin Rules", Retro_),
            new FormatInfo(".STRG", FormatType.Text, "Retro String Table", Retro_),

            //Next Level Games
            new FormatInfo(".rlt","PTLG", FormatType.Texture, "Strikers Texture","Next Level Games"),
            new FormatInfo(".sanim", FormatType.Animation, "Striker Skeleton Animation","Next Level Games"),
            new FormatInfo(".nlxwb", FormatType.Audio, "Next Level Wave","Next Level Games"),
            
            //HAL Laboratory & Sora Ltd.
            new FormatInfo(".pac","ARC", FormatType.Archive, "Brawl Archive"),
            //new FormatInfo(".dat", FormatType.Archive, "HAL Archive", "HAL Laboratory"), // https://wiki.tockdom.com/wiki/HAL_DAT_(File_Format)
            new FormatInfo(".msbin", FormatType.Text),
            #endregion

            #region Common
            //Common Archives
            new FormatInfo(".LZ", "LzS", FormatType.Archive, "Lempel-Ziv-Stac", "Stac Electronics"),
            new FormatInfo(".lz77", FormatType.Archive, "Lempel-Ziv 77"),
            new FormatInfo(".lz77","LZ77", FormatType.Archive, "Lempel-Ziv 77 Wii"),
            new FormatInfo(".gz",new byte[]{31,139},0, FormatType.Archive, "GNU zip","GNU Project"){ Class = typeof(GZip)},
            new FormatInfo(".LZ", FormatType.Archive, "compressed"),
            new FormatInfo(".zlib", FormatType.Archive, "compressed"),
            new FormatInfo(".ZLB","ZLB", FormatType.Archive, "compressed"),
            new FormatInfo(".tar","KIJ=H", FormatType.Archive, "tape archive"),

            //Common Textures
            new FormatInfo(".PNG", new byte[]{137,80,78,71,13},0, FormatType.Texture, "Portable Network Graphics"),
            new FormatInfo(".Jpg", new byte[]{255,216,255,224},0, FormatType.Texture, "Joint Photographic Group"),
            new FormatInfo(".rtf", @"{\rtf1", FormatType.Texture, "Joint Photographic Group"),
            new FormatInfo(".tga", FormatType.Texture, "Truevision Graphic Advanced","Truevision"),

            //Microsoft
            new FormatInfo(".cab","MSCF", FormatType.Archive, "Cabinet Archive", "Microsoft"),
            new FormatInfo(".bmp", "BM", FormatType.Texture,"BitMap Picture", "Microsoft"),
            new FormatInfo(".DDS", "DDS |", FormatType.Texture, "Direct Draw Surface", "Microsoft"),
            new FormatInfo(".exe", new byte[]{77,90,144}, 0, FormatType.Executable, "Windows Executable", "Microsoft"),

            //Audio
            new FormatInfo(".mid","MThd", FormatType.Audio,"Musical Instrument Digital Interface"),

            //Text
            new FormatInfo(".txt", FormatType.Text,"Text file"),
            new FormatInfo(".log", FormatType.Text, "Log file"),
            new FormatInfo(".xml", FormatType.Text,"eXtensible Markup Language file"),
            new FormatInfo(".csv", FormatType.Text,"Comma Separated Values"),
            new FormatInfo(".inf", FormatType.Text, "info file"),
            new FormatInfo(".ini", FormatType.Text, "Configuration file"),

            //Roms & Iso
            new FormatInfo(".gba", new byte[]{46,0,0,234,36,255,174,81,105,154,162,33,61,132,130},0, FormatType.Rom, "GBA Rom", Nin_),
            new FormatInfo(".nes", new byte[]{78,69,83,26,1,1},0 , FormatType.Rom, "NES Rom", Nin_),
            new FormatInfo(".rvz", new byte[]{82,86,90,1,1},0 , FormatType.Rom, "Dolphin Iso", "Dolphin Team"),
            new FormatInfo(".WIA", new byte[]{87,73,65,1,1},0 , FormatType.Rom, "Wii ISO Archive","Wiimm"),
            new FormatInfo(".wad", new byte[]{32,73,115},3, FormatType.Rom, "Wii"),
            new FormatInfo(".ciso", FormatType.Rom, "Compact ISO"),
            new FormatInfo(".iso", new byte[]{43,44,30,30,31},0, FormatType.Rom, "ISO-9660 table"),
            new FormatInfo(".WDF", FormatType.Rom, "Wii Disc Format","Wiimm"),
            new FormatInfo(".GCZ", FormatType.Rom, "GameCube Zip"),
            new FormatInfo(".WBFS", FormatType.Rom, "Wii Backup File System"),

            //else
            new FormatInfo(".htm", FormatType.Else, "Hypertext Markup"),
            new FormatInfo(".MAP", FormatType.Else, "Debugger infos"),
            new FormatInfo(".lua", FormatType.Skript, "Script"),
            new FormatInfo(".cpp", FormatType.Skript, "C++ Source code"),
            new FormatInfo(".h",FormatType.Text,"Header file"),
            #endregion

            //CRIWARE
            new FormatInfo(".cpk", "CPK ", FormatType.Archive, "Compact Archive", "CRIWARE"),
            new FormatInfo(".afs", "AFS", FormatType.Archive, "File Archive", "CRIWARE"),
            new FormatInfo(".adx", FormatType.Audio, "CRI Audio", "CRIWARE"),
            new FormatInfo(".aix", FormatType.Audio, "CRI Audio Archive", "CRIWARE"),
            new FormatInfo(".sfd", new byte[]{1,186,33},2 , FormatType.Video, "SofDec Video", "CRIWARE"),

            //UbiSoft
            new FormatInfo(".bf","BUG", FormatType.Archive, "UbiSoft"),
            new FormatInfo(".bf","BIG", FormatType.Archive, "UbiSoft"),
            new FormatInfo(".waa","RIFF", FormatType.Audio, "UbiSoft"),

            //Namco Bandai
            new FormatInfo(".dkz", "DKZF", FormatType.Archive, "Donkey Konga"),
            new FormatInfo(".nut", "NUTC", FormatType.Texture, "Namco Universal Texture", "Namco"),

            //SEGA
            new FormatInfo(".one", FormatType.Archive, "Sonic Archive", "SEGA"),
            new FormatInfo(".XVRs", FormatType.Texture, "SonicRiders Texture", "SEGA"), //https://github.com/Sewer56/SonicRiders.Index/tree/master/Source

            //Imageepoch
            new FormatInfo(".vol", "RTDP", FormatType.Archive, "Arc Rise Archive", "Imageepoch"),
            new FormatInfo(".wtm", "WTMD", FormatType.Texture, "Arc Rise Texture", "Imageepoch"),

            //Treasure
            new FormatInfo(".RSC", FormatType.Archive, "Wario World archive", "Treasure"),
            new FormatInfo(".arc", "NARC", FormatType.Archive, "Sin and Punishment archive", "Treasure"),
            new FormatInfo(".gvm", "GVMH", FormatType.Archive, "IKARUGA Stage archive", "Treasure"),
            new FormatInfo(".gvr", "GCIX", FormatType.Texture, "IKARUGA texture", "Treasure"),//https://gitlab.com/dashgl/ikaruga/-/snippets/2054452
            new FormatInfo(".gvrt", "GVRT", FormatType.Texture, "IKARUGA texture", "Treasure"),
            new FormatInfo(".nj", "NJTL", FormatType.Model, "Ninja Model", "Treasure"),//https://gitlab.com/dashgl/ikaruga/-/snippets/2046285

            #region Mixed
            //mix Archives
            new FormatInfo(".clz", "CLZ", FormatType.Archive, "Harvest Moon compressed", "Victor Interactive"),
            new FormatInfo(".fpk", FormatType.Archive, "compressed"),
            new FormatInfo(".dir", FormatType.Else, "Archive Info"),
            new FormatInfo(".pk", FormatType.Archive), //https://github.com/RGBA-CRT/LSPK-Extracter
            new FormatInfo(".apf", FormatType.Archive, "Ganbarion"),
            new FormatInfo(".aar","ALAR", FormatType.Archive, "Pandoras Tower"),
            new FormatInfo(".dat","FREB", FormatType.Archive, "Rune Factory"),
            new FormatInfo(".pos","POSD", FormatType.Else, "Rune Factory FREB Archive Info"),
            new FormatInfo(".fsys","FSYS", FormatType.Archive, "Pokemon"), //https://projectpokemon.org/home/tutorials/rom/stars-pok%C3%A9mon-colosseum-and-xd-hacking-tutorial/part-1-file-decompression-and-recompression-r5/
            new FormatInfo(".asr","AsuraZlb", FormatType.Archive, "Rebellion"),
            new FormatInfo(".dat","FBTI0001", FormatType.Archive, "Rune Factory"),
            new FormatInfo(".bin","NLCM", FormatType.Else, "Rune Factory Archive Info"),
            new FormatInfo(".ftx", "FCMP", FormatType.Archive, "MURAMASA"),// compressed MURAMASA: THE DEMON BLADE |.ftx|FCMP FTEX||.mbs|FCMP FMBS||.nms|FCMP NMSB||.nsb|FCMP NSBD|Skript Data||.esb|FCMP EMBP||.abf|FCMP MLIB|
            new FormatInfo(".dict", new byte[]{169,243,36,88,6,1},0, FormatType.Archive),
            new FormatInfo(".dat", "AKLZ~?Qd=ÌÌÍ", FormatType.Archive,"Skies of Arcadia Legends"),
            
            //Audio
            new FormatInfo(".mul", FormatType.Audio),
            new FormatInfo(".pkb", "mca", FormatType.Audio, "Archive?"),
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
            new FormatInfo(".chd","CHD", FormatType.Else),
            //Video
            new FormatInfo(".dat", "MOC5", FormatType.Video,"Mobiclip"),
            new FormatInfo(".mds", "MDSV", FormatType.Video,"zack and wiki Video"),
            new FormatInfo(".bik", "BIKi", FormatType.Video,"Bink","Epic Game"),
            new FormatInfo(".h4m", "HVQM4 1.3" , FormatType.Video,"","Hudson Soft"),
            new FormatInfo(".h4m", "HVQM4 1.4" , FormatType.Video,"","Hudson Soft"),
            new FormatInfo(".h4m", "HVQM4 1.5", FormatType.Video,"","Hudson Soft"),

            //Font
            new FormatInfo(".aft", "ALFT", FormatType.Font),
            new FormatInfo(".aig",  "ALIG", FormatType.Font),
            new FormatInfo(".bfn", "FONTbfn1", FormatType.Font),
            new FormatInfo(".pkb",  "RFNA", FormatType.Font),
            //Model
            new FormatInfo(".CMDL", FormatType.Model),
            new FormatInfo(".MREA", FormatType.Model, "Area"),
            new FormatInfo(".fpc", FormatType.Model, "pac file container"),
            //else
            new FormatInfo(".t",FormatType.Text),
            new FormatInfo(".asrBE","Asura   TXTH", FormatType.Text, "Rebellion"),
            new FormatInfo(".bas", FormatType.Animation, "Sound Animation"),
            new FormatInfo(".blight", "LGHT", FormatType.Effect, "Light"),
            new FormatInfo(".bfog", "FOGM", FormatType.Else, "Fog"),
            new FormatInfo(".cmd", "CAM ", FormatType.Else, "Camera data"),
            new FormatInfo(".cam", FormatType.Else, "Camera data"),
            new FormatInfo(".bin", "BTGN", FormatType.Else, "Materials"),
            new FormatInfo(".pac", "NPAC", FormatType.Else, "Star Fox Assault"),
            new FormatInfo(".blmap", "LMAP", FormatType.Else, "Light Map"),
            new FormatInfo(".idb", "looc", FormatType.Else, "Debugger infos"),
            new FormatInfo(".zzz", FormatType.Else, "place holder"),
            new FormatInfo(".pkb", "SB  ", FormatType.Else, "Skript"),
            new FormatInfo(".efc", new byte[]{114,117,110,108,101,110,103,116,104,32,99,111,109,112,46}, 0, FormatType.Unknown),
            #endregion
        };
    }
}
