using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AuroraLib.Texture.PixelFormats
{
    /// <summary>
    /// Represents a 8-bit pixel format containing a 4-bit intensity and a 4-bit alpha value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public partial struct IA4 : IPixel<IA4>, IPackedVector<byte>
    {
        /// <summary>
        /// Gets or sets the intensity component.
        /// </summary>
        public byte I
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)(((PackedValue & 0xF) << 4) | PackedValue & 0xF);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetIA4(value, A);
        }

        /// <summary>
        /// Gets or sets the alpha component.
        /// </summary>
        public byte A
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)((((PackedValue >> 4) & 0xF) << 4) | ((PackedValue >> 4) & 0xF));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetIA4(I, value);
        }

        public IA4(byte packedValue) => PackedValue = packedValue;

        public IA4(byte intensity, byte alpha)
        {
            PackedValue = 0;
            PackedValue |= (byte)((intensity >> 4) & 0xF);
            PackedValue |= (byte)((alpha << 4) & 0xF0);
        }

        /// <inheritdoc />
        public byte PackedValue { get; set; }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PixelOperations<IA4> CreatePixelOperations() => new();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(IA4 other) => this.PackedValue.Equals(other.PackedValue);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromAbgr32(Abgr32 source) => SetIA4((byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f), source.A);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromArgb32(Argb32 source) => SetIA4((byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f), source.A);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgr24(Bgr24 source) => SetIA4((byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f), 0xFF);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgra32(Bgra32 source) => SetIA4((byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f), source.A);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgra5551(Bgra5551 source) => FromVector4(source.ToVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromL16(L16 source) => SetIA4((byte)Math.Round(source.PackedValue / 65535f * 255f), 0xFF);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromL8(L8 source) => SetIA4(source.PackedValue, 0xFF);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromLa16(La16 source) => SetIA4(source.L, source.A);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromLa32(La32 source) => SetIA4((byte)(source.L >> 8), (byte)(source.A >> 8));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgb24(Rgb24 source) => SetIA4((byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f), 0xFF);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgb48(Rgb48 source) => SetIA4((byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f), 0xFF);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgba32(Rgba32 source) => SetIA4((byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f), source.A);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgba64(Rgba64 source) => SetIA4((byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f), (byte)(source.A >> 8));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromScaledVector4(Vector4 vector) => this.FromVector4(vector);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromVector4(Vector4 vector) => SetIA4((byte)Math.Round((vector.X * 255 * 30 + vector.Y * 255 * 59 + vector.Z * 255 * 11) / 100f), (byte)Math.Round(vector.W * 255));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToRgba32(ref Rgba32 dest)
        {
            dest.R = I;
            dest.G = I;
            dest.B = I;
            dest.A = A;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToScaledVector4() => this.ToVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4() => new(I / 255f, I / 255f, I / 255f, A / 255f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetIA4(byte intensity, byte alpha)
        {
            PackedValue = 0;
            PackedValue |= (byte)((intensity >> 4) & 0xF);
            PackedValue |= (byte)((alpha << 4) & 0xF0);
        }
    }
}
