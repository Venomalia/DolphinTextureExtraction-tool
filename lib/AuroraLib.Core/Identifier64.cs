using AuroraLib.Core.Interfaces;
using AuroraLib.Core.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace AuroraLib.Core
{
    /// <summary>
    /// Represents a 64-bit identifier that is not affected by the endian order.
    /// </summary>
    public unsafe struct Identifier64 : IIdentifier
    {
        /// <summary>
        /// The lower 32-bit identifier.
        /// </summary>
        public Identifier32 Lower;

        /// <summary>
        /// The higher 32-bit identifier.
        /// </summary>
        public Identifier32 Higher;

        /// <summary>
        /// Gets the memory address of the identifier as a pointer to the first byte.
        /// </summary>
        public byte* Address
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                fixed (Identifier32* bytePtr = &Lower)
                {
                    return (byte*)bytePtr;
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
        /// Initializes a new instance of the <see cref="Identifier64"/> struct from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes representing the identifier.</param>
        public Identifier64(ReadOnlySpan<byte> bytes) : this(new Identifier32(bytes[..4]), new Identifier32(bytes.Slice(4, 4))) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Identifier64"/> struct with the specified lower and higher identifiers.
        /// </summary>
        /// <param name="identifierLower">The lower 32-bit identifier.</param>
        /// <param name="identifierHigher">The higher 32-bit identifier.</param>
        public Identifier64(Identifier32 identifierLower, Identifier32 identifierHigher)
        {
            Lower = identifierLower;
            Higher = identifierHigher;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Identifier64"/> struct with the specified value and endianness.
        /// </summary>
        /// <param name="value">The 64-bit value to initialize the identifier.</param>
        /// <param name="endian">The endianness of the value.</param>
        public Identifier64(long value, Endian endian = Endian.Little) : this((ulong)value, endian) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Identifier64"/> struct with the specified 64-bit value and endianness.
        /// </summary>
        /// <param name="value">The 64-bit value to initialize the identifier.</param>
        /// <param name="endian">The endianness of the value.</param>
        public Identifier64(ulong value, Endian endian = Endian.Little)
        {
            if (endian == Endian.Big)
                value = BitConverterX.Swap(value);

            Lower = new Identifier32((uint)value);
            Higher = new Identifier32((uint)(value >> 32));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Identifier64"/> struct with the specified character span.
        /// </summary>
        /// <param name="span">The character span representing the identifier.</param>
        public Identifier64(ReadOnlySpan<char> span)
        {
            ReadOnlySpan<char> span64 = span[..Math.Min(span.Length, 8)];
            Span<byte> bytes = stackalloc byte[8];
            Encoding.GetEncoding(28591).GetBytes(span64, bytes);
            Lower = new Identifier32(bytes[..4]);
            Higher = new Identifier32(bytes.Slice(4, 4));
        }

        #endregion

        /// <inheritdoc />
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan()
            => new(Address, 8);

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

        public static implicit operator Identifier64(ulong v) => *(Identifier64*)&v;
        public static implicit operator ulong(Identifier64 v) => *(ulong*)&v;

        public static explicit operator Identifier64(long v) => *(Identifier64*)&v;
        public static explicit operator long(Identifier64 v) => *(long*)&v;

        public static explicit operator Identifier64(string v) => new(v);
        public static explicit operator string(Identifier64 v) => v.GetString();

        public override int GetHashCode() => (int)HashDepot.XXHash.Hash32(AsSpan());

        public override string ToString() => EncodingX.ValidSize(AsSpan()) > 2 ? GetString() : BitConverter.ToString(AsSpan().ToArray());
    }
}
