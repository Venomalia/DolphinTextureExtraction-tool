using AuroraLib.Core.Interfaces;
using AuroraLib.Core.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace AuroraLib.Core
{
    /// <summary>
    /// Represents a 32-bit identifier that is not affected by the endian order.
    /// </summary>
    public unsafe struct Identifier32 : IIdentifier
    {
        private byte b0, b1, b2, b3;

        /// <summary>
        /// Gets the memory address of the identifier as a pointer to the first byte.
        /// </summary>
        public byte* Address
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                fixed (byte* bytePtr = &b0)
                {
                    return bytePtr;
                }
            }
        }

        /// <inheritdoc />
        public byte this[int index]
        {
            [DebuggerStepThrough]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => AsSpan()[index];
            [DebuggerStepThrough]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => AsSpan()[index] = value;
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Identifier32"/> struct using the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes representing the identifier.</param>
        public Identifier32(ReadOnlySpan<byte> bytes) : this(bytes[0], bytes[1], bytes[2], bytes[3]) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Identifier32"/> struct using the specified bytes.
        /// </summary>
        /// <param name="b0">The first byte of the identifier.</param>
        /// <param name="b1">The second byte of the identifier.</param>
        /// <param name="b2">The third byte of the identifier.</param>
        /// <param name="b3">The fourth byte of the identifier.</param>
        public Identifier32(in byte b0, in byte b1, in byte b2, in byte b3)
        {
            this.b0 = b0;
            this.b1 = b1;
            this.b2 = b2;
            this.b3 = b3;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Identifier32"/> struct using the specified integer value.
        /// </summary>
        /// <param name="value">The integer value to initialize the identifier.</param>
        /// <param name="endian">The endianness of the identifier bytes.</param>
        public Identifier32(in int value, Endian endian = Endian.Little) : this((uint)value, endian) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Identifier32"/> struct using the specified unsigned integer value.
        /// </summary>
        /// <param name="value">The unsigned integer value to initialize the identifier.</param>
        /// <param name="endian">The endianness of the identifier bytes.</param>
        public Identifier32(in uint value, Endian endian = Endian.Little)
        {
            if (endian == Endian.Big)
                BitConverterX.Swap(value);

            b0 = (byte)(value & 0xFF);
            b1 = (byte)(value >> 8 & 0xFF);
            b2 = (byte)(value >> 16 & 0xFF);
            b3 = (byte)(value >> 24 & 0xFF);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Identifier32"/> struct using the specified character span.
        /// </summary>
        /// <param name="span">The character span to initialize the identifier. Only the first 4 characters will be considered.</param>
        public Identifier32(ReadOnlySpan<char> span)
        {
            ReadOnlySpan<char> span32 = span[..Math.Min(span.Length, 4)];
            Span<byte> bytes = stackalloc byte[4];
            Encoding.GetEncoding(28591).GetBytes(span32, bytes);

            b0 = bytes[0];
            b1 = bytes[1];
            b2 = bytes[2];
            b3 = bytes[3];
        }

        #endregion

        /// <inheritdoc />
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan()
            => new(Address, 4);

        /// <inheritdoc />
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetString()
            => EncodingX.GetString(AsSpan(), 0x0);

        /// <inheritdoc />
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetString(Encoding encoding)
            => EncodingX.GetString(AsSpan(), encoding, 0x0);

        /// <inheritdoc />
        public bool Equals(string other) => other == GetString();

        /// <inheritdoc />
        public bool Equals(IIdentifier other) => other != null && other.AsSpan().SequenceEqual(AsSpan());

        public static implicit operator Identifier32(uint v) => *(Identifier32*)&v;
        public static implicit operator uint(Identifier32 v) => *(uint*)&v;

        public static explicit operator Identifier32(int v) => *(Identifier32*)&v;
        public static explicit operator int(Identifier32 v) => *(int*)&v;

        public static explicit operator Identifier32(string v) => new(v);
        public static explicit operator string(Identifier32 v) => v.GetString();

        public override int GetHashCode() => (int)HashDepot.XXHash.Hash32(AsSpan());

        public override string ToString() => EncodingX.ValidSize(AsSpan()) > 2 ? GetString() : BitConverter.ToString(AsSpan().ToArray());
    }
}
