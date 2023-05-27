using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Texture.PixelFormats
{
    /// <summary>
    /// Represents a 16-bit pixel format containing a 16-bit integer value.
    /// </summary>
    public struct I16 : IPixel<I16>, IPackedVector<ushort>
    {
        public I16(ushort intensity) => this.PackedValue = intensity;

        /// <inheritdoc />
        public ushort PackedValue { get; set; }

        /// <inheritdoc />
        public PixelOperations<I16> CreatePixelOperations() => new();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromAbgr32(Abgr32 source) => PackedValue = (ushort)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromArgb32(Argb32 source) => PackedValue = (ushort)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgr24(Bgr24 source) => PackedValue = (ushort)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgra32(Bgra32 source) => PackedValue = (ushort)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgra5551(Bgra5551 source) => PackedValue = (byte)(source.PackedValue & 0x1F * 8);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromL16(L16 source) => PackedValue = source.PackedValue;

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromL8(L8 source) => PackedValue = source.PackedValue;

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromLa16(La16 source) => PackedValue = source.L;

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromLa32(La32 source) => PackedValue = source.L;

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgb24(Rgb24 source) => PackedValue = (ushort)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgb48(Rgb48 source) => PackedValue = (ushort)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgba32(Rgba32 source) => PackedValue = (ushort)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgba64(Rgba64 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromScaledVector4(Vector4 vector) => this.FromVector4(vector);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromVector4(Vector4 vector) => PackedValue = (ushort)Math.Round((vector.X * 30 + vector.Y * 59 + vector.Z * 11) / 100f);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToRgba32(ref Rgba32 dest)
        {
            dest.R = (byte)(PackedValue >> 8);
            dest.G = (byte)(PackedValue >> 8);
            dest.B = (byte)(PackedValue >> 8);
            dest.A = (byte)(PackedValue >> 8);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToScaledVector4() => this.ToVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4() => new(PackedValue / 255f, PackedValue / 255f, PackedValue / 255f, PackedValue / 255f);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(I16 other) => this.PackedValue.Equals(other.PackedValue);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => this.PackedValue.GetHashCode();
    }
}
