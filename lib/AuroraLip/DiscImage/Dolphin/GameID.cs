using AuroraLib.Core.Interfaces;
using AuroraLib.Core.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace AuroraLib.DiscImage.Dolphin
{
    /// <summary>
    /// consist of 6 Chars(byte) and is composed of the SystemCode, GameCode, RegionCode and MakerCode.
    /// </summary>
    public partial struct GameID : IIdentifier
    {
        public SystemCode SystemCode;
        private byte gameCode0, gameCode1;
        public RegionCode RegionCode;
        private byte makercode0, makercode1;

        /// <summary>
        /// Gets the memory address of the identifier as a pointer to the first byte.
        /// </summary>
        public unsafe byte* Address
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                fixed (SystemCode* bytePtr = &SystemCode)
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

        /// <summary>
        /// Get GameCode
        /// </summary>
        public Span<byte> GameCode => AsSpan().Slice(1, 2);

        /// <summary>
        /// Get MakerCode
        /// </summary>
        public Span<byte> MakerCode => AsSpan().Slice(4, 2);

        public GameID(ReadOnlySpan<char> Value)
        {
            if (Value.Length != 6)
                throw new ArgumentException($"A {nameof(GameID)} must consist of 6 characters");

            SystemCode = (SystemCode)Value[0];
            gameCode0 = (byte)Value[1];
            gameCode1 = (byte)Value[2];
            RegionCode = (RegionCode)Value[3];
            makercode0 = (byte)Value[4];
            makercode1 = (byte)Value[5];
        }

        public GameID(ReadOnlySpan<byte> Value)
        {
            if (Value.Length != 6)
                throw new ArgumentException($"A {nameof(GameID)} must consist of 6 characters");

            SystemCode = (SystemCode)Value[0];
            gameCode0 = Value[1];
            gameCode1 = Value[2];
            RegionCode = (RegionCode)Value[3];
            makercode0 = Value[4];
            makercode1 = Value[5];
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Span<byte> AsSpan()
            => new(Address, 6);

        /// <inheritdoc />
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetString()
            => EncodingX.GetCString(AsSpan());

        /// <inheritdoc />
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetString(Encoding encoding)
            => EncodingX.GetCString(AsSpan(), encoding);

        /// <inheritdoc />
        public bool Equals(string other) => other == GetString();

        /// <inheritdoc />
        public bool Equals(IIdentifier other) => other != null && other.AsSpan().SequenceEqual(AsSpan());

        public override string ToString() => EncodingX.GetDisplayableString(AsSpan());

        public string GetMaker()
        {
            MakerCodes.TryGetValue(new string(ToString().AsSpan(4, 2)), out string maker);
            return maker;
        }

        public override int GetHashCode() => (int)HashDepot.XXHash.Hash32(AsSpan());

        public override readonly bool Equals(object obj) => obj is GameID ID && ID == this;

        public static bool operator ==(GameID l, GameID r) => l.GetHashCode() == r.GetHashCode();

        public static bool operator !=(GameID l, GameID r) => !(l == r);

        public static implicit operator Span<byte>(GameID x) => x.AsSpan();

        public static explicit operator string(GameID x) => x.ToString();
    }
}
