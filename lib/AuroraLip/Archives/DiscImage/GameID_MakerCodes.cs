﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AuroraLip.Archives.DiscImage
{
    public partial struct GameID
    {
        // comes from dolphin
        internal static readonly ReadOnlyDictionary<string, string> MakerCodes = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()
        {
            {"01", "Nintendo"},
            {"02", "Nintendo"},
            {"08", "Capcom"},
            {"0A", "Jaleco / Jaleco Entertainment"},
            {"0L", "Warashi"},
            {"0M", "Entertainment Software Publishing"},
            {"0Q", "IE Institute"},
            {"13", "Electronic Arts Japan"},
            {"18", "Hudson Soft / Hudson Entertainment"},
            {"1K", "Titus Software"},
            {"20", "DSI Games / ZOO Digital Publishing"},
            {"28", "Kemco Japan"},
            {"29", "SETA Corporation"},
            {"2K", "NEC Interchannel"},
            {"2L", "Agatsuma Entertainment"},
            {"2M", "Jorudan"},
            {"2N", "Smilesoft / Rocket Company"},
            {"2Q", "MediaKite"},
            {"36", "Codemasters"},
            {"41", "Ubisoft"},
            {"4F", "Eidos Interactive"},
            {"4Q", "Disney Interactive Studios / Buena Vista Games"},
            {"4Z", "Crave Entertainment / Red Wagon Games"},
            {"51", "Acclaim Entertainment"},
            {"52", "Activision"},
            {"54", "Take-Two Interactive / GameTek / Rockstar Games / Global Star Software"},
            {"5D", "Midway Games / Tradewest"},
            {"5G", "Majesco Entertainment"},
            {"5H", "3DO / Global Star Software"},
            {"5L", "NewKidCo"},
            {"5S", "Evolved Games / Xicat Interactive"},
            {"5V", "Agetec"},
            {"5Z", "Data Design / Conspiracy Entertainment"},
            {"60", "Titus Interactive / Titus Software"},
            {"64", "LucasArts"},
            {"68", "Bethesda Softworks / Mud Duck Productions / Vir2L Studios"},
            {"69", "Electronic Arts"},
            {"6E", "Sega"},
            {"6K", "UFO Interactive Games"},
            {"6L", "BAM! Entertainment"},
            {"6M", "System 3"},
            {"6N", "Midas Interactive Entertainment"},
            {"6S", "TDK Mediactive"},
            {"6U", "The Adventure Company / DreamCatcher Interactive"},
            {"6V", "JoWooD Entertainment"},
            {"6W", "Sega"},
            {"6X", "Wanadoo Edition"},
            {"6Z", "NDS Software"},
            {"70", "Atari (Infogrames)"},
            {"71", "Interplay Entertainment"},
            {"75", "SCi Games"},
            {"78", "THQ / Play THQ"},
            {"7D", "Sierra Entertainment / Vivendi Games / Universal Interactive Studios"},
            {"7F", "Kemco"},
            {"7G", "Rage Software"},
            {"7H", "Encore Software"},
            {"7J", "Zushi Games / ZOO Digital Publishing"},
            {"7K", "Kiddinx Entertainment"},
            {"7L", "Simon & Schuster Interactive"},
            {"7M", "Badland Games"},
            {"7N", "Empire Interactive / Xplosiv"},
            {"7S", "Rockstar Games"},
            {"7T", "Scholastic"},
            {"7U", "Ignition Entertainment"},
            {"82", "Namco"},
            {"8G", "NEC Interchannel"},
            {"8J", "Kadokawa Shoten"},
            {"8M", "CyberFront"},
            {"8N", "Success"},
            {"8P", "Sega"},
            {"91", "Chunsoft"},
            {"99", "Marvelous Entertainment / Victor Entertainment / Pack-In-Video / Rising Star Games"},
            {"9B", "Tecmo"},
            {"9G", "Take-Two Interactive / Global Star Software / Gotham Games / Gathering of Developers"},
            {"9S", "Brother International"},
            {"9Z", "Crunchyroll"},
            {"A4", "Konami"},
            {"A7", "Takara"},
            {"AF", "Namco Bandai Games"},
            {"AU", "Alternative Software"},
            {"AX", "Vivendi"},
            {"B0", "Acclaim Japan"},
            {"B2", "Bandai Games"},
            {"BB", "Sunsoft"},
            {"BL", "MTO"},
            {"BM", "XING"},
            {"BN", "Sunrise Interactive"},
            {"BP", "Global A Entertainment"},
            {"C0", "Taito"},
            {"C8", "Koei"},
            {"CM", "Konami Computer Entertainment Osaka"},
            {"CQ", "From Software"},
            {"D9", "Banpresto"},
            {"DA", "Tomy / Takara Tomy"},
            {"DQ", "Compile Heart / Idea Factory"},
            {"E5", "Epoch"},
            {"E6", "Game Arts"},
            {"E7", "Athena"},
            {"E8", "Asmik Ace Entertainment"},
            {"E9", "Natsume"},
            {"EB", "Atlus"},
            {"EL", "Spike"},
            {"EM", "Konami Computer Entertainment Tokyo"},
            {"EP", "Sting Entertainment"},
            {"ES", "Starfish-SD"},
            {"EY", "Vblank Entertainment"},
            {"FH", "Easy Interactive"},
            {"FJ", "Virtual Toys"},
            {"FK", "The Game Factory"},
            {"FP", "Mastiff"},
            {"FR", "Digital Tainment Pool"},
            {"FS", "XS Games"},
            {"G0", "Alpha Unit"},
            {"G2", "Yuke's"},
            {"G6", "SIMS"},
            {"G9", "D3 Publisher"},
            {"GA", "PIN Change"},
            {"GD", "Square Enix"},
            {"GE", "Kids Station"},
            {"GG", "O3 Entertainment"},
            {"GJ", "Detn8 Games"},
            {"GK", "Genki"},
            {"GL", "Gameloft / Ubisoft"},
            {"GM", "Gamecock Media Group"},
            {"GN", "Oxygen Games"},
            {"GR", "GSP"},
            {"GT", "505 Games"},
            {"GX", "Commodore"},
            {"GY", "The Game Factory"},
            {"GZ", "Gammick Entertainment"},
            {"H3", "Zen United"},
            {"H4", "SNK Playmore"},
            {"HA", "Nobilis"},
            {"HE", "Gust"},
            {"HF", "Level-5"},
            {"HG", "Graffiti Entertainment"},
            {"HH", "Focus Home Interactive"},
            {"HJ", "Genius Products"},
            {"HK", "D2C Games"},
            {"HL", "Frontier Developments"},
            {"HM", "HMH Interactive"},
            {"HN", "High Voltage Software"},
            {"HQ", "Abstraction Games"},
            {"HS", "Tru Blu"},
            {"HT", "Big Blue Bubble"},
            {"HU", "Ghostfire Games"},
            {"HW", "Incredible Technologies"},
            {"HY", "Reef Entertainment"},
            {"HZ", "Nordcurrent"},
            {"J8", "D4 Enterprise"},
            {"J9", "AQ Interactive"},
            {"JD", "SKONEC Entertainment"},
            {"JE", "E Frontier"},
            {"JF", "Arc System Works"},
            {"JG", "The Games Company"},
            {"JH", "City Interactive"},
            {"JJ", "Deep Silver"},
            {"JP", "redspotgames"},
            {"JR", "Engine Software"},
            {"JS", "Digital Leisure"},
            {"JT", "Empty Clip Studios"},
            {"JU", "Riverman Media"},
            {"JV", "JV Games"},
            {"JW", "BigBen Interactive"},
            {"JX", "Shin'en Multimedia"},
            {"JY", "Steel Penny Games"},
            {"JZ", "505 Games"},
            {"K2", "Coca-Cola (Japan) Company"},
            {"K3", "Yudo"},
            {"K6", "Nihon System"},
            {"KB", "Nippon Ichi Software"},
            {"KG", "Kando Games"},
            {"KH", "Joju Games"},
            {"KJ", "Studio Zan"},
            {"KK", "DK Games"},
            {"KL", "Abylight"},
            {"KM", "Deep Silver"},
            {"KN", "Gameshastra"},
            {"KP", "Purple Hills"},
            {"KQ", "Over the Top Games"},
            {"KR", "KREA Medie"},
            {"KT", "The Code Monkeys"},
            {"KW", "Semnat Studios"},
            {"KY", "Medaverse Studios"},
            {"L3", "G-Mode"},
            {"L8", "FujiSoft"},
            {"LB", "Tryfirst"},
            {"LD", "Studio Zan"},
            {"LF", "Kemco"},
            {"LG", "Black Bean Games"},
            {"LJ", "Legendo Entertainment"},
            {"LL", "HB Studios"},
            {"LN", "GameOn"},
            {"LP", "Left Field Productions"},
            {"LR", "Koch Media"},
            {"LT", "Legacy Interactive"},
            {"LU", "Lexis Numérique"},
            {"LW", "Grendel Games"},
            {"LY", "Icon Games / Super Icon"},
            {"M0", "Studio Zan"},
            {"M1", "Grand Prix Games"},
            {"M2", "HomeMedia"},
            {"M4", "Cybird"},
            {"M6", "Perpetuum"},
            {"MB", "Agenda"},
            {"MD", "Ateam"},
            {"ME", "Silver Star Japan"},
            {"MF", "Yamasa"},
            {"MH", "Mentor Interactive"},
            {"MJ", "Mumbo Jumbo"},
            {"ML", "DTP Young Entertainment"},
            {"MM", "Big John Games"},
            {"MN", "Mindscape"},
            {"MR", "Mindscape"},
            {"MS", "Milestone / UFO Interactive Games"},
            {"MT", "Blast! Entertainment"},
            {"MV", "Marvelous Entertainment"},
            {"MZ", "Mad Catz"},
            {"N0", "Exkee"},
            {"N4", "Zoom"},
            {"N7", "T&S"},
            {"N9", "Tera Box"},
            {"NA", "Tom Create"},
            {"NB", "HI Games & Publishing"},
            {"NE", "Kosaido"},
            {"NF", "Peakvox"},
            {"NG", "Nordic Games"},
            {"NH", "Gevo Entertainment"},
            {"NJ", "Enjoy Gaming"},
            {"NK", "Neko Entertainment"},
            {"NL", "Nordic Softsales"},
            {"NN", "Nnooo"},
            {"NP", "Nobilis"},
            {"NQ", "Namco Bandai Partners"},
            {"NR", "Destineer Publishing / Bold Games"},
            {"NS", "Nippon Ichi Software America"},
            {"NT", "Nocturnal Entertainment"},
            {"NV", "Nicalis"},
            {"NW", "Deep Fried Entertainment"},
            {"NX", "Barnstorm Games"},
            {"NY", "Nicalis"},
            {"P1", "Poisoft"},
            {"PH", "Playful Entertainment"},
            {"PK", "Knowledge Adventure"},
            {"PL", "Playlogic Entertainment"},
            {"PM", "Warner Bros. Interactive Entertainment"},
            {"PN", "P2 Games"},
            {"PQ", "PopCap Games"},
            {"PS", "Bplus"},
            {"PT", "Firemint"},
            {"PU", "Pub Company"},
            {"PV", "Pan Vision"},
            {"PY", "Playstos Entertainment"},
            {"PZ", "GameMill Publishing"},
            {"Q2", "Santa Entertainment"},
            {"Q3", "Asterizm"},
            {"Q4", "Hamster"},
            {"Q5", "Recom"},
            {"QA", "Miracle Kidz"},
            {"QC", "Kadokawa Shoten / Enterbrain"},
            {"QH", "Virtual Play Games"},
            {"QK", "MACHINE Studios"},
            {"QM", "Object Vision Software"},
            {"QQ", "Gamelion"},
            {"QR", "Lapland Studio"},
            {"QT", "CALARIS"},
            {"QU", "QubicGames"},
            {"QV", "Ludia"},
            {"QW", "Kaasa Solution"},
            {"QX", "Press Play"},
            {"QZ", "Hands-On Mobile"},
            {"RA", "Office Create"},
            {"RG", "Ronimo Games"},
            {"RH", "h2f Games"},
            {"RM", "Rondomedia"},
            {"RN", "Mastiff / N3V Games"},
            {"RQ", "GolemLabs & Zoozen"},
            {"RS", "Brash Entertainment"},
            {"RT", "RTL Enterprises"},
            {"RV", "bitComposer Games"},
            {"RW", "RealArcade"},
            {"RX", "Reflexive Entertainment"},
            {"RZ", "Akaoni Studio"},
            {"S5", "SouthPeak Games"},
            {"SH", "Sabarasa"},
            {"SJ", "Cosmonaut Games"},
            {"SP", "Blade Interactive Studios"},
            {"SQ", "Sonalysts"},
            {"SR", "SnapDragon Games"},
            {"SS", "Sanuk Games"},
            {"ST", "Stickmen Studios"},
            {"SU", "Slitherine"},
            {"SV", "SevenOne Intermedia"},
            {"SZ", "Storm City Games"},
            {"TH", "Kolkom"},
            {"TJ", "Broken Rules"},
            {"TL", "Telltale Games"},
            {"TR", "Tetris Online"},
            {"TS", "Triangle Studios"},
            {"TV", "Tivola"},
            {"TW", "Two Tribes"},
            {"TY", "Teyon"},
            {"UG", "Data Design Interactive / Popcorn Arcade / Metro 3D"},
            {"UH", "Intenium Console"},
            {"UJ", "Ghostlight"},
            {"UK", "iFun4all"},
            {"UN", "Chillingo"},
            {"UP", "EnjoyUp Games"},
            {"UR", "Sudden Games"},
            {"US", "USM"},
            {"UU", "Onteca"},
            {"UV", "Fugazo"},
            {"UW", "Coresoft"},
            {"VG", "Vogster Entertainment"},
            {"VK", "Sandlot Games"},
            {"VL", "Eko Software"},
            {"VN", "Valcon Games"},
            {"VP", "Virgin Play"},
            {"VS", "Korner Entertainment"},
            {"VT", "Microforum Games"},
            {"VU", "Double Jungle"},
            {"VV", "Pixonauts"},
            {"VX", "Frontline Studios"},
            {"VZ", "Little Orbit"},
            {"WD", "Amazon"},
            {"WG", "2D Boy"},
            {"WH", "NinjaBee"},
            {"WJ", "Studio Walljump"},
            {"WL", "Wired Productions"},
            {"WN", "tons of bits"},
            {"WP", "White Park Bay Software"},
            {"WQ", "Revistronic"},
            {"WR", "Warner Bros. Interactive Entertainment"},
            {"WS", "MonkeyPaw Games"},
            {"WW", "Slang Publishing"},
            {"WY", "WayForward Technologies"},
            {"WZ", "Wizarbox"},
            {"X0", "SDP Games"},
            {"X3", "CK Games"},
            {"X4", "Easy Interactive"},
            {"XB", "Hulu"},
            {"XG", "XGen Studios"},
            {"XJ", "XSEED Games"},
            {"XK", "Exkee"},
            {"XM", "DreamBox Games"},
            {"XN", "Netflix"},
            {"XS", "Aksys Games"},
            {"XT", "Funbox Media"},
            {"XU", "Shanblue Interactive"},
            {"XV", "Keystone Game Studio"},
            {"XW", "Lemon Games"},
            {"XY", "Gaijin Games"},
            {"Y1", "Tubby Games"},
            {"Y5", "Easy Interactive"},
            {"Y6", "Motiviti"},
            {"Y7", "The Learning Company"},
            {"Y9", "RadiationBurn"},
            {"YC", "NECA"},
            {"YD", "Infinite Dreams"},
            {"YF", "O2 Games"},
            {"YG", "Maximum Family Games"},
            {"YJ", "Frozen Codebase"},
            {"YK", "MAD Multimedia"},
            {"YN", "Game Factory"},
            {"YS", "Yullaby"},
            {"YT", "Corecell Technology"},
            {"YV", "KnapNok Games"},
            {"YX", "Selectsoft"},
            {"YY", "FDG Entertainment"},
            {"Z4", "Ntreev Soft"},
            {"Z5", "Shinsegae I&C"},
            {"ZA", "WBA Interactive"},
            {"ZG", "Zallag"},
            {"ZH", "Internal Engine"},
            {"ZJ", "Performance Designed Products"},
            {"ZK", "Anima Game Studio"},
            {"ZP", "Fishing Cactus"},
            {"ZS", "Zinkia Entertainment"},
            {"ZV", "RedLynx"},
            {"ZW", "Judo Baby"},
            {"ZX", "TopWare Interactive"}
        });
    }
}