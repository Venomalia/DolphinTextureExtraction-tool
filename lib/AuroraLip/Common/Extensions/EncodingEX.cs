using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace AuroraLip.Common
{
    public static class EncodingEX
    {
        public static Encoding DefaultEncoding { get; set; } = Encoding.GetEncoding(28591);

        internal static readonly Predicate<byte> AllValidBytes_ASKI = b => b >= 32 && b < 127;

        internal static readonly Predicate<byte> AllValidBytes = b => b >= 32 && b != 127;

        /// <summary>
        /// Decodes all the Valid bytes in the specified byte array into a string.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="validbytes">Determines if the byte is valid</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string ToValidString(this byte[] bytes, Predicate<byte> validbytes = null)
            => bytes.ToValidString(DefaultEncoding, validbytes);

        /// <summary>
        /// Decodes all the Valid bytes in the specified byte array into a string.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="Encoding">Encoding to use when getting the string</param>
        /// <param name="validbytes">Determines if the byte is valid</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string ToValidString(this byte[] bytes, Encoding Encoding, Predicate<byte> validbytes = null)
        {
            if (validbytes == null) validbytes = GetValidbytesPredicate(Encoding);

            List<byte> magicbytes = new List<byte>();
            foreach (byte b in bytes) if (validbytes.Invoke(b)) magicbytes.Add(b);
            return Encoding.GetString(magicbytes.ToArray());
        }

        /// <summary>
        /// Decodes all the Valid bytes in the specified byte array into a string.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="validbytes"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string ToValidString(this byte[] bytes, int start, int length, Predicate<byte> validbytes = null)
            => bytes.ToValidString(DefaultEncoding, start, length, validbytes);
        /// <summary>
        /// Decodes all the Valid bytes in the specified byte array into a string.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="Encoding"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="validbytes"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string ToValidString(this byte[] bytes, Encoding Encoding, int start, int length, Predicate<byte> validbytes = null)
        {
            if (validbytes == null) validbytes = GetValidbytesPredicate(Encoding);

            List<byte> magicbytes = new List<byte>();
            for (int i = start; i < start + length; i++)
                if (validbytes.Invoke(bytes[i])) magicbytes.Add(bytes[i]);
            return Encoding.GetString(magicbytes.ToArray());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="String"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static byte[] ToByte(this string String)
            => DefaultEncoding.GetBytes(String);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="String"></param>
        /// <param name="Encoding">Encoding to use when getting the byte[]</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static byte[] ToByte(this string String, Encoding Encoding)
            => Encoding.GetBytes(String);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Byte"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static char ToChar(this byte Byte)
            => DefaultEncoding.GetChars(new byte[] { Byte })[0];
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Byte"></param>
        /// <param name="Encoding">Encoding to use when getting the char</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static char ToChar(this byte Byte, Encoding Encoding)
            => Encoding.GetChars(new byte[] { Byte })[0];
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Char"></param>
        /// <returns></returns>
        //[DebuggerStepThrough]
        public static byte ToByte(this char Char)
            => DefaultEncoding.GetBytes(Char.ToString())[0];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Char"></param>
        /// <param name="Encoding">Encoding to use when getting the byte</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static byte ToByte(this char Char, Encoding Encoding)
            => Encoding.GetBytes(Char.ToString())[0];

        internal static Predicate<byte> GetValidbytesPredicate(in Encoding encoder = null)
        {
            if (encoder == Encoding.ASCII) return AllValidBytes_ASKI;
            else return AllValidBytes;
        }

    }
}
