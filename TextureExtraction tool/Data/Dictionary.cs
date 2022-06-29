using System.Collections.Generic;

namespace DolphinTextureExtraction_tool
{
    public static class Dictionary
    {

        static Dictionary()
        {
            foreach (Filetyp file in Master)
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
        public static readonly Filetyp[] Master =
        {
            new Filetyp("arc",new Header("RARC"), FileTyp.Archive),
            new Filetyp("arc",new Header(new byte[]{85,170,56,45}), FileTyp.Archive),
            new Filetyp("szs",new Header("Yaz0"), FileTyp.Archive, "compressed"),
            new Filetyp("szp",new Header("Yay0"), FileTyp.Archive, "compressed"),
            new Filetyp("cpk",new Header("CPK "), FileTyp.Archive, "Middleware"),

            new Filetyp("bdl",new Header("J3D2bdl4"), FileTyp.Archive, "display lists"),
            new Filetyp("bmd",new Header("J3D2bmd3"), FileTyp.Archive, "model"),
            //Textures
            new Filetyp("bti", FileTyp.Texture, "Image"),
            new Filetyp("TPL", FileTyp.Texture, "Palette Library"),
            new Filetyp(".TPL",new Header(new byte[]{32,175,48},1), FileTyp.Texture),

            //Not supported
            //Archives
            new Filetyp("LZ", FileTyp.Archive, "LZ compressed"),
            new Filetyp("brres",new Header("bres"), FileTyp.Archive, "Wii Resource"),
            new Filetyp("aar",new Header("ALAR"), FileTyp.Archive, "Pandoras Tower"),
            new Filetyp("dat",new Header("FREB"), FileTyp.Archive, "Rune Factory"),
            new Filetyp("pos",new Header("POSD"), FileTyp.Else, "FREB Archive Info"),
            new Filetyp("clz",new Header("CLZ"), FileTyp.Archive, "Harvest Moon compressed"),
            new Filetyp("PAK", FileTyp.Archive, "Retro Studios"), //GC https://www.metroid2002.com/retromodding/wiki/PAK_(Metroid_Prime)#Header Wii https://www.metroid2002.com/retfromodding/wiki/PAK_(Metroid_Prime_3)
            new Filetyp("dat", FileTyp.Archive, "HAL Laboratory"), // https://wiki.tockdom.com/wiki/HAL_DAT_(File_Format)
            new Filetyp("fsys",new Header("FSYS"), FileTyp.Archive, "Pokemon"), //https://projectpokemon.org/home/tutorials/rom/stars-pok%C3%A9mon-colosseum-and-xd-hacking-tutorial/part-1-file-decompression-and-recompression-r5/
            new Filetyp("bf",new Header("BUG"), FileTyp.Archive, "UbiSoft"),
            new Filetyp("bf",new Header("BIG"), FileTyp.Archive, "UbiSoft"),
            new Filetyp("asr",new Header("AsuraZlb"), FileTyp.Archive, "Rebellion"),
            new Filetyp("dkz",new Header("DKZF"), FileTyp.Archive, "Donkey Konga"),
            new Filetyp("one", FileTyp.Archive, "SEGA"),
            new Filetyp("RSC", FileTyp.Archive, "Wario World"),
            new Filetyp("",new Header("FCMP"), FileTyp.Archive, "MURAMASA"),// compressed MURAMASA: THE DEMON BLADE |.ftx|FCMP FTEX||.mbs|FCMP FMBS||.nms|FCMP NMSB||.nsb|FCMP NSBD|Skript Data||.esb|FCMP EMBP||.abf|FCMP MLIB|
            new Filetyp("afs",new Header("AFS"), FileTyp.Archive, "Ganbarion"),
            new Filetyp("",new Header(new byte[]{90,76,66}), FileTyp.Archive, "Starfox"),
            new Filetyp("dict",new Header(new byte[]{169,243,36,88,6,1}), FileTyp.Archive),
            new Filetyp("",new Header(new byte[]{78,80,65,67}), FileTyp.Archive),
            new Filetyp("",new Header(new byte[]{65,75,76,90,126,63,81,100,61,204,204,205}), FileTyp.Archive,"Skies of Arcadia Legends"),
            
            //Textures
            //new File("brtex","bres", FileTyp.Texture, "Wii"),
            new Filetyp("nut", new Header(new byte[]{78,85,84,67,128,2}), FileTyp.Texture ),
            new Filetyp("tga", FileTyp.Texture, "Truevision"),
            new Filetyp("rtex", FileTyp.Texture, "Wii XML"),
            new Filetyp("TXTR", FileTyp.Texture, "Retro Studios"), //http://www.metroid2002.com/retromodding/wiki/TXTR_(Metroid_Prime)
            new Filetyp("PNG", new Header(new byte[]{137,80,78,71,13}), FileTyp.Texture ),
            new Filetyp("Jpg", new Header(new byte[]{255,216,255,22}), FileTyp.Texture ),
            new Filetyp("bmp", FileTyp.Texture, "bitmap"),
            new Filetyp(".bmp", new Header("BM8"), FileTyp.Texture),
            new Filetyp(".bmp", new Header("BMö"), FileTyp.Texture),
            new Filetyp("DDS", new Header("DDS |"), FileTyp.Texture, "Direct Draw Surface"),
            //Roms
            new Filetyp("gba", new Header(new byte[]{46,0,0,234,36,255,174,81,105,154,162,33,61,132,130}), FileTyp.Executable, "GBA Rom"),
            new Filetyp("nes", new Header(new byte[]{78,69,83,26,1,1}) , FileTyp.Executable, "Rom"),
            new Filetyp("rvz", new Header(new byte[]{82,86,90,1,1}) , FileTyp.Executable, "Rom"),
            new Filetyp("wia", new Header(new byte[]{87,73,65,1,1}) , FileTyp.Executable, "Rom"),
            new Filetyp("wad", new Header(new byte[]{32,73,115},3) , FileTyp.Executable, "Wii"),
            new Filetyp("", new Header(new byte[]{174,15,56,162}) , FileTyp.Executable ),
            //Executable
            new Filetyp("exe", new Header(new byte[]{77,90,144}) , FileTyp.Executable, "Windows"),
            new Filetyp("DOL", FileTyp.Executable, "GC Executable"),
            new Filetyp("REL", FileTyp.Executable, "Wii Executable LIB"),
            new Filetyp("elf", new Header(new byte[]{127,69,76,70,1,2,1 }) , FileTyp.Executable),
            //Audio
            new Filetyp("pkb", new Header("mca"), FileTyp.Audio, "Archive?"),
            new Filetyp("brsar",new Header("RSAR"), FileTyp.Audio, "Wii Archive"),
            new Filetyp("brstm", new Header("RSTM"), FileTyp.Audio, "Wii Stream"),
            new Filetyp("csb",new Header("@UTF"), FileTyp.Audio),
            new Filetyp("fsb",new Header("FSB3"), FileTyp.Audio),
            new Filetyp("ast",new Header("STRM"), FileTyp.Audio, "Stream"),
            new Filetyp("mid",new Header("MThd"), FileTyp.Audio),
            new Filetyp("aix",new Header("AIXF"), FileTyp.Audio),
            new Filetyp("",new Header(new byte[]{70,74,70}), FileTyp.Audio),
            new Filetyp("wt", FileTyp.Audio, "Wave"),
            new Filetyp("bwav", FileTyp.Audio, "Wave"),
            new Filetyp("wav",new Header("RIFX"), FileTyp.Audio, "Wave"),
            new Filetyp("dsp", FileTyp.Audio, "Nintendo ADPCM codec"),
            new Filetyp(".dsp",new Header(new byte[]{67,115,116,114}), FileTyp.Audio),
            new Filetyp("AGSC", FileTyp.Audio, "Retro Studios GC"), // https://www.metroid2002.com/retromodding/wiki/AGSC_(File_Format)
            new Filetyp("CSMP", FileTyp.Audio, "Retro Studios WII"), // https://www.metroid2002.com/retromodding/wiki/CSMP_(File_Format)
            new Filetyp("adx", FileTyp.Audio, "CRI"),
            new Filetyp("afc", FileTyp.Audio, "Stream"),
            new Filetyp("baa", FileTyp.Audio, "JAudio audio archive "),
            new Filetyp("aw", FileTyp.Audio, "JAudio wave archive"),
            new Filetyp("bms", FileTyp.Audio, "JAudio music sequence"),
            new Filetyp("bct", FileTyp.Audio, "Wii Remote sound info"),
            new Filetyp("csw", FileTyp.Audio, "Wii Remote sound effect"),
            new Filetyp("cit", FileTyp.Else, "Chord information table"),
            //Video
            new Filetyp("thp", new Header("THP"), FileTyp.Video),
            new Filetyp("dat", new Header("MOC5"), FileTyp.Video),
            new Filetyp("bik", new Header("BIKi"), FileTyp.Video),
            new Filetyp("h4m", new Header("HVQM4 1.3") , FileTyp.Video),
            new Filetyp("h4m", new Header("HVQM4 1.4") , FileTyp.Video),
            new Filetyp("h4m", new Header(new byte[]{72,86,81,77,52,32,49,46,53}) , FileTyp.Video),
            new Filetyp("sfd", new Header(new byte[]{1,186,33},2) , FileTyp.Video),
            //Text
            new Filetyp("t",FileTyp.Text),
            new Filetyp("h",FileTyp.Text,"File info"),
            new Filetyp("txt", FileTyp.Text),
            new Filetyp("log", FileTyp.Text),
            new Filetyp("xml", FileTyp.Text),
            new Filetyp("csv", FileTyp.Text),
            new Filetyp("inf", FileTyp.Text, "info"),
            new Filetyp("ini", FileTyp.Text, "Configuration"),
            new Filetyp("msbt",new Header("MsgStdBn"), FileTyp.Text, "LMS data"),
            new Filetyp("msbf",new Header("MsgFlwBn"), FileTyp.Text, "LMS flow data"),
            new Filetyp("bmg",new Header("MESGbmg1"), FileTyp.Text, "Binary message container"),
            new Filetyp("asrBE",new Header("Asura   TXTH"), FileTyp.Text, "Rebellion"),
            //Font
            new Filetyp("aft",new Header("ALFT"), FileTyp.Font),
            new Filetyp("aig", new Header("ALIG"), FileTyp.Font),
            new Filetyp("bfn",new Header("FONTbfn1"), FileTyp.Font),
            new Filetyp("brfnt",new Header("RFNT"), FileTyp.Font, "NW4R"),
            new Filetyp("pkb", new Header("RFNA"), FileTyp.Font),
            //2D Layout
            new Filetyp("blo", FileTyp.Layout, "UI"),
            new Filetyp(".blo", new Header("SCRNblo1"), FileTyp.Layout, "UI"),
            new Filetyp(".blo", new Header("SCRNblo2"), FileTyp.Layout, "UI"),
            new Filetyp("brlyt", new Header("RLYT"), FileTyp.Layout, "NW4R structure"),
            //Model
            new Filetyp("brmdl", FileTyp.Model),
            //Animation
            new Filetyp("bck",new Header("J3D1bck1"), FileTyp.Animation, "skeletal transformation"),
            new Filetyp("bck",new Header("J3D1bck3"), FileTyp.Animation, "skeletal transformation"),
            new Filetyp("bca",new Header("J3D1bca1"), FileTyp.Animation, "skeletal transformation"),
            new Filetyp("btp",new Header("J3D1btp1"), FileTyp.Animation, "Texture pattern"),
            new Filetyp("bpk",new Header("J3D1bpk1"), FileTyp.Animation, "color"),
            new Filetyp("bpa",new Header("J3D1bpa1"), FileTyp.Animation, "color"),
            new Filetyp("bva",new Header("J3D1bva1"), FileTyp.Animation, "visibility"),
            new Filetyp("blk",new Header("J3D1blk1"), FileTyp.Animation, "cluster"),
            new Filetyp("bla",new Header("J3D1bla1"), FileTyp.Animation, "cluster"),
            new Filetyp("bxk",new Header("J3D1bxk1"), FileTyp.Animation, "vertex color"),
            new Filetyp("bxa",new Header("J3D1bxa1"), FileTyp.Animation, "vertex color"),
            new Filetyp("btk",new Header("J3D1btk1"), FileTyp.Animation, "texture"),
            new Filetyp("brk",new Header("J3D1brk1"), FileTyp.Animation, "TEV color"),
            new Filetyp("bmt",new Header("J3D2bmt3"), FileTyp.Else),
            new Filetyp("bas", FileTyp.Animation, "Sound"),
            new Filetyp("brlan",new Header("RLAN"), FileTyp.Animation, "NW4R layout"),
            new Filetyp("branm", FileTyp.Animation),
            new Filetyp("brtsa", FileTyp.Animation, "Texture"),
            new Filetyp("brsha", FileTyp.Animation, "Vertex"),
            new Filetyp("brvia", FileTyp.Animation, "Visibility"),
            //Banner
            new Filetyp("bns", FileTyp.Else, "Banner"),
            new Filetyp("bnr",new Header(new byte[]{66,78,82,49}), FileTyp.Else, "Banner"),
            new Filetyp("bnr",new Header(new byte[]{66,78,82,50}), FileTyp.Else, "Banner"),
            new Filetyp("bnr",new Header(new byte[]{73,77,69,84},64), FileTyp.Else, "Banner"),
            new Filetyp("pac", FileTyp.Else, "Banner"),
            //else
            new Filetyp("jpc",new Header("JPAC2-10"), FileTyp.Else , "JParticle container"),
            new Filetyp("jpa",new Header("JEFFjpa1"), FileTyp.Else , "JParticle"),
            new Filetyp("blight",new Header("LGHT"), FileTyp.Else, "Light"),
            new Filetyp("bfog",new Header("FOGM"), FileTyp.Else, "Fog"),
            new Filetyp("breff",new Header("REFF"), FileTyp.Else, "Effect"),
            new Filetyp("breft",new Header("REFT"), FileTyp.Else, "Effect"),
            new Filetyp("cmd",new Header("CAM "), FileTyp.Else, "Camera data"),
            new Filetyp("bin",new Header("BTGN"), FileTyp.Else, "Materials"),
            new Filetyp("tbl", FileTyp.Else, "JMap data"),
            new Filetyp("bcam", FileTyp.Else, "JMap camera data"),
            new Filetyp("brplt", FileTyp.Else, "Palette"),
            new Filetyp("brcha", FileTyp.Else, "Bone"),
            new Filetyp("brsca", FileTyp.Else, "Scene Settings"),
            new Filetyp("brtpa", FileTyp.Else, "Texture Pattern"),
            new Filetyp("lua", FileTyp.Else, "Script"),
            new Filetyp("pkb",new Header("SB  "), FileTyp.Else, "Skript"),
            new Filetyp("efc",new Header(new byte[]{114,117,110,108,101,110,103,116,104,32,99,111,109,112,46}), FileTyp.Unknown),
        };


        public static readonly Dictionary<string, Filetyp> Extension = new Dictionary<string, Filetyp>();

        public static readonly Dictionary<string, Filetyp> Header = new Dictionary<string, Filetyp>();
    }
}
