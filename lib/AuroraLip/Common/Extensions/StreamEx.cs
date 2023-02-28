using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace AuroraLip.Common
{

    /// <summary>
    /// Extension of the <see cref="Stream"/>.
    /// </summary>
    public static partial class StreamEx
    {
        /// <summary>
        /// Reads a block of bytes from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="Count">The maximum number of bytes to read.</param>
        /// <param name="order">Byte order, in which bytes are read.</param>
        /// <param name="Offset">The byte offset in array at which the read bytes will be placed.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="ArgumentOutOfRangeException">Offset or Count is negative.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ArgumentException">Offset and Count describe an invalid range in array.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        [DebuggerStepThrough]
        public static byte[] Read(this Stream stream, int Count, Endian order = Endian.Little, int Offset = 0)
        {

#if DEBUG
            if (stream.Position + Count > stream.Length)
                Events.NotificationEvent.Invoke(NotificationType.Warning, $"Passed limit of {stream}.");
#endif

            byte[] Final = new byte[Count];
            stream.Read(Final, Offset, Count);
            switch (order)
            {
                case Endian.Big:
                    Array.Reverse(Final);
                    break;
                case Endian.Middle:
                    throw new NotImplementedException();
            }
            return Final;
        }

        /// <summary>
        /// Writes a block of bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="Array">The buffer containing data to write to the stream</param>
        /// <param name="Count">The maximum number of bytes to write</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <exception cref="ArgumentNullException">array is null.</exception>
        /// <exception cref="ArgumentException">offset and count describe an invalid range in array.</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or count is negative</exception>
        /// <exception cref="IOException">An I/O error occurred, or Another thread may have caused an unexpected change in the position of the operating system's file handle</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed</exception>
        /// <exception cref="NotSupportedException">The current stream instance does not support writing</exception>
        [DebuggerStepThrough]
        public static void Write(this Stream stream, byte[] Array, int Count, Endian order = Endian.Little, int Offset = 0)
        {
            switch (order)
            {
                case Endian.Big:
                    System.Array.Reverse(Array);
                    break;
                case Endian.Middle:
                    throw new NotImplementedException();
            }
            stream.Write(Array, Offset, Count);
        }

        /// <summary>
        /// Reads a object of <typeparamref name="T"/> from the stream. (Requires more testing)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="order">Byte order, in which bytes are read.</param>
        /// <returns>The value <typeparamref name="T"/> that were read.</returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="ArgumentOutOfRangeException">Offset or Count is negative.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ArgumentException">Offset and Count describe an invalid range in array.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public static T Read<T>(this Stream stream, Endian order = Endian.Little)
        {
            byte[] buffer = new byte[Marshal.SizeOf<T>()];
            stream.Read(buffer, 0, buffer.Length);
            switch (order)
            {
                case Endian.Big:
                    buffer.FlipByteOrder(typeof(T));
                    break;
                case Endian.Middle:
                    throw new NotImplementedException();
            }

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                IntPtr rawDataPtr = handle.AddrOfPinnedObject();
                return Marshal.PtrToStructure<T>(rawDataPtr);
            }
            finally
            {
                handle.Free();
            }
        }
        /// <summary>
        /// Writes an <paramref name="objekt"/> of <typeparamref name="T"/> to the <paramref name="stream"/>. (Requires more testing)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="objekt"></param>
        /// <param name="order">Byte order, in which bytes are read.</param>
        /// <exception cref="NotImplementedException"></exception>
        public static void WriteObjekt<T>(this Stream stream, T objekt, Endian order = Endian.Little)
        {
            byte[] buffer = new byte[Marshal.SizeOf<T>()];

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                IntPtr rawDataPtr = handle.AddrOfPinnedObject();
                Marshal.StructureToPtr<T>(objekt, rawDataPtr, false);
            }
            finally
            {
                handle.Free();
            }

            switch (order)
            {
                case Endian.Big:
                    buffer.FlipByteOrder(typeof(T));
                    break;
                case Endian.Middle:
                    throw new NotImplementedException();
            }
            stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Invokes <paramref name="func"/> of <typeparamref name="T"/> for <paramref name="count"/> times within this <typeparamref name="S"/>/>.
        /// </summary>
        /// <typeparam name="T">The value returned by <paramref name="func"/>.</typeparam>
        /// <typeparam name="S"></typeparam>
        /// <param name="stream"></param>
        /// <param name="count">How many times the <paramref name="func"/> should be Invoke</param>
        /// <param name="func">a function to be called <paramref name="count"/> times x</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static T[] For<T, S>(this S stream, int count, Func<S, T> func) where S : Stream
        {
            T[] values = new T[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = func(stream);
            }
            return values;
        }

        /// <summary>
        /// Invokes <paramref name="func"/> at the given <paramref name="position"/> within the <typeparamref name="S"/>, retains the current position within the <typeparamref name="S"/>.
        /// </summary>
        /// <typeparam name="T">The value returned by <paramref name="func"/>.</typeparam>
        /// <typeparam name="S"></typeparam>
        /// <param name="stream"></param>
        /// <param name="position">the position within the current</param>
        /// <param name="func">a function to be Invoke at the desired position</param>
        /// <returns>The value <typeparamref name="T"/> returned by <paramref name="func"/>.</returns>
        /// <exception cref="IOException">An I/O error occurred, or Another thread may have caused an unexpected change in the position of the operating system's file handle</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed</exception>
        /// <exception cref="NotSupportedException">The current stream instance does not support writing</exception>
        [DebuggerStepThrough]
        public static T At<T, S>(this S stream, long position, Func<S, T> func) where S : Stream
            => stream.At(position, SeekOrigin.Begin, func);

        /// <summary>
        /// Invokes <paramref name="func"/> at the given <paramref name="position"/> within the <typeparamref name="S"/>, retains the current position within the <typeparamref name="S"/>.
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="stream"></param>
        /// <param name="position">the position within the current</param>
        /// <param name="func">a function to be Invoke at the desired position</param>
        /// <exception cref="IOException">An I/O error occurred, or Another thread may have caused an unexpected change in the position of the operating system's file handle</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed</exception>
        /// <exception cref="NotSupportedException">The current stream instance does not support writing</exception>
        [DebuggerStepThrough]
        public static void At<S>(this S stream, long position, Action<S> func) where S : Stream
            => stream.At(position, SeekOrigin.Begin, func);

        /// <summary>
        /// Invokes <paramref name="func"/> at the given <paramref name="offset"/> and <paramref name="origin"/> within the <typeparamref name="S"/>, retains the current position within the <typeparamref name="S"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="S"></typeparam>
        /// <param name="stream"></param>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <param name="func">a function to be Invoke at the desired position</param>
        /// <returns>The value <typeparamref name="T"/> returned by <paramref name="func"/>.</returns>
        /// <exception cref="IOException">An I/O error occurred, or Another thread may have caused an unexpected change in the position of the operating system's file handle</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed</exception>
        /// <exception cref="NotSupportedException">The current stream instance does not support writing</exception>
        [DebuggerStepThrough]
        public static T At<T, S>(this S stream, long offset, SeekOrigin origin, Func<S, T> func) where S : Stream
        {
            var orpos = stream.Position;
            stream.Seek(offset, origin);
            T value = func(stream);
            stream.Seek(orpos, origin);
            return value;
        }

        /// <summary>
        /// Invokes <paramref name="func"/> at the given <paramref name="offset"/> and <paramref name="origin"/> within the <typeparamref name="S"/>, retains the current position within the <typeparamref name="S"/>.
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="stream"></param>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <param name="func">a function to be Invoke at the desired position</param>
        /// <exception cref="IOException">An I/O error occurred, or Another thread may have caused an unexpected change in the position of the operating system's file handle</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed</exception>
        /// <exception cref="NotSupportedException">The current stream instance does not support writing</exception>
        [DebuggerStepThrough]
        public static void At<S>(this S stream, long offset, SeekOrigin origin, Action<S> func) where S : Stream
        {
            var orpos = stream.Position;
            stream.Seek(offset, origin);
            func(stream);
            stream.Seek(orpos, origin);
        }

        [DebuggerStepThrough]
        public static void Write(this Stream stream, byte[] Array, Endian order = Endian.Little, int Offset = 0)
            => stream.Write(Array, Array.Length, order, Offset);

        [DebuggerStepThrough]
        public static void WriteBigEndian(this Stream FS, byte[] Array, int Count, int Offset = 0)
            => FS.Write(Array, Count, Endian.Big, Offset);

        /// <summary>
        /// Peek the next byte
        /// </summary>
        /// <param name="FS">this</param>
        /// <returns>The next byte to be read</returns>
        [DebuggerStepThrough]
        public static byte PeekByte(this Stream FS)
        {
            byte val = (byte)FS.ReadByte();
            FS.Position--;
            return val;
        }
        /// <summary>
        /// Reads the stream and outputs it as array
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static byte[] ToArray(this Stream stream, int bufferSize = 81920)
        {
            if (stream is MemoryStream ms)
                return ms.ToArray();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream, bufferSize);
                return memoryStream.ToArray();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="magic">Match</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static bool MatchString(this Stream stream, in string magic)
        {
            if (magic.Length > stream.Length - stream.Position) return false;
            return stream.ReadString(magic.Length) == magic;
        }

        /// <summary>
        /// searches for a specific pattern in a stream and moves its position until the pattern is found.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pattern">the string to search for</param>
        /// <returns>"true" when the pattern is found</returns>
        [DebuggerStepThrough]
        public static bool Search(this Stream stream, string pattern) => stream.Search(pattern.ToByte());

        /// <summary>
        /// searches for a specific pattern in a stream and moves its position until the pattern is found.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pattern">the byte[] to search for</param>
        /// <returns>"true" when the pattern is found</returns>
        [DebuggerStepThrough]
        public static bool Search(this Stream stream, byte[] pattern)
        {
            int i = 0;
            int readbyte;
            while ((readbyte = stream.ReadByte()) > -1)
            {
                if (readbyte == pattern[i])
                {
                    i++;
                    if (i != pattern.Length)
                        continue;

                    stream.Seek(-pattern.Length, SeekOrigin.Current);
                    return true;
                }
                else
                    i = 0;
            }
            return false;
        }

        /// <summary>
        /// searches for a specific pattern in a stream and moves its position until a pattern is found.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pattern">a Enumerable of byte[] to search for</param>
        /// <param name="match">the found byte[] that was found in the stream</param>
        /// <returns>"true" when the pattern is found</returns>
        public static bool Search(this Stream stream, IEnumerable<byte[]> pattern, out byte[] match)
        {
            int[] i = new int[pattern.Count()];
            for (int p = 0; p < pattern.Count(); p++)
                i[p] = 0;

            int readbyte;
            while ((readbyte = stream.ReadByte()) > -1)
            {
                for (int p = 0; p < pattern.Count(); p++)
                {
                    if (readbyte == pattern.ElementAt(p)[i[p]])
                    {
                        i[p]++;
                        if (i[p] != pattern.ElementAt(p).Length)
                            continue;

                        stream.Seek(-pattern.ElementAt(p).Length, SeekOrigin.Current);
                        match = pattern.ElementAt(p);
                        return true;
                    }
                    else
                        i[p] = 0;
                }
            }
            match = null;
            return false;
        }

        /// <summary>
        /// Writes a bit for the specified length
        /// (redundant?)
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        /// <param name="Byte">The bit that gets write to the stream</param>
        [DebuggerStepThrough]
        public static void WriteX(this Stream stream, int length, byte Byte = 0x00)
        {
            for (int i = 0; i < length; i++)
                stream.WriteByte(Byte);
        }

        /// <summary>
        /// sets the position within the current stream to the nearest possible boundary.
        /// </summary>
        /// <param name="stream">the current stream</param>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type System.IO.SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <param name="boundary">The byte boundary to Seek to</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static long Seek(this Stream stream, long offset, SeekOrigin origin, int boundary)
        {
            if (boundary <= 1)
                throw new ArgumentException($"{nameof(boundary)}: Must be 2 or more");

            switch (origin)
            {
                case SeekOrigin.Begin:
                    break;
                case SeekOrigin.Current:
                    offset += stream.Position;
                    break;
                case SeekOrigin.End:
                    offset = stream.Length + offset;
                    break;
            }
            offset = (long)Math.Ceiling((double)offset / boundary) * boundary;
            return stream.Seek(offset, SeekOrigin.Begin);
        }

        /// <summary>
        /// sets the position within the current stream to the nearest possible boundary.
        /// </summary>
        /// <param name="stream">the current stream</param>
        /// <param name="origin">A value of type System.IO.SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static long Seek(this Stream stream, int boundary = 32)
            => stream.Seek(CalculatePadding(stream.Position, boundary), SeekOrigin.Begin);

        /// <summary>
        /// Adds Padding to the Current Position in the provided Stream
        /// </summary>
        /// <param name="FS">The Stream to add padding to</param>
        /// <param name="Multiple">The byte multiple to pad to</param>
        /// <param name="Padding">The bit that gets write to the stream</param>
        [DebuggerStepThrough]
        public static void WritePadding(this Stream FS, int Multiple, byte Padding = 0x00)
        {
            while (FS.Position % Multiple != 0)
                FS.WriteByte(Padding);
        }

        /// <summary>
        /// Adds Padding to the Current Position in the provided Stream use a String
        /// </summary>
        /// <param name="stream">The Stream to add padding to</param>
        /// <param name="multiple">The byte multiple to pad to</param>
        [DebuggerStepThrough]
        public static void WritePadding(this Stream stream, int multiple, in string Padding)
        {
            int PadCount = 0;
            while (stream.Position % multiple != 0)
            {
                stream.WriteByte((byte)Padding[PadCount++]);
                if (Padding.Length < PadCount) PadCount = 0;
            }
        }

        /// <summary>
        /// get the first position in the nearest possible boundary.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="boundary">The byte boundary to pad to</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static long CalculatePadding(long position, int boundary)
        {
            if (position % boundary != 0)
                position += (boundary - (position % boundary));

            return position;
        }
    }
}
