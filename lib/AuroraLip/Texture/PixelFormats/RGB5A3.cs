using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Texture.PixelFormats
{
    /// <summary>
    /// Represents a 16-bit pixel format with either a 5-bit per-color or a 4-bit per-color and a 3-bit alpha channel.
    /// </summary>
    public partial struct RGB5A3 : IPixel<RGB5A3>, IPackedVector<ushort>
    {
        private const ushort AlphaMask = 0x8000;

        /// <summary>
        /// Gets or sets the red component.
        /// </summary>
        /// <remarks>
        /// The red channel is stored as a 5-bit value if the alpha channel is fully opaque (255),
        /// otherwise, it is stored as a 4-bit value.
        /// </remarks>
        public byte R
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)((PackedValue & AlphaMask) == 0 ? ((PackedValue >> 8) & 0xF) << 4 : ((PackedValue >> 10) & 0x1F) << 3);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetRGBA(R, G, B, A);
        }

        /// <summary>
        /// Gets or sets the green component.
        /// </summary>
        /// <remarks>
        /// The green channel is stored as a 5-bit value if the alpha channel is fully opaque (255),
        /// otherwise, it is stored as a 4-bit value.
        /// </remarks>
        public byte G
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)((PackedValue & AlphaMask) == 0 ? ((PackedValue >> 4) & 0xF) << 4 : ((PackedValue >> 5) & 0x1F) << 3);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetRGBA(R, G, B, A);
        }

        /// <summary>
        /// Gets or sets the blue component.
        /// </summary>
        /// <remarks>
        /// The blue channel is stored as a 5-bit value if the alpha channel is fully opaque (255),
        /// otherwise, it is stored as a 4-bit value.
        /// </remarks>
        public byte B
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)((PackedValue & AlphaMask) == 0 ? ((PackedValue) & 0xF) << 4 : ((PackedValue) & 0x1F) << 3);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetRGBA(R, G, B, A);
        }

        /// <summary>
        /// Gets or sets the alpha component.
        /// </summary>
        /// <remarks>
        /// If the alpha value is set to 255 (fully opaque) the flag bit is set to 1.
        /// Otherwise, it is stored in 3 bits, limiting the precision of other color channels to 4 bits.
        /// </remarks>
        public byte A
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((PackedValue & AlphaMask) == 0)
                {
                    int alpha = (PackedValue >> 12) & 0x7;
                    return (byte)(alpha << 5 | alpha << 2 | alpha >> 1);
                }
                return byte.MaxValue;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetRGBA(R, G, B, A);
        }

        /// <inheritdoc />
        public ushort PackedValue { get; set; }

        public RGB5A3(byte r, byte g, byte b, byte a) : this() => SetRGBA(r, g, b, a);

        public RGB5A3(ushort value) => PackedValue = value;

        private void SetRGBA(byte R, byte G, byte B, byte A = 0xFF)
        {
            int Result = 0x0000;
            if (A != 255)
            {
                Result |= (((A >> 5) & 0x7) << 12);
                Result |= (((R >> 4) & 0xF) << 8);
                Result |= (((G >> 4) & 0xF) << 4);
                Result |= (((B >> 4) & 0xF) << 0);
            }
            else
            {
                Result = AlphaMask;
                Result |= (((R >> 3) & 0x1F) << 10);
                Result |= (((G >> 3) & 0x1F) << 5);
                Result |= (((B >> 3) & 0x1F) << 0);
            }
            PackedValue = (ushort)Result;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PixelOperations<RGB5A3> CreatePixelOperations() => new();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(RGB5A3 other) => PackedValue.Equals(other.PackedValue);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromAbgr32(Abgr32 source) => SetRGBA(source.R, source.G, source.B, source.A);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromArgb32(Argb32 source) => SetRGBA(source.R, source.G, source.B, source.A);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgr24(Bgr24 source) => SetRGBA(source.R, source.G, source.B);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgra32(Bgra32 source) => SetRGBA(source.R, source.G, source.B, source.A);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgra5551(Bgra5551 source) => FromVector4(source.ToVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromL16(L16 source) => SetRGBA((byte)(source.PackedValue >> 8), (byte)(source.PackedValue >> 8), (byte)(source.PackedValue >> 8));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromL8(L8 source) => SetRGBA(source.PackedValue, source.PackedValue, source.PackedValue);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromLa16(La16 source) => SetRGBA(source.L, source.L, source.L, source.A);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromLa32(La32 source) => SetRGBA((byte)(source.L >> 8), (byte)(source.L >> 8), (byte)(source.L >> 8), (byte)(source.A >> 8));

        public void FromRgb24(Rgb24 source) => SetRGBA(source.R, source.G, source.B);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgb48(Rgb48 source) => SetRGBA((byte)(source.R >> 8), (byte)(source.G >> 8), (byte)(source.B >> 8));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgba32(Rgba32 source) => SetRGBA(source.R, source.G, source.B, source.A);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgba64(Rgba64 source) => SetRGBA((byte)(source.R >> 8), (byte)(source.G >> 8), (byte)(source.B >> 8), (byte)(source.A >> 8));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromScaledVector4(Vector4 vector) => FromVector4(vector);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromVector4(Vector4 vector) => SetRGBA((byte)(vector.X * 255), (byte)(vector.Y * 255), (byte)(vector.Z * 255), (byte)(vector.W * 255));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToRgba32(ref Rgba32 dest)
        {
            dest.R = R;
            dest.G = G;
            dest.B = B;
            dest.A = A;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToScaledVector4() => ToVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4() => new(R / 255f, G / 255f, B / 255f, A / 255f);
    }
}
