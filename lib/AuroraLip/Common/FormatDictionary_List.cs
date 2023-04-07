using AuroraLib.Archives.Formats;
using AuroraLib.Compression.Formats;
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
            new FormatInfo(".arc", "RARC", FormatType.Archive,comp_, Nin_),
            new FormatInfo(".arc", "Uª8-", FormatType.Archive, "U8 Archive", Nin_),
            new FormatInfo(".szs", "Yaz0", FormatType.Archive, comp_, Nin_),
            new FormatInfo(".szs", "Yaz1", FormatType.Archive, comp_, Nin_),
            new FormatInfo(".szp", "Yay0", FormatType.Archive, comp_, Nin_),
            new FormatInfo(".brres","bres", FormatType.Archive, "Wii Resources Archive", Nin_),
            new FormatInfo(".mod", FormatType.Archive, "Dolphin 1 Model Archive", Nin_){ Class = typeof(MOD), IsMatch = MOD.Matcher },
            new FormatInfo(".mdl", new byte[]{4, 180, 0,0}, 0, FormatType.Texture, "Luigi's mansion Model", Nin_){ Class = typeof(MDL_LM), IsMatch = MDL_LM.Matcher },
            new FormatInfo(".bin", new byte[]{2}, 0, FormatType.Texture, "Luigi's mansion Binary Model", Nin_){ IsMatch = BIN_LM_Matcher},

            //Nintendo Textures
            new FormatInfo(".breft","REFT", FormatType.Texture, "Wii Effect Texture", Nin_),
            new FormatInfo(".TPL", new byte[]{0,32,175,48},0, FormatType.Texture, "Texture Palette Library", Nin_){ Class = typeof(TPL), IsMatch = TPL.Matcher },
            new FormatInfo(".TPL", FormatType.Texture, "Texture Palette Library v0", Nin_){ Class = typeof(TPL_0), IsMatch = TPL_0.Matcher },
            new FormatInfo(".txe", FormatType.Texture, "Dolphin 1 Texture", Nin_){ Class = typeof(TXE), IsMatch = TXE.Matcher },
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
            new FormatInfo(".bmt", "J3D2bmt3", FormatType.Else, "", Nin_),
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
            new FormatInfo(".brlan", "RLAN", FormatType.Animation, "Wii layout Animation", Nin_),
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
            new FormatInfo(".canm","ANDO", FormatType.Parameter, "JCameraAnimation", Nin_),
            //new FormatInfo(".brmdl", FormatType.Model, "Wii Model Display Lists", Nin_),
            #endregion

            #region Second party developer
            //Retro Studios
            new FormatInfo(".PAK", FormatType.Archive, "Retro Archive", Retro_){ Class = typeof(PAK_Retro), IsMatch = PAK_Retro.Matcher }, //GC https://www.metroid2002.com/retromodding/wiki/PAK_(Metroid_Prime)#Header
            new FormatInfo(".PAK", FormatType.Archive, "Retro Wii Archive", Retro_){ Class = typeof(PAK_RetroWii), IsMatch = PAK_RetroWii.Matcher }, //Wii https://www.metroid2002.com/retfromodding/wiki/PAK_(Metroid_Prime_3)#Header
            new FormatInfo(".TXTR", FormatType.Texture, "Retro Texture", Retro_){ Class = typeof(TXTR) },
            new FormatInfo(".AGSC", FormatType.Audio, "Retro sound effect", Retro_), // https://www.metroid2002.com/retromodding/wiki/AGSC_(File_Format)
            new FormatInfo(".CSMP", FormatType.Audio, "Retro Audio", Retro_), // https://www.metroid2002.com/retromodding/wiki/CSMP_(File_Format)
            new FormatInfo(".PART", FormatType.Effect, "Retro Particle System", Retro_),
            new FormatInfo(".WPSC", FormatType.Effect, "Retro Swoosh Particle System", Retro_),
            new FormatInfo(".DCLN", FormatType.Collision, "Retro Dynamic Collision", Retro_),
            new FormatInfo(".RULE", "RULE", FormatType.Parameter, "Retro Studios Rule Set", Retro_),
            new FormatInfo(".SCAN", "SCAN", FormatType.Else, "Metroid Scan", Retro_),
            new FormatInfo(".FONT", "FONT", FormatType.Font, "Retro Font", Retro_),
            new FormatInfo(".MLVL", "Þ¯º¾", FormatType.Font, "Retro World Data", Retro_),
            new FormatInfo(".ANIM", FormatType.Animation, "Retro animation", Retro_),
            new FormatInfo(".CSKR", FormatType.Parameter, "Retro Skin Rules", Retro_),
            new FormatInfo(".STRG", FormatType.Text, "Retro String Table", Retro_),

            //Next Level Games
            new FormatInfo(".rlt","PTLG", FormatType.Texture, "Strikers Texture","Next Level Games"),
            new FormatInfo(".sanim", FormatType.Animation, "Striker Skeleton Animation","Next Level Games"),
            new FormatInfo(".nlxwb", FormatType.Audio, "Next Level Wave","Next Level Games"),
            
            //HAL Laboratory & Sora Ltd.
            new FormatInfo(".pac",new byte[]{65,82,67,0},0, FormatType.Archive, "Brawl Archive"){ Class = typeof(ARC0)},
            //new FormatInfo(".dat", FormatType.Archive, "HAL Archive", "HAL Laboratory"), // https://wiki.tockdom.com/wiki/HAL_DAT_(File_Format)
            new FormatInfo(".msbin", FormatType.Text,"Brawl Text"),

            //Intelligent Systems
            new FormatInfo(".pak","pack", FormatType.Archive, "Fire Emblem Archive", "Intelligent Systems"),

            //Genius Sonority
            new FormatInfo(".fsys","FSYS", FormatType.Archive, "Pokemon Archive","Genius Sonority"), //https://projectpokemon.org/home/tutorials/rom/stars-pok%C3%A9mon-colosseum-and-xd-hacking-tutorial/part-1-file-decompression-and-recompression-r5/
            new FormatInfo(".GTX", FormatType.Texture, "Pokemon Texture","Genius Sonority"){ Class = typeof(GTX), IsMatch = GTX.Matcher },
            new FormatInfo(".GSscene", FormatType.Texture, "Genius Sonority Scene File (based on sysdolphin)", "Genius Sonority"){ Class = typeof(GSScene) },
            new FormatInfo(".FLOORDAT", FormatType.Texture, "Genius Sonority Floor Model", "Genius Sonority"){ Class = typeof(GSScene) },
            new FormatInfo(".MODELDAT", FormatType.Texture, "Genius Sonority Character Model", "Genius Sonority"){ Class = typeof(GSScene) },
            new FormatInfo(".PKX", FormatType.Archive, "Genius Sonority Pokémons", "Genius Sonority"){ Class = typeof(PKX) },
            new FormatInfo(".GPT", "GPT0", FormatType.Unknown, "GS Particle v0", "Genius Sonority"),
            new FormatInfo(".GPT", "GPT1", FormatType.Unknown, "GS Particle v1", "Genius Sonority"),
            new FormatInfo(".GPT", new byte[]{ 0x01, 0xF0, 0x5, 0xDA, 0x00, 0x03, 0x00, 0x02 }, 0, FormatType.Unknown, "GS Particle (unknown)", "Genius Sonority"),
            #endregion

            #region Common
            //Common Archives
            new FormatInfo(".rar","Rar!", FormatType.Archive, "Roshal Archive","win.rar GmbH") { Class = typeof(SevenZip)},
            new FormatInfo(".zip","PK", FormatType.Archive, "zip Archive","PKWARE, Inc") { Class = typeof(SevenZip)},
            new FormatInfo(".7z",new byte[]{55, 122, 188, 175, 39, 28},0, FormatType.Archive, "7-Zip archive","Igor Pavlov") { Class = typeof(SevenZip)},
            new FormatInfo(".tar","ustar", FormatType.Archive, "Unix Standard TAR","Unix") { Class = typeof(SevenZip)},
            new FormatInfo(".deb","!<arch>", FormatType.Archive, "Debian pack","The Debian Projec") { Class = typeof(SevenZip)},
            new FormatInfo(".dmg",new byte[]{120, 1, 115, 13, 98, 98, 96},0, FormatType.Archive, "Apple Disk Image","Apple Inc.") { Class = typeof(SevenZip)},
            new FormatInfo(".rpm",new byte[]{237, 171, 238, 219},0, FormatType.Archive, "Red Hat Pack","Red Hat") { Class = typeof(SevenZip)},
            new FormatInfo(".xar","xar!", FormatType.Archive, "eXtensible ARchive format","OpenDarwin project") { Class = typeof(SevenZip)},
            new FormatInfo(".bz2","BZ", FormatType.Archive, "BZip2 compression","Julian Seward") { Class = typeof(SevenZip) , IsMatch = BZip_Matcher},
            new FormatInfo(".lzh","-lh", FormatType.Archive, "LHA compression","Haruyasu Yoshizaki") { Class = typeof(SevenZip) , IsMatch = LZH_Matcher},
            new FormatInfo(".gz",new byte[]{31,139},0, FormatType.Archive, "GNU zip","GNU Project"){ Class = typeof(GZip)},
            //new FormatInfo(".arj",new byte[]{96, 234},0, FormatType.Archive, "Archived by Robert Jung","Robert K. Jung"),
            new FormatInfo(".LZ", "LZSS", FormatType.Archive, "Lempel–Ziv–SS", "Storer–Szymanski"),
            new FormatInfo(".LZ", "LzS", FormatType.Archive, "Lempel-Ziv-Stac", "Stac Electronics"),
            new FormatInfo(".Lz00", "LZ00", FormatType.Archive, "Lempel-Ziv 00 "+comp_),
            new FormatInfo(".Lz01", "LZ01", FormatType.Archive, "Lempel-Ziv 01 "+comp_),
            new FormatInfo(".lz77","LZ77", FormatType.Archive, "Lempel-Ziv 77 Wii"),
            new FormatInfo(".Comp","COMP", FormatType.Archive, comp_),
            new FormatInfo(".CNX","CNX", FormatType.Archive, comp_),
            new FormatInfo(".CXLZ","CXLZ", FormatType.Archive, comp_),
            new FormatInfo(".LZ", FormatType.Archive, "Lempel-Ziv " + comp_),
            new FormatInfo(".zlib", FormatType.Archive, comp_) {Class = typeof(ZLib), IsMatch = ZLib.Matcher},
            new FormatInfo(".ZLB","ZLB", FormatType.Archive, comp_),
            new FormatInfo(".tar","KIJ=H", FormatType.Archive, "tape archive"),

            //Common Textures
            new FormatInfo(".PNG", new byte[]{137,80,78,71,13},0, FormatType.Texture, "Portable Network Graphics"),
            new FormatInfo(".Gif", "GIF89", FormatType.Texture, "Graphics Interchange Format"),
            new FormatInfo(".Jpg", new byte[]{255,216,255,224},0, FormatType.Texture, "Joint Photographic Group"),
            new FormatInfo(".tga", FormatType.Texture, "Truevision Graphic Advanced","Truevision"),

            //Microsoft
            new FormatInfo(".cab","MSCF", FormatType.Archive, "Cabinet Archive", "Microsoft") { Class = typeof(SevenZip)},
            new FormatInfo(".vhd","conectix", FormatType.Archive, "Virtual Hard Disk","Microsoft") { Class = typeof(SevenZip)},
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
            new FormatInfo(".nes", new byte[]{78,69,83,26,1,1},0 , FormatType.Rom, "Rom", Nin_),
            new FormatInfo(".rvz", new byte[]{82,86,90,1,1},0 , FormatType.Rom, "Dolphin Iso", "Dolphin Team"),
            new FormatInfo(".WIA", new byte[]{87,73,65,1,1},0 , FormatType.Rom, "Wii ISO Archive","Wiimm"),
            new FormatInfo(".wad", FormatType.Rom, "Wii WAD",Nin_){ Class = typeof(WAD), IsMatch = WAD.Matcher},
            new FormatInfo(".ciso", FormatType.Rom, "Compact ISO"),
            new FormatInfo(".iso", FormatType.Rom, "Gamecube Mini Disc Image",Nin_){ Class = typeof(GCDisk), IsMatch = GCDisk.Matcher},
            new FormatInfo(".iso", FormatType.Rom, "Wii Disc Image",Nin_){ Class = typeof(WiiDisk), IsMatch = WiiDisk.Matcher},
            new FormatInfo(".iso", "CD001", FormatType.Rom, "ISO-9660 table"),
            new FormatInfo(".WDF", FormatType.Rom, "Wii Disc Format","Wiimm"),
            new FormatInfo(".GCZ", FormatType.Rom, "GameCube Zip"),
            new FormatInfo(".wbfs", FormatType.Rom, "Wii Backup File System"),

            //else
            new FormatInfo(".htm", FormatType.Else, "Hypertext Markup"),
            new FormatInfo(".MAP", FormatType.Else, "Debugger infos"),
            new FormatInfo(".lua", FormatType.Skript, "Script"),
            new FormatInfo(".cpp", FormatType.Skript, "C++ Source code"),
            new FormatInfo(".h",FormatType.Text,"Header file"),
            #endregion
            
            #region Mixed
            //CRIWARE
            new FormatInfo(".cpk", "CPK ", FormatType.Archive, "Compact Archive", "CRIWARE"),
            new FormatInfo(".CCRI", "CRILAYLA" , FormatType.Archive, "Compact Compressed", "CRIWARE"),
            new FormatInfo(".afs", "AFS", FormatType.Archive, "File Archive", "CRIWARE"),
            new FormatInfo(".adx", "€", FormatType.Audio, "CRI Audio", "CRIWARE"){ IsMatch = ADX_Matcher},
            new FormatInfo(".aix", FormatType.Audio, "CRI Audio Archive", "CRIWARE"),
            new FormatInfo(".sfd", new byte[]{1,186,33},2 , FormatType.Video, "SofDec Video", "CRIWARE"),

            //UbiSoft
            new FormatInfo(".bf","BUG", FormatType.Archive, "UbiSoft Archive"),
            new FormatInfo(".bf","BIG", FormatType.Archive, "UbiSoft Archive"),
            new FormatInfo(".waa","RIFF", FormatType.Audio, "UbiSoft Audio"),

            //Namco Bandai
            new FormatInfo(".dkz", "DKZF", FormatType.Archive, "Donkey Konga"),
            new FormatInfo(".olk", "olnk".ToByte(),4, FormatType.Archive, "Archive", "Namco"), //https://forum.xentax.com/viewtopic.php?t=22500
            new FormatInfo(".nut", "NUTC", FormatType.Texture, "Namco Universal Texture", "Namco"),

            //SEGA
            new FormatInfo(".one", FormatType.Archive, "Sonic Storybook Series Archive", "SEGA") { Class = typeof(ONE_SB), IsMatch = ONE_SB.Matcher},
            new FormatInfo(".one","one.", FormatType.Archive, "Sonic Unleashed Archive", "SEGA"),
            new FormatInfo(".one", FormatType.Archive, "Sonic Archive", "SEGA"),
            new FormatInfo(".TXD", "TXAG", FormatType.Archive, "Sonic Storybook Texture Archive", "SEGA"),
            new FormatInfo(".gvm", "GVMH", FormatType.Archive, "SEGA Texture archive", "SEGA"),
            new FormatInfo(".gvr", "GBIX", FormatType.Texture, "VR Texture", "SEGA"),
            new FormatInfo(".gvr", "GCIX", FormatType.Texture, "VR Texture", "SEGA"),
            new FormatInfo(".gvrt","GVRT", FormatType.Texture, "VR Texture", "SEGA"),
            new FormatInfo("", new byte[]{128,0,0,1,0},0, FormatType.Archive, "Sonic Riders lzss", "SEGA"), //https://github.com/romhack/sonic_riders_lzss
            new FormatInfo(".rvm","CVMH", FormatType.Archive, "Sonic Riders Archive", "SEGA"),
            new FormatInfo(".XVRs", FormatType.Texture, "Sonic Riders Texture", "SEGA"), //https://github.com/Sewer56/SonicRiders.Index/tree/master/Source

            //Imageepoch
            new FormatInfo(".vol", "RTDP", FormatType.Archive, "Arc Rise Archive", "Imageepoch"),
            new FormatInfo(".wtm", "WTMD", FormatType.Texture, "Arc Rise Texture", "Imageepoch"),

            //Natsume
            new FormatInfo(".pBin", "pBin", FormatType.Archive, "Harvest Moon Archive", "Natsume"),
            new FormatInfo(".tex", FormatType.Texture, "Harvest Moon Texture", "Natsume"){ Class = typeof(FIPAFTEX), IsMatch = FIPAFTEX.Matcher },

            //Neverland
            new FormatInfo(".bin", "FBTI", FormatType.Archive, "Rune Factory Archive", "Neverland"),
            new FormatInfo(".bin", "NLCM", FormatType.Archive, "Rune Factory Archive Header", "Neverland"),
            new FormatInfo(".hvt", "HXTB", FormatType.Texture, "Rune Factory Texture", "Neverland"),

            //Square Enix
            new FormatInfo(".pos","POSD", FormatType.Archive, "Crystal Bearers Archive Header","Square Enix"),
            new FormatInfo(".FREB","FREB", FormatType.Archive, "Crystal Bearers Archive", "Square Enix"),
            new FormatInfo(".MPD", new byte[]{(byte)'M', (byte)'P', (byte)'D',0}, 0, FormatType.Unknown, "Crystal Bearers data", "Square Enix"),

            //Tecmo & Grasshopper Manufacture
            new FormatInfo(".RSL","RMHG", FormatType.Archive, "Fatal Frame Archive", "Tecmo"),
            new FormatInfo(".bin","GCT0", FormatType.Texture, "Fatal Frame Texture", "Tecmo"),
            new FormatInfo(".bin","CGMG", FormatType.Model, "Fatal Frame Model", "Tecmo"),

            //Treasure
            new FormatInfo(".RSC", FormatType.Archive, "Wario World archive", "Treasure"),
            new FormatInfo(".arc", "NARC", FormatType.Archive, "Sin and Punishment archive", "Treasure"),
            new FormatInfo(".nj", "NJTL", FormatType.Model, "Ninja Model", "Treasure"),//https://gitlab.com/dashgl/ikaruga/-/snippets/2046285

            //Tri-Crescendo
            new FormatInfo(".csl", "CSL ", FormatType.Archive, "Fragile Dreams "+comp_, "Tri-Crescendo"),

            //Vanillaware
            new FormatInfo(".fcmp", "FCMP", FormatType.Archive, "Muramasa "+comp_,"Vanillaware"),//Muramasa - The Demon Blade Decompressor http://www.jaytheham.com/code/
            new FormatInfo(".ftx", "FTEX", FormatType.Texture, "Muramasa Texture","Vanillaware"),
            new FormatInfo(".mbs", "FMBS", FormatType.Model, "Muramasa Model","Vanillaware"),
            new FormatInfo(".nms", "NMSB", FormatType.Text, "Muramasa Text","Vanillaware"),
            new FormatInfo(".nsb", "NSBD", FormatType.Skript, "Muramasa Skript","Vanillaware"),

            //Krome Studios
            new FormatInfo(".rkv", "RKV2", FormatType.Archive, "Star Wars Force Unleashed", "Krome Studios"),
            new FormatInfo(".tex", FormatType.Texture, "Star Wars Force Unleashed", "Krome Studios"){ Class = typeof(TEX_KS), IsMatch = TEX_KS.Matcher },

            //Red Fly Studios
            new FormatInfo("", FormatType.Texture, "Star Wars Force Unleashed 2", "Red Fly Studios"){ Class = typeof(TEX_RFS), IsMatch = TEX_RFS.Matcher },
            new FormatInfo(".POD", "POD5", FormatType.Archive, "Star Wars Force Unleashed 2", "Red Fly Studios"),

            //H.a.n.d.
            new FormatInfo(".fbc", FormatType.Archive, "Fables Chocobo archive", "H.a.n.d.") { Class= typeof(FBC)},

            //Cing
            new FormatInfo(".pac", "PCKG", FormatType.Archive, "Little King's Story Archive", "Cing"),  // also pcha

            //Victor Interactive
            new FormatInfo(".clz", "CLZ", FormatType.Archive, comp_, "Victor Interactive"),

            //Ganbarion
            new FormatInfo(".apf", FormatType.Archive,"One Piece FSM Archive", "Ganbarion"), //One Piece: Grand Adventure

            //Aqualead. use in Pandora's Tower
            new FormatInfo(".aar","ALAR", FormatType.Archive, "Archive", "Aqualead"), // https://zenhax.com/viewtopic.php?t=16613
            new FormatInfo(".act","ALCT", FormatType.Archive, "Container", "Aqualead"),
            new FormatInfo(".aar","ALLZ", FormatType.Archive, "AL LZSS Compressed", "Aqualead"), //https://github.com/Brolijah/Aqualead_LZSS
            new FormatInfo(".atx","ALTX", FormatType.Texture, "Texture", "Aqualead"),
            new FormatInfo(".aig","ALIG", FormatType.Texture, "Image", "Aqualead"),
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
            new FormatInfo(".GCLz","GCLZ", FormatType.Archive, comp_),

            //Hudson Soft
            new FormatInfo(".bin", FormatType.Archive, "Mario Party Archive", "Hudson Soft"){ Class = typeof(BIN_MP), IsMatch = BIN_MP.Matcher },
            new FormatInfo(".hsf", "HSFV037" , FormatType.Texture,"Mario Party Model","Hudson Soft"),
            new FormatInfo(".atb", FormatType.Texture,"Mario Party texture","Hudson Soft"){ Class = typeof(ATB), IsMatch = ATB.Matcher },
            new FormatInfo(".h4m", "HVQM4 1.3" , FormatType.Video,"","Hudson Soft"),
            new FormatInfo(".h4m", "HVQM4 1.4" , FormatType.Video,"","Hudson Soft"),
            new FormatInfo(".h4m", "HVQM4 1.5", FormatType.Video,"","Hudson Soft"),

            //mix
            //new FormatInfo(".cmpr","CMPR", FormatType.Archive, "compressed Data"),
            new FormatInfo(".ash","ASH0", FormatType.Archive, comp_), //https://github.com/trapexit/wiiqt/blob/master/WiiQt/ash.cpp
            new FormatInfo(".fpk", FormatType.Archive, comp_),
            new FormatInfo(".dir", FormatType.Else, "Archive Info"),
            new FormatInfo(".pk", FormatType.Archive), //https://github.com/RGBA-CRT/LSPK-Extracter
            new FormatInfo(".asr","AsuraZlb", FormatType.Archive, "Rebellion"),
            new FormatInfo(".dict", new byte[]{169,243,36,88,6,1},0, FormatType.Archive),
            new FormatInfo(".dat", "AKLZ~?Qd=ÌÌÍ", FormatType.Archive,"Skies of Arcadia Legends"),
            
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
            new FormatInfo(".chd","CHD", FormatType.Else),
            //Video
            new FormatInfo(".dat", "MOC5", FormatType.Video,"Mobiclip"),
            new FormatInfo(".mds", "MDSV", FormatType.Video,"zack and wiki Video"),
            new FormatInfo(".bik", "BIKi", FormatType.Video,"Bink","Epic Game"),

            //Font
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
            new FormatInfo(".pkb", "SB  ", FormatType.Skript, "Skript"),
            new FormatInfo(".efc", new byte[]{114,117,110,108,101,110,103,116,104,32,99,111,109,112,46}, 0, FormatType.Unknown),
            #endregion
        };

        //Is needed to detect ADX files reliably.
        public static bool ADX_Matcher(Stream stream, in string extension = "")
        {
            if (stream.ReadByte() != 128 || stream.ReadByte() != 0)
                return false;
            ushort CopyrightOffset = stream.ReadUInt16(Endian.Big);
            if (CopyrightOffset < 8) return false;
            stream.Seek(CopyrightOffset - 2, SeekOrigin.Begin);
            return stream.MatchString("(c)CRI");
        }

        public static bool BZip_Matcher(Stream stream, in string extension = "")
        {
            byte[] Data = stream.Read(4);
            return stream.Length > 4 && Data[0] == 66 && Data[1] == 90 && (Data[2] == 104 || Data[2] == 0) && Data[3] >= 49 && Data[3] <= 57;
        }

        public static bool LZH_Matcher(Stream stream, in string extension = "")
        {
            return stream.Length > 5 && stream.MatchString("-lz") && stream.ReadByte() > 32 && stream.MatchString("-");
        }

        public static bool BIN_LM_Matcher(Stream stream, in string extension = "")
        {
            return stream.ReadUInt8() == 2 && extension.ToLower() == ".bin" && stream.ReadString(2).Length == 2;
        }
    }
}
