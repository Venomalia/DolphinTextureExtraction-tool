namespace AuroraLib.DiscImage.Dolphin
{
    public enum SystemCode : byte
    {
        /// <summary>
        /// Gamecube disc
        /// </summary>
        GC = (byte)'G',

        /// <summary>
        /// Gamecube Demo disc
        /// </summary>
        GCDemo = (byte)'D',

        /// <summary>
        /// Wii disc before 2010
        /// </summary>
        Revolution = (byte)'R',

        /// <summary>
        /// Wii disc after 2010
        /// </summary>
        Wii_Newer = (byte)'S',

        /// <summary>
        /// Wii Channel
        /// </summary>
        Channel = (byte)'H',

        /// <summary>
        /// WiiWare
        /// </summary>
        WiiWare = (byte)'W',

        /// <summary>
        /// Virtual Console Nintendo Entertainment System (NES) or Famicom (FC)
        /// </summary>
        NES = (byte)'F',

        /// <summary>
        /// Virtual Console Super Nintendo Entertainment System (SNES) or Super Famicom (SFC)
        /// </summary>
        SNES = (byte)'J',

        /// <summary>
        /// Nintendo 64
        /// </summary>
        N64 = (byte)'N',

        /// <summary>
        /// Virtual Console MasterSystem
        /// </summary>
        MasterSystem = (byte)'L',

        /// <summary>
        /// Virtual Console Commodore 64
        /// </summary>
        C64 = (byte)'C',

        /// <summary>
        /// Virtual Console Arcade
        /// </summary>
        Arcade = (byte)'E',

        /// <summary>
        /// Virtual Console Megadrive
        /// </summary>
        Megadrive = (byte)'M',

        /// <summary>
        /// Virtual Console(J) PCEngine or GC Promotional Disc
        /// </summary>
        PCEngine = (byte)'P',

        /// <summary>
        /// Virtual Console(J) PCEngine CD
        /// </summary>
        PCEngineCD = (byte)'Q',

        /// <summary>
        /// Virtual Console(J) MSX
        /// </summary>
        MSX = (byte)'X',

        /// <summary>
        /// Utility like the GBA-Player
        /// </summary>
        Utility = (byte)'U',

        //0-4 were used by nintendo
        /// <summary>
        /// Wii Service Disc (autoboot)
        /// </summary>
        Diagnostic = (byte)'0',

        /// <summary>
        /// Wii Service Disc 2
        /// </summary>
        Diagnostic1 = (byte)'1',

        DVDRelated = (byte)'2',

        /// <summary>
        /// Wii Backup Disc
        /// </summary>
        Backup = (byte)'4',

        ChanInstaller = (byte)'_',

        Unknown = default,
    }
}
