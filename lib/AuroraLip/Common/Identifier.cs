using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace AuroraLib.Common
{
    /// <summary>
    /// Represents a identifier.
    /// </summary>
    public class Identifier : IIdentifier
    {
        public byte[] Bytes;

        /// <inheritdoc />
        public byte this[int index]
        {
            get => Bytes[index];
            set => Bytes[index] = value;
        }

        public Identifier(byte[] bytes)
            => Bytes = bytes;

        public Identifier(string span) : this(span.GetBytes())
        { }

        public Identifier(IIdentifier identifier) : this(identifier.AsSpan().ToArray())
        { }

        /// <inheritdoc />
        public Span<byte> AsSpan() => Bytes.AsSpan();

        /// <inheritdoc />
        public bool Equals(string other) => other == GetString();

        /// <inheritdoc />
        public bool Equals(IIdentifier other) => other.AsSpan().SequenceEqual(AsSpan());

        /// <inheritdoc />
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetString()
            => EncodingEX.GetString(AsSpan(), 0x0);

        /// <inheritdoc />
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetString(Encoding encoding)
            => EncodingEX.GetString(AsSpan(), encoding, 0x0);

        public static explicit operator Identifier(byte[] v) => new(v);
        public static explicit operator byte[](Identifier v) => v.Bytes;

        public override int GetHashCode() => (int)HashDepot.XXHash.Hash32(AsSpan());

        public override string ToString() => EncodingEX.ValidSize(AsSpan()) > Math.Max(2, Bytes.Length - 4) ? GetString() : BitConverter.ToString(AsSpan().ToArray());
    }
}
