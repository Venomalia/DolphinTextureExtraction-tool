using AuroraLip.Common;

namespace AuroraLip.Archives.DiscImage
{
    public static class WiiKey
    {
        /// <summary>
        /// Wii Common Key
        /// </summary>
        public static readonly byte[] CKey = new byte[] { 176, 123, 5, 203, 217, 74, 35, 21, 134, 80, 232, 7, 220, 219, 48, 86 };

        /// <summary>
        /// Wii Korean Common Key
        /// </summary>
        public static readonly byte[] KKey = new byte[] { 108, 141, 91, 182, 36, 20, 57, 157, 189, 2, 191, 156, 37, 193, 106, 141 };

        /// <summary>
        /// vWii Common Key
        /// </summary>
        public static readonly byte[] VKey = new byte[] { 118, 189, 189, 122, 244, 60, 10, 171, 19, 1, 75, 60, 41, 170, 112, 86 };

        /// <summary>
        /// Generates the keys
        /// </summary>
        static WiiKey()
        {
            CKey = MiscEX.AESDecrypt(CKey, MiscEX.RKey(42, 16), MiscEX.RKey(13, 16));
            KKey = MiscEX.AESDecrypt(KKey, MiscEX.RKey(42, 16), MiscEX.RKey(13, 16));
            VKey = MiscEX.AESDecrypt(VKey, MiscEX.RKey(42, 16), MiscEX.RKey(13, 16));
        }
    }
}
