using System.Collections.Generic;

namespace DolphinTextureExtraction_tool
{
    public static class Dictionary
    {

        static Dictionary()
        {
            foreach (FileTypInfo file in Master)
            {
                if (file.Extension.Contains("."))
                {
                    Header.Add(file.Header.MagicASKI, Extension[file.Extension.ToLower()]);
                    continue;
                }

                if (!Extension.ContainsKey("." + file.Extension.ToLower()))
                {
                    Extension.Add("." + file.Extension.ToLower(), file);
                }
                if (file.Header != null && file.Header.MagicASKI != "")
                {
                    Header.Add(file.Header.MagicASKI, file);
                }
            }
        }
        /// <summary>
        /// List of known files
        /// </summary>
        public static readonly FileTypInfo[] Master =
        {
            new FileTypInfo("arc",new Header("RARC"), FileTyp.Archive),
            new FileTypInfo("arc",new Header(new byte[]{85,170,56,45}), FileTyp.Archive),
            new FileTypInfo("szs",new Header("Yaz0"), FileTyp.Archive, "compressed"),
            new FileTypInfo("szp",new Header("Yay0"), FileTyp.Archive, "compressed"),
            new FileTypInfo("cpk",new Header("CPK "), FileTyp.Archive, "Middleware"),
            new FileTypInfo("clz",new Header("CLZ"), FileTyp.Archive, "Harvest Moon compressed"),

            new FileTypInfo("bdl",new Header("J3D2bdl4"), FileTyp.Archive, "display lists"),
            new FileTypInfo("bmd",new Header("J3D2bmd3"), FileTyp.Archive, "model"),
            //Textures
            new FileTypInfo("bti", FileTyp.Texture, "Image"),
            new FileTypInfo("TPL", FileTyp.Texture, "Palette Library"),
            new FileTypInfo("TPL",new Header(new byte[]{32,175,48},1), FileTyp.Texture, "Palette Library"),

            //Not supported
            //Archives
            new FileTypInfo("bdl",new Header("J3D2bdl3"), FileTyp.Archive, "display lists"),
            new FileTypInfo("bdl",new Header("J3D2bmd2"), FileTyp.Archive, "model"),
            new FileTypInfo("LZ", FileTyp.Archive, "LZ compressed"),
            new FileTypInfo("brres",new Header("bres"), FileTyp.Archive, "Wii Resource"),
            new FileTypInfo("aar",new Header("ALAR"), FileTyp.Archive, "Pandoras Tower"),
            new FileTypInfo("dat",new Header("FREB"), FileTyp.Archive, "Rune Factory"),
            new FileTypInfo("pos",new Header("POSD"), FileTyp.Else, "FREB Archive Info"),
            new FileTypInfo("arc",new Header("NARC"), FileTyp.Archive, "Sin and Punishment"),
            new FileTypInfo("PAK", FileTyp.Archive, "Retro Studios"), //GC https://www.metroid2002.com/retromodding/wiki/PAK_(Metroid_Prime)#Header Wii https://www.metroid2002.com/retfromodding/wiki/PAK_(Metroid_Prime_3)
            new FileTypInfo("dat", FileTyp.Archive, "HAL Laboratory"), // https://wiki.tockdom.com/wiki/HAL_DAT_(File_Format)
            new FileTypInfo("fsys",new Header("FSYS"), FileTyp.Archive, "Pokemon"), //https://projectpokemon.org/home/tutorials/rom/stars-pok%C3%A9mon-colosseum-and-xd-hacking-tutorial/part-1-file-decompression-and-recompression-r5/
            new FileTypInfo("bf",new Header("BUG"), FileTyp.Archive, "UbiSoft"),
            new FileTypInfo("bf",new Header("BIG"), FileTyp.Archive, "UbiSoft"),
            new FileTypInfo("asr",new Header("AsuraZlb"), FileTyp.Archive, "Rebellion"),
            new FileTypInfo("dkz",new Header("DKZF"), FileTyp.Archive, "Donkey Konga"),
            new FileTypInfo("one", FileTyp.Archive, "SEGA"),
            new FileTypInfo("RSC", FileTyp.Archive, "Wario World"),
            new FileTypInfo("",new Header("FCMP"), FileTyp.Archive, "MURAMASA"),// compressed MURAMASA: THE DEMON BLADE |.ftx|FCMP FTEX||.mbs|FCMP FMBS||.nms|FCMP NMSB||.nsb|FCMP NSBD|Skript Data||.esb|FCMP EMBP||.abf|FCMP MLIB|
            new FileTypInfo("afs",new Header("AFS"), FileTyp.Archive, "Ganbarion"),
            new FileTypInfo("",new Header(new byte[]{90,76,66}), FileTyp.Archive, "Starfox"),
            new FileTypInfo("dict",new Header(new byte[]{169,243,36,88,6,1}), FileTyp.Archive),
            new FileTypInfo("",new Header(new byte[]{78,80,65,67}), FileTyp.Archive),
            new FileTypInfo("",new Header(new byte[]{65,75,76,90,126,63,81,100,61,204,204,205}), FileTyp.Archive,"Skies of Arcadia Legends"),
            
            //Textures
            //new File("brtex","bres", FileTyp.Texture, "Wii"),
            new FileTypInfo("nut", new Header(new byte[]{78,85,84,67,128,2}), FileTyp.Texture ),
            new FileTypInfo("tga", FileTyp.Texture, "Truevision"),
            new FileTypInfo("rtex", FileTyp.Texture, "Wii XML"),
            new FileTypInfo("TXTR", FileTyp.Texture, "Retro Studios"), //http://www.metroid2002.com/retromodding/wiki/TXTR_(Metroid_Prime)
            new FileTypInfo("PNG", new Header(new byte[]{137,80,78,71,13}), FileTyp.Texture, "Portable Network Graphics"),
            new FileTypInfo("Jpg", new Header(new byte[]{255,216,255,224}), FileTyp.Texture, "JPEG"),
            new FileTypInfo("bmp", FileTyp.Texture, "bitmap"),
            new FileTypInfo(".bmp", new Header("BM8"), FileTyp.Texture),
            new FileTypInfo(".bmp", new Header("BMö"), FileTyp.Texture),
            new FileTypInfo("DDS", new Header("DDS |"), FileTyp.Texture, "Direct Draw Surface"),
            //Roms
            new FileTypInfo("gba", new Header(new byte[]{46,0,0,234,36,255,174,81,105,154,162,33,61,132,130}), FileTyp.Executable, "GBA Rom"),
            new FileTypInfo("nes", new Header(new byte[]{78,69,83,26,1,1}) , FileTyp.Executable, "Rom"),
            new FileTypInfo("rvz", new Header(new byte[]{82,86,90,1,1}) , FileTyp.Executable, "Rom"),
            new FileTypInfo("wia", new Header(new byte[]{87,73,65,1,1}) , FileTyp.Executable, "Rom"),
            new FileTypInfo("wad", new Header(new byte[]{32,73,115},3) , FileTyp.Executable, "Wii"),
            new FileTypInfo("", new Header(new byte[]{174,15,56,162}) , FileTyp.Executable ),
            //Executable
            new FileTypInfo("exe", new Header(new byte[]{77,90,144}) , FileTyp.Executable, "Windows"),
            new FileTypInfo("DOL", FileTyp.Executable, "GC Executable"),
            new FileTypInfo("REL", FileTyp.Executable, "Wii Executable LIB"),
            new FileTypInfo("elf", new Header(new byte[]{127,69,76,70,1,2,1 }) , FileTyp.Executable),
            //Audio
            new FileTypInfo("pkb", new Header("mca"), FileTyp.Audio, "Archive?"),
            new FileTypInfo("brsar",new Header("RSAR"), FileTyp.Audio, "Wii Archive"),
            new FileTypInfo("brstm", new Header("RSTM"), FileTyp.Audio, "Wii Stream"),
            new FileTypInfo("csb",new Header("@UTF"), FileTyp.Audio),
            new FileTypInfo("fsb",new Header("FSB3"), FileTyp.Audio),
            new FileTypInfo("ast",new Header("STRM"), FileTyp.Audio, "Stream"),
            new FileTypInfo("mid",new Header("MThd"), FileTyp.Audio),
            new FileTypInfo("aix",new Header("AIXF"), FileTyp.Audio),
            new FileTypInfo("",new Header(new byte[]{70,74,70}), FileTyp.Audio),
            new FileTypInfo("wt", FileTyp.Audio, "Wave"),
            new FileTypInfo("bwav", FileTyp.Audio, "Wave"),
            new FileTypInfo("wav",new Header("RIFX"), FileTyp.Audio, "Wave"),
            new FileTypInfo("dsp", FileTyp.Audio, "Nintendo ADPCM codec"),
            new FileTypInfo(".dsp",new Header(new byte[]{67,115,116,114}), FileTyp.Audio),
            new FileTypInfo("AGSC", FileTyp.Audio, "Retro Studios GC"), // https://www.metroid2002.com/retromodding/wiki/AGSC_(File_Format)
            new FileTypInfo("CSMP", FileTyp.Audio, "Retro Studios WII"), // https://www.metroid2002.com/retromodding/wiki/CSMP_(File_Format)
            new FileTypInfo("adx", FileTyp.Audio, "CRI"),
            new FileTypInfo("afc", FileTyp.Audio, "Stream"),
            new FileTypInfo("baa", FileTyp.Audio, "JAudio audio archive "),
            new FileTypInfo("aw", FileTyp.Audio, "JAudio wave archive"),
            new FileTypInfo("bms", FileTyp.Audio, "JAudio music sequence"),
            new FileTypInfo("bct", FileTyp.Audio, "Wii Remote sound info"),
            new FileTypInfo("csw", FileTyp.Audio, "Wii Remote sound effect"),
            new FileTypInfo("cit", FileTyp.Else, "Chord information table"),
            //Video
            new FileTypInfo("thp", new Header("THP"), FileTyp.Video),
            new FileTypInfo("dat", new Header("MOC5"), FileTyp.Video),
            new FileTypInfo("bik", new Header("BIKi"), FileTyp.Video),
            new FileTypInfo("h4m", new Header("HVQM4 1.3") , FileTyp.Video),
            new FileTypInfo("h4m", new Header("HVQM4 1.4") , FileTyp.Video),
            new FileTypInfo("h4m", new Header(new byte[]{72,86,81,77,52,32,49,46,53}) , FileTyp.Video),
            new FileTypInfo("sfd", new Header(new byte[]{1,186,33},2) , FileTyp.Video),
            //Text
            new FileTypInfo("t",FileTyp.Text),
            new FileTypInfo("h",FileTyp.Text,"File info"),
            new FileTypInfo("txt", FileTyp.Text),
            new FileTypInfo("log", FileTyp.Text),
            new FileTypInfo("xml", FileTyp.Text),
            new FileTypInfo("csv", FileTyp.Text),
            new FileTypInfo("inf", FileTyp.Text, "info"),
            new FileTypInfo("ini", FileTyp.Text, "Configuration"),
            new FileTypInfo("msbt",new Header("MsgStdBn"), FileTyp.Text, "LMS data"),
            new FileTypInfo("msbf",new Header("MsgFlwBn"), FileTyp.Text, "LMS flow data"),
            new FileTypInfo("msbp",new Header("MsgPrjBn"), FileTyp.Text, "LMS Prj data"),
            new FileTypInfo("bmg",new Header("MESGbmg1"), FileTyp.Text, "Binary message container"),
            new FileTypInfo("asrBE",new Header("Asura   TXTH"), FileTyp.Text, "Rebellion"),
            //Font
            new FileTypInfo("aft",new Header("ALFT"), FileTyp.Font),
            new FileTypInfo("aig", new Header("ALIG"), FileTyp.Font),
            new FileTypInfo("bfn",new Header("FONTbfn1"), FileTyp.Font),
            new FileTypInfo("brfnt",new Header("RFNT"), FileTyp.Font, "NW4R"),
            new FileTypInfo("pkb", new Header("RFNA"), FileTyp.Font),
            //2D Layout
            new FileTypInfo("blo", FileTyp.Layout, "UI"),
            new FileTypInfo(".blo", new Header("SCRNblo1"), FileTyp.Layout, "UI"),
            new FileTypInfo(".blo", new Header("SCRNblo2"), FileTyp.Layout, "UI"),
            new FileTypInfo("brlyt", new Header("RLYT"), FileTyp.Layout, "NW4R structure"),
            //Model
            new FileTypInfo("brmdl", FileTyp.Model),
            //Animation
            new FileTypInfo("bck",new Header("J3D1bck1"), FileTyp.Animation, "skeletal transformation"),
            new FileTypInfo("bck",new Header("J3D1bck3"), FileTyp.Animation, "skeletal transformation"),
            new FileTypInfo("bca",new Header("J3D1bca1"), FileTyp.Animation, "skeletal transformation"),
            new FileTypInfo("btp",new Header("J3D1btp1"), FileTyp.Animation, "Texture pattern"),
            new FileTypInfo("bpk",new Header("J3D1bpk1"), FileTyp.Animation, "color"),
            new FileTypInfo("bpa",new Header("J3D1bpa1"), FileTyp.Animation, "color"),
            new FileTypInfo("bva",new Header("J3D1bva1"), FileTyp.Animation, "visibility"),
            new FileTypInfo("blk",new Header("J3D1blk1"), FileTyp.Animation, "cluster"),
            new FileTypInfo("bla",new Header("J3D1bla1"), FileTyp.Animation, "cluster"),
            new FileTypInfo("bxk",new Header("J3D1bxk1"), FileTyp.Animation, "vertex color"),
            new FileTypInfo("bxa",new Header("J3D1bxa1"), FileTyp.Animation, "vertex color"),
            new FileTypInfo("btk",new Header("J3D1btk1"), FileTyp.Animation, "texture"),
            new FileTypInfo("brk",new Header("J3D1brk1"), FileTyp.Animation, "TEV color"),
            new FileTypInfo("bmt",new Header("J3D2bmt3"), FileTyp.Else),
            new FileTypInfo("bas", FileTyp.Animation, "Sound"),
            new FileTypInfo("brlan",new Header("RLAN"), FileTyp.Animation, "NW4R layout"),
            new FileTypInfo("branm", FileTyp.Animation),
            new FileTypInfo("brtsa", FileTyp.Animation, "Texture"),
            new FileTypInfo("brsha", FileTyp.Animation, "Vertex"),
            new FileTypInfo("brvia", FileTyp.Animation, "Visibility"),
            //Banner
            new FileTypInfo("bns", FileTyp.Else, "Banner"),
            new FileTypInfo("bnr",new Header(new byte[]{66,78,82,49}), FileTyp.Else, "Banner"),
            new FileTypInfo("bnr",new Header(new byte[]{66,78,82,50}), FileTyp.Else, "Banner"),
            new FileTypInfo("bnr",new Header(new byte[]{73,77,69,84},64), FileTyp.Else, "Banner"),
            new FileTypInfo("pac", FileTyp.Else, "Banner"),
            //else
            new FileTypInfo("jpc",new Header("JPAC2-10"), FileTyp.Else , "JParticle container"),
            new FileTypInfo("jpc",new Header("JPAC2-11"), FileTyp.Else , "JParticle container"),
            new FileTypInfo("jpa",new Header("JEFFjpa1"), FileTyp.Else , "JParticle"),
            new FileTypInfo("blight",new Header("LGHT"), FileTyp.Else, "Light"),
            new FileTypInfo("bfog",new Header("FOGM"), FileTyp.Else, "Fog"),
            new FileTypInfo("breff",new Header("REFF"), FileTyp.Else, "Effect"),
            new FileTypInfo("breft",new Header("REFT"), FileTyp.Else, "Effect"),
            new FileTypInfo("cmd",new Header("CAM "), FileTyp.Else, "Camera data"),
            new FileTypInfo("bin",new Header("BTGN"), FileTyp.Else, "Materials"),
            new FileTypInfo("tbl", FileTyp.Else, "JMap data"),
            new FileTypInfo("bcam", FileTyp.Else, "JMap camera data"),
            new FileTypInfo("brplt", FileTyp.Else, "Palette"),
            new FileTypInfo("brcha", FileTyp.Else, "Bone"),
            new FileTypInfo("brsca", FileTyp.Else, "Scene Settings"),
            new FileTypInfo("brtpa", FileTyp.Else, "Texture Pattern"),
            new FileTypInfo("lua", FileTyp.Else, "Script"),
            new FileTypInfo("pkb",new Header("SB  "), FileTyp.Else, "Skript"),
            new FileTypInfo("efc",new Header(new byte[]{114,117,110,108,101,110,103,116,104,32,99,111,109,112,46}), FileTyp.Unknown),
        };


        public static readonly Dictionary<string, FileTypInfo> Extension = new Dictionary<string, FileTypInfo>();

        public static readonly Dictionary<string, FileTypInfo> Header = new Dictionary<string, FileTypInfo>();
    }
}
