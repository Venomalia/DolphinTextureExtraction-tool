namespace AuroraLip.Archives.DiscImage
{
    public enum RegionCode : int
    {
        /// <summary>
        /// Worldwide
        /// </summary>
        World = 'A',

        /// <summary>
        /// in China on the Nvidia Shield. 
        /// </summary>
        China = 'C',

        // NTSC
        /// <summary>
        /// PAL games released on NTSC-U Virtual Console
        /// </summary>
        NTSC_VC_P = 'B',
        /// <summary>
        /// Default country code for NTSC-U versions
        /// </summary>
        NTSC = 'E',
        /// <summary>
        /// NTSC-J games released on NTSC-U Virtual Console
        /// </summary>
        NTSC_VC_J = 'N',
        /// <summary>
        /// Default country code for NTSC-J versions
        /// </summary>
        Japan = 'J',
        /// <summary>
        /// NTSC-K
        /// </summary>
        Korea = 'K',
        /// <summary>
        /// NTSC-J games released on Korean Virtual Console
        /// </summary>
        Korea_VC_J = 'Q',
        /// <summary>
        /// NTSC-U games released on Korean Virtual Console
        /// </summary>
        Korea_VC_U = 'T',

        //Pal
        /// <summary>
        /// Pal German specific versions
        /// </summary>
        Germany = 'D',
        /// <summary>
        /// Pal France specific versions
        /// </summary>
        France = 'F',
        /// <summary>
        /// Pal Netherlands specific versions
        /// </summary>
        Netherlands = 'H',
        /// <summary>
        /// Pal Italy specific versions
        /// </summary>
        Italy = 'I',
        /// <summary>
        /// NTSC-U games released on PAL Virtual Console
        /// </summary>
        Pal_VC_U = 'M',
        /// <summary>
        /// Default country code for Pal versions
        /// </summary>
        Pal = 'P',
        /// <summary>
        /// Pal Spain specific versions
        /// </summary>
        Spain = 'S',
        /// <summary>
        /// NTSC-J games released on PAL Virtual Console
        /// </summary>
        Pal_VC_J = 'L',
        /// <summary>
        /// Pal Russia specific versions
        /// </summary>
        Russia = 'R',
        /// <summary>
        /// Pal Australia specific versions
        /// </summary>
        Australia = 'U',
        /// <summary>
        /// Pal nordic specific versions
        /// </summary>
        Nordic = 'V',
        /// <summary>
        /// Mainly Taiwan and Hong Kong
        /// </summary>
        Taiwan = 'W',

        //Special
        Special_X = 'X',
        Special_Y = 'Y',
        Special_Z = 'Z',
    }
}
