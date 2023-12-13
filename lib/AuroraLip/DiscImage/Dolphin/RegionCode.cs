namespace AuroraLib.DiscImage.Dolphin
{
    public enum RegionCode : byte
    {
        /// <summary>
        /// Worldwide
        /// </summary>
        World = (byte)'A',

        /// <summary>
        /// in China on the Nvidia Shield.
        /// </summary>
        China = (byte)'C',

        // NTSC
        /// <summary>
        /// PAL games released on NTSC-U Virtual Console
        /// </summary>
        NTSC_VC_P = (byte)'B',

        /// <summary>
        /// Default country code for NTSC-U versions
        /// </summary>
        NTSC = (byte)'E',

        /// <summary>
        /// NTSC-J games released on NTSC-U Virtual Console
        /// </summary>
        NTSC_VC_J = (byte)'N',

        /// <summary>
        /// Default country code for NTSC-J versions
        /// </summary>
        Japan = (byte)'J',

        /// <summary>
        /// NTSC-K
        /// </summary>
        Korea = (byte)'K',

        /// <summary>
        /// NTSC-J games released on Korean Virtual Console
        /// </summary>
        Korea_VC_J = (byte)'Q',

        /// <summary>
        /// NTSC-U games released on Korean Virtual Console
        /// </summary>
        Korea_VC_U = (byte)'T',

        //Pal
        /// <summary>
        /// Pal German specific versions
        /// </summary>
        Germany = (byte)'D',

        /// <summary>
        /// Pal France specific versions
        /// </summary>
        France = (byte)'F',

        /// <summary>
        /// Pal Netherlands specific versions
        /// </summary>
        Netherlands = (byte)'H',

        /// <summary>
        /// Pal Italy specific versions
        /// </summary>
        Italy = (byte)'I',

        /// <summary>
        /// NTSC-U games released on PAL Virtual Console
        /// </summary>
        Pal_VC_U = (byte)'M',

        /// <summary>
        /// Default country code for Pal versions
        /// </summary>
        Pal = (byte)'P',

        /// <summary>
        /// Pal Spain specific versions
        /// </summary>
        Spain = (byte)'S',

        /// <summary>
        /// NTSC-J games released on PAL Virtual Console
        /// </summary>
        Pal_VC_J = (byte)'L',

        /// <summary>
        /// Pal Russia specific versions
        /// </summary>
        Russia = (byte)'R',

        /// <summary>
        /// Pal Australia specific versions
        /// </summary>
        Australia = (byte)'U',

        /// <summary>
        /// Pal nordic specific versions
        /// </summary>
        Nordic = (byte)'V',

        /// <summary>
        /// Mainly Taiwan and Hong Kong
        /// </summary>
        Taiwan = (byte)'W',

        //Special
        Special_X = (byte)'X',

        Special_Y = (byte)'Y',
        Special_Z = (byte)'Z',
    }
}
