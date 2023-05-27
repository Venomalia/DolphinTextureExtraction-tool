using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Texture.PixelFormats
{
    /// <summary>
    /// Represents a 8-bit pixel format containing a 8-bit integer value.
    /// </summary>
    public struct I8 : IPixel<I8>, IPackedVector<byte>
    {
        public I8(byte intensity) => this.PackedValue = intensity;

        /// <inheritdoc />
        public byte PackedValue { get; set; }

        /// <inheritdoc />
        public PixelOperations<I8> CreatePixelOperations() => new();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromAbgr32(Abgr32 source) => PackedValue = (byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromArgb32(Argb32 source) => PackedValue = (byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgr24(Bgr24 source) => PackedValue = (byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgra32(Bgra32 source) => PackedValue = (byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgra5551(Bgra5551 source) => PackedValue = (byte)(source.PackedValue & 0x1F * 8);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromL16(L16 source) => PackedValue = (byte)(source.PackedValue >> 8);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromL8(L8 source) => PackedValue = source.PackedValue;

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromLa16(La16 source) => PackedValue = (byte)(source.L >> 8);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromLa32(La32 source) => PackedValue = (byte)(source.L >> 24);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgb24(Rgb24 source) => PackedValue = (byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgb48(Rgb48 source) => PackedValue = (byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgba32(Rgba32 source) => PackedValue = (byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgba64(Rgba64 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromScaledVector4(Vector4 vector) => this.FromVector4(vector);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromVector4(Vector4 vector) => PackedValue = (byte)Math.Round((vector.X * 30 + vector.Y * 59 + vector.Z * 11) / 100f);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToRgba32(ref Rgba32 dest)
        {
            dest.R = PackedValue;
            dest.G = PackedValue;
            dest.B = PackedValue;
            dest.A = PackedValue;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToScaledVector4() => this.ToVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4() => new(PackedValue / 255f, PackedValue / 255f, PackedValue / 255f, PackedValue / 255f);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(I8 other) => this.PackedValue.Equals(other.PackedValue);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(I8 left, I8 right) => left.Equals(right);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(I8 left, I8 right) => !(left == right);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => this.PackedValue.GetHashCode();

    }
}
