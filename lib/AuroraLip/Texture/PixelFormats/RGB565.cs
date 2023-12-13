using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Texture.PixelFormats
{
    /// <summary>
    /// Represents a 16-bit pixel format containing 5-bit for red, 6-bit for green, and 5-bit for blue.
    /// </summary>
    public partial struct RGB565 : IPixel<RGB565>, IPackedVector<ushort>
    {
        /// <summary>
        /// Gets or sets the red component.
        /// </summary>
        /// <remarks>
        /// The red component is stored in 5 bits, allowing a range of 0-31.
        /// When accessing the value is expanded to an 8-bit range (0-255).
        /// </remarks>
        public byte R
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)(((PackedValue >> 11) & 0x1F) << 3);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetRGB(value, G, B);
        }

        /// <summary>
        /// Gets or sets the green component.
        /// </summary>
        /// <remarks>
        /// The green component is stored in 6 bits, allowing a range of 0-63.
        /// When accessing the value is automatically expanded to an 8-bit range (0-255).
        /// </remarks>
        public byte G
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)(((PackedValue >> 5) & 0x3F) << 2);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetRGB(R, value, B);
        }

        /// <summary>
        /// Gets or sets the blue component.
        /// </summary>
        /// <remarks>
        /// The blue component is stored in 5 bits, allowing a range of 0-31.
        /// When accessing the value is automatically expanded to an 8-bit range (0-255).
        /// </remarks>
        public byte B
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)((PackedValue & 0x1F) << 3);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetRGB(R, G, value);
        }

        /// <inheritdoc />
        public ushort PackedValue { get; set; }

        public RGB565(in byte r, in byte g, in byte b) : this() => SetRGB(r, g, b);

        public RGB565(in ushort value) => PackedValue = value;

        private void SetRGB(in byte R, in byte G, in byte B)
        {
            int Value = 0x0000;
            Value |= ((R >> 3) & 0x1F) << 11;
            Value |= ((G >> 2) & 0x3F) << 5;
            Value |= (B >> 3) & 0x1F;
            PackedValue = (ushort)Value;
        }

        /// <inheritdoc />
        public PixelOperations<RGB565> CreatePixelOperations() => new();

        /// <inheritdoc />
        public bool Equals(RGB565 other) => PackedValue.Equals(other.PackedValue);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromAbgr32(Abgr32 source) => SetRGB(source.R, source.G, source.B);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromArgb32(Argb32 source) => SetRGB(source.R, source.G, source.B);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgr24(Bgr24 source) => SetRGB(source.R, source.G, source.B);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgra32(Bgra32 source) => SetRGB(source.R, source.G, source.B);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgra5551(Bgra5551 source) => FromVector4(source.ToVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromL16(L16 source) => SetRGB((byte)(source.PackedValue >> 8), (byte)(source.PackedValue >> 8), (byte)(source.PackedValue >> 8));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromL8(L8 source) => SetRGB(source.PackedValue, source.PackedValue, source.PackedValue);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromLa16(La16 source) => SetRGB(source.L, source.L, source.L);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromLa32(La32 source) => SetRGB((byte)(source.L >> 8), (byte)(source.L >> 8), (byte)(source.L >> 8));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgb24(Rgb24 source) => SetRGB(source.R, source.G, source.B);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgb48(Rgb48 source) => SetRGB((byte)(source.R >> 8), (byte)(source.G >> 8), (byte)(source.B >> 8));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgba32(Rgba32 source) => SetRGB(source.R, source.G, source.B);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgba64(Rgba64 source) => SetRGB((byte)(source.R >> 8), (byte)(source.G >> 8), (byte)(source.B >> 8));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromScaledVector4(Vector4 vector) => FromVector4(vector);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromVector4(Vector4 vector) => SetRGB((byte)(vector.X * 255), (byte)(vector.Y * 255), (byte)(vector.Z * 255));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromVector3(Vector3 vector) => SetRGB((byte)(vector.X * 255), (byte)(vector.Y * 255), (byte)(vector.Z * 255));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToRgba32(ref Rgba32 dest)
        {
            dest.R = R;
            dest.G = G;
            dest.B = B;
            dest.A = 0xFF;
        }

        public Vector4 ToScaledVector4() => ToVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4() => new(R / 255f, G / 255f, B / 255f, 1f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ToVector3() => new(R / 255f, G / 255f, B / 255f);
    }
}
