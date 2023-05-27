using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AuroraLib.Texture.PixelFormats
{
    /// <summary>
    /// Represents a 16-bit pixel format containing a 8-bit intensity and a 8-bit alpha value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct IA8  : IPixel<IA8>, IPackedVector<ushort>, IPixel
    {

        /// <summary>
        /// Gets or sets the intensity component.
        /// </summary>
        public byte I { get; set; }

        /// <summary>
        /// Gets or sets the alpha component.
        /// </summary>
        public byte A { get; set; }

        public IA8(byte intensity, byte alpha)
        {
            I = intensity;
            A = alpha;
        }

        public IA8(ushort value)
        {
            I = (byte)(value & 0xFF);
            A = (byte)(value >> 8);
        }

        /// <inheritdoc />
        public ushort PackedValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Unsafe.As<IA8, ushort>(ref Unsafe.AsRef(this));
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Unsafe.As<IA8, ushort>(ref this) = value;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PixelOperations<IA8> CreatePixelOperations() => new();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(IA8 other) => this.PackedValue.Equals(other.PackedValue);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromAbgr32(Abgr32 source) => SetIA8((byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f), source.A);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromArgb32(Argb32 source) => SetIA8((byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f), source.A);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgr24(Bgr24 source) => SetIA8((byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f), 0xFF);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgra32(Bgra32 source) => SetIA8((byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f), source.A);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgra5551(Bgra5551 source) => FromVector4(source.ToVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromL16(L16 source) => SetIA8((byte)Math.Round(source.PackedValue / 65535f * 255f), 0xFF);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromL8(L8 source) => SetIA8(source.PackedValue, 0xFF);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromLa16(La16 source) => SetIA8(source.L, source.A);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromLa32(La32 source) => SetIA8((byte)(source.L >> 8), (byte)(source.A >> 8));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgb24(Rgb24 source) => SetIA8((byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f), 0xFF);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgb48(Rgb48 source) => SetIA8((byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f), 0xFF);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgba32(Rgba32 source) => SetIA8((byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f), source.A);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgba64(Rgba64 source) => SetIA8((byte)Math.Round((source.R * 30 + source.G * 59 + source.B * 11) / 100f), (byte)(source.A >> 8));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromScaledVector4(Vector4 vector) => this.FromVector4(vector);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromVector4(Vector4 vector) => SetIA8((byte)Math.Round((vector.X * 30 + vector.Y * 59 + vector.Z * 11) / 100f), (byte)Math.Round(vector.W * 255));

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
        private void SetIA8(byte intensity, byte alpha)
        {
            I = intensity;
            A = alpha;
        }
    }
}
