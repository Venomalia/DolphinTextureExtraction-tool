namespace AuroraLip.Archives.DiscImage
{
    public enum SystemCode : int
    {
        /// <summary>
        /// Gamecube disc
        /// </summary>
        GC = 'G',
        /// <summary>
        /// Gamecube Demo disc
        /// </summary>
        GCDemo = 'D',
        /// <summary>
        /// Wii disc before 2010
        /// </summary>
        Revolution = 'R',
        /// <summary>
        /// Wii disc after 2010
        /// </summary>
        Wii_Newer = 'S',
        /// <summary>
        /// Wii Channel
        /// </summary>
        Channel = 'H',
        /// <summary>
        /// WiiWare
        /// </summary>
        WiiWare = 'W',
        /// <summary>
        /// Virtual Console Nintendo Entertainment System (NES) or Famicom (FC)
        /// </summary>
        NES = 'F',
        /// <summary>
        /// Virtual Console Super Nintendo Entertainment System (SNES) or Super Famicom (SFC)
        /// </summary>
        SNES = 'J',
        /// <summary>
        /// Nintendo 64
        /// </summary>
        N64 = 'N',
        /// <summary>
        /// Virtual Console MasterSystem
        /// </summary>
        MasterSystem = 'L',
        /// <summary>
        /// Virtual Console Commodore 64
        /// </summary>
        C64 = 'C',
        /// <summary>
        /// Virtual Console Arcade
        /// </summary>
        Arcade = 'E',
        /// <summary>
        /// Virtual Console Megadrive
        /// </summary>
        Megadrive = 'M',
        /// <summary>
        /// Virtual Console(J) PCEngine or GC Promotional Disc
        /// </summary>
        PCEngine = 'P',
        /// <summary>
        /// Virtual Console(J) PCEngine CD
        /// </summary>
        PCEngineCD = 'Q',
        /// <summary>
        /// Virtual Console(J) MSX
        /// </summary>
        MSX = 'X',
        /// <summary>
        /// Utility like the GBA-Player
        /// </summary>
        Utility = 'U',
        //0-4 were used by nintendo
        /// <summary>
        /// Wii Service Disc (autoboot)
        /// </summary>
        Diagnostic = '0',
        /// <summary>
        /// Wii Service Disc 2
        /// </summary>
        Diagnostic1 = '1',
        DVDRelated = '2',
        /// <summary>
        /// Wii Backup Disc
        /// </summary>
        Backup = '4',
        ChanInstaller = '_',
        Unknown = default,
        /// <summary>
        /// Wii disc
        /// </summary>
        WiiDisc = Revolution | Wii_Newer,
        /// <summary>
        /// Gamecube disc or Gamecube Demo disc
        /// </summary>
        GCDisc = GC | GCDemo | Utility | PCEngine,
        /// <summary>
        /// Virtual Console
        /// </summary>
        VirtualConsole = NES | SNES | N64 | MasterSystem | Megadrive | C64 | MSX | PCEngine | PCEngineCD | Arcade
    }
}
