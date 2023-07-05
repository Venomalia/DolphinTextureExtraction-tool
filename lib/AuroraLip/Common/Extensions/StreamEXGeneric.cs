using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AuroraLib.Common
{
    public static partial class StreamEx
    {
        #region Read
        /// <summary>
        /// Reads a unmanaged struct of <typeparamref name="T"/> from the <paramref name="stream"/>.
        /// </summary>
        /// <typeparam name="T">The type of element.</typeparam>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="order">The endianness of the data in the stream. Default is <see cref="Endian.Little"/>.</param>
        /// <returns>The value <typeparamref name="T"/> that were read.</returns>
        /// <exception cref="EndOfStreamException">Thrown when attempting to read <typeparamref name="T"/> beyond the end of the stream.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe T Read<T>(this Stream stream, Endian order = Endian.Little) where T : unmanaged
        {
            int sizeT = sizeof(T);
            if (stream.Position + sizeT > stream.Length)
                ThrowHelper<T>();

            T value;
            Span<byte> buffer = new(&value, sizeT);
            stream.Read(buffer);
            if (order == Endian.Big == BitConverter.IsLittleEndian)
            {
                buffer.ReversesByteOrder(typeof(T));
            }
            return value;
        }

        /// <summary>
        /// Reads an array of <typeparamref name="T"/> from the <paramref name="stream"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="count">The number of elements to read.</param>
        /// <param name="order">The endianness of the data in the stream. Default is <see cref="Endian.Little"/>.</param>
        /// <returns>An array of <typeparamref name="T"/> read from the stream.</returns>
        /// <exception cref="EndOfStreamException">Thrown when attempting to read <typeparamref name="T"/> beyond the end of the stream.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static T[] Read<T>(this Stream stream, int count, Endian order = Endian.Little) where T : unmanaged
        {
            T[] values = new T[count];
            stream.Read<T>(values, order);
            return values;
        }

        /// <inheritdoc cref="Read{T}(Stream, uint, Endian)"/>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Read<T>(this Stream stream, uint count, Endian order = Endian.Little) where T : unmanaged
            => stream.Read<T>((int)count, order);

        /// <summary>
        /// Reads a span of <typeparamref name="T"/> from the <paramref name="stream"/>.
        /// </summary>
        /// <typeparam name="T">The type of the values in the span.</typeparam>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="values">The span of values to read into.</param>
        /// <param name="order">The endianness of the data in the stream. Default is <see cref="Endian.Little"/>.</param>
        /// <exception cref="EndOfStreamException">Thrown when attempting to read beyond the end of the stream.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe static void Read<T>(this Stream stream, Span<T> values, Endian order = Endian.Little) where T : unmanaged
        {
            int sizeT = sizeof(T);
            if (stream.Position + sizeT * values.Length > stream.Length)
                ThrowHelper<T>((uint)values.Length);

            Span<byte> buffer = MemoryMarshal.Cast<T, byte>(values);
            stream.Read(buffer);

            if (order == Endian.Big == BitConverter.IsLittleEndian && sizeT > 1)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    buffer.Slice(i * sizeT, sizeT).ReversesByteOrder(typeof(T));
                }
            }
        }

        #region Throw

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowHelper<T>()
            => throw new EndOfStreamException($"Cannot read {typeof(T)} is beyond the end of the stream.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowHelper<T>(uint count)
            => throw new EndOfStreamException($"Cannot read {typeof(T)}[{count}] is beyond the end of the stream.");

        #endregion

        #endregion

        #region Write
        /// <summary>
        /// Writes the specified <paramref name="objekt"/> to the <paramref name="stream"/> in the specified endianness.
        /// </summary>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        /// <param name="stream">The stream to write the value to.</param>
        /// <param name="value">The value to write to the stream.</param>
        /// <param name="order">The endianness of the data to write. Default is <see cref="Endian.Little"/>.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe void WriteObjekt<T>(this Stream stream, in T objekt, Endian order = Endian.Little) where T : unmanaged
        {
            T value = objekt;
            Span<byte> buffer = new(&value, sizeof(T));
            if (buffer.Length > 1 && order == Endian.Big)
            {
                buffer.ReversesByteOrder(typeof(T));
            }
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes a sequence of <typeparamref name="T"/> <paramref name="objects"/> to the <paramref name="stream"/>.
        /// </summary>
        /// <typeparam name="T">The type of objects in the sequence.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="objects">The sequence of <typeparamref name="T"/> objects to write.</param>
        /// <param name="order">The endianness of the data to write. Default is <see cref="Endian.Little"/>.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteObjekt<T>(this Stream stream, IEnumerable<T> objects, Endian order = Endian.Little) where T : unmanaged
        {
            foreach (var item in objects)
            {
                stream.WriteObjekt(item, order);
            }
        }

        /// <summary>
        /// Writes multiple instances of the specified <typeparamref name="T"/> <paramref name="objekt"/> to the <paramref name="stream"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to write.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="objekt">The object to write.</param>
        /// <param name="count">The number of times to write the object.</param>
        /// <param name="order">The endianness of the data to write. Default is <see cref="Endian.Little"/>.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static unsafe void WriteObjekt<T>(this Stream stream, T objekt, uint count, Endian order = Endian.Little) where T : unmanaged
        {
            T value = objekt;
            Span<byte> buffer = new(&value, sizeof(T));
            if (buffer.Length > 1 && order == Endian.Big)
            {
                buffer.ReversesByteOrder(typeof(T));
            }
            for (int i = 0; i < count; i++)
            {
                stream.Write(buffer);
            }
        }

        /// <inheritdoc cref="WriteObjekt{T}(Stream, T, uint, Endian)"/>
        public static unsafe void WriteObjekt<T>(this Stream stream, T objekt, int count, Endian order = Endian.Little) where T : unmanaged
            => WriteObjekt(stream, objekt, (uint)count, order);

        #endregion

        #region For
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
        public static T[] For<T, S>(this S stream, uint count, Func<S, T> func) where S : Stream
        {
            T[] values = new T[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = func(stream);
            }
            return values;
        }

        /// <inheritdoc cref="For{T, S}(S, uint, Func{S, T})"/>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] For<T, S>(this S stream, int count, Func<S, T> func) where S : Stream
            => stream.For((uint)count, func);
        #endregion

        #region At
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
            long orpos = stream.Position;
            stream.Seek(offset, origin);
            T value = func(stream);
            stream.Seek(orpos, SeekOrigin.Begin);
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
            long orpos = stream.Position;
            stream.Seek(offset, origin);
            func(stream);
            stream.Seek(orpos, SeekOrigin.Begin);
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
        #endregion

        #region Peek
        /// <summary>
        /// Returns the result of the given function on the specified <paramref name="stream"/> without changing the stream position.
        /// </summary>
        /// <typeparam name="T">The type of the value returned by the function.</typeparam>
        /// <typeparam name="S">The type of the stream.</typeparam>
        /// <param name="stream">The stream to peek at.</param>
        /// <param name="func">The function to apply to the stream.</param>
        /// <returns>The result of the given function.</returns>
        [DebuggerStepThrough]
        public static T Peek<T, S>(this S stream, Func<S, T> func) where S : Stream
        {
            long orpos = stream.Position;
            T value = func(stream);
            stream.Seek(orpos, SeekOrigin.Begin);
            return value;
        }

        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from the given <paramref name="stream"/> without advancing its position.
        /// </summary>
        /// <typeparam name="T">The type of the value to read.</typeparam>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="order">The endianness of the data in the stream. Default is <see cref="Endian.Little"/>.</param>
        /// <returns>The value of type T read from the stream.</returns>
        /// <exception cref="EndOfStreamException">Thrown when attempting to read <typeparamref name="T"/> beyond the end of the stream.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T Peek<T>(this Stream stream, Endian order = Endian.Little) where T : unmanaged
        {
            T value = stream.Read<T>(order);
            stream.Position -= sizeof(T);
            return value;
        }
        #endregion

    }
}
