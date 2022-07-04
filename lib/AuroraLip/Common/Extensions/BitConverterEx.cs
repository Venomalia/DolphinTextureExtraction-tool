namespace AuroraLip.Common
{
    /// <summary>
    /// Extra BitConverter functions
    /// </summary>
    public static class BitConverterEx
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="StartIndex"></param>
        /// <returns></returns>
        public static Int24 ToInt24(byte[] value, int StartIndex) => new Int24(value[StartIndex] | value[StartIndex + 1] << 8 | value[StartIndex + 2] << 16);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(Int24 value) => new byte[3] { (byte)value.Value, (byte)(value.Value >> 8), (byte)(value.Value >> 16) };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="StartIndex"></param>
        /// <returns></returns>
        public static UInt24 ToUInt24(byte[] value, int StartIndex) => new UInt24((uint)(value[StartIndex] | value[StartIndex + 1] << 8 | value[StartIndex + 2] << 16));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(UInt24 value) => new byte[3] { (byte)value.Value, (byte)(value.Value >> 8), (byte)(value.Value >> 16) };
    }
}