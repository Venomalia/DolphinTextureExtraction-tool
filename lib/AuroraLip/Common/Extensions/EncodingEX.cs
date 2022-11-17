﻿using System.Diagnostics;
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
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="validbytes">Determines if the byte is valid</param>
        /// <returns></returns>
        //[DebuggerStepThrough]
        public static string ToValidString(this byte[] bytes, Predicate<byte> validbytes = null)
            => bytes.ToValidString(DefaultEncoding, validbytes);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="Encoding">Encoding to use when getting the string</param>
        /// <param name="validbytes">Determines if the byte is valid</param>
        /// <returns></returns>
        //[DebuggerStepThrough]
        public static string ToValidString(this byte[] bytes, Encoding Encoding, Predicate<byte> validbytes = null)
        {
            if (validbytes == null) validbytes = GetValidbytesPredicate(Encoding);

            List<byte> magicbytes = new List<byte>();
            foreach (byte b in bytes) if (validbytes.Invoke(b)) magicbytes.Add(b);
            return Encoding.GetString(magicbytes.ToArray());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="String"></param>
        /// <returns></returns>
        //[DebuggerStepThrough]
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
        //[DebuggerStepThrough]
        public static char ToChar(this byte Byte)
            => DefaultEncoding.GetChars(new byte[] { Byte })[0];
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Byte"></param>
        /// <param name="Encoding">Encoding to use when getting the char</param>
        /// <returns></returns>
        //[DebuggerStepThrough]
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
        /// Flip the ByteOrder for each field of the given <paramref name="type"/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="type"></param>
        /// <param name="offset"></param>
        //[DebuggerStepThrough]
        public static void FlipByteOrder(this byte[] buffer, Type type, int offset = 0)
        {
            if (type.IsPrimitive || type == typeof(UInt24) || type == typeof(Int24))
            {
                Array.Reverse(buffer, offset, Marshal.SizeOf(type));
                return;
            }

            foreach (var field in type.GetRuntimeFields())
            {
                if (field.IsStatic) continue;

                Type fieldtype = field.FieldType;

                if (fieldtype.IsEnum)
                    fieldtype = Enum.GetUnderlyingType(fieldtype);

                var subOffset = Marshal.OffsetOf(type, field.Name).ToInt32();
                buffer.FlipByteOrder(fieldtype, subOffset);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Char"></param>
        /// <param name="Encoding">Encoding to use when getting the byte</param>
        /// <returns></returns>
        //[DebuggerStepThrough]
        public static byte ToByte(this char Char, Encoding Encoding)
            => Encoding.GetBytes(Char.ToString())[0];

        internal static Predicate<byte> GetValidbytesPredicate(in Encoding encoder = null)
        {
            if (encoder == Encoding.ASCII) return AllValidBytes_ASKI;
            else return AllValidBytes;
        }
    }
}
