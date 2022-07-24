using System;
using System.Collections;

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
        public static Int24 ToInt24(byte[] value, int StartIndex)
            => new Int24(value[StartIndex] | value[StartIndex + 1] << 8 | value[StartIndex + 2] << 16);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(Int24 value)
            => new byte[3] { (byte)value.Value, (byte)(value.Value >> 8), (byte)(value.Value >> 16) };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="StartIndex"></param>
        /// <returns></returns>
        public static UInt24 ToUInt24(byte[] value, int StartIndex)
            => new UInt24((uint)(value[StartIndex] | value[StartIndex + 1] << 8 | value[StartIndex + 2] << 16));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(UInt24 value)
            => new byte[3] { (byte)value.Value, (byte)(value.Value >> 8), (byte)(value.Value >> 16) };

        /// <summary>
        /// Converts this BitArray to an Int32
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int ToInt32(this BitArray array)
        {
            if (array.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");

            int[] Finalarray = new int[1];
            array.CopyTo(Finalarray, 0);
            return Finalarray[0];
        }

        public static byte[] DataXor(this byte[] data, byte key)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ key);
            }
            return data;
        }
    }
}