namespace AuroraLip.Texture.J3D
{
    public static partial class JUtility
    {
        /// <summary>
        /// FilterMode specifies what type of filtering the file should use for min/mag.
        /// </summary>
        public enum GXFilterMode : byte
        {
            /// <summary>
            /// Point Sampling, No Mipmap
            /// </summary>
            Nearest = 0x00,
            /// <summary>
            /// Bilinear Filtering, No Mipmap
            /// </summary>
            Linear = 0x01,
            /// <summary>
            /// Point Sampling, Discrete Mipmap
            /// </summary>
            NearestMipmapNearest = 0x02,
            /// <summary>
            /// Bilinear Filtering, Discrete Mipmap
            /// </summary>
            NearestMipmapLinear = 0x03,
            /// <summary>
            /// Point Sampling, Linear MipMap
            /// </summary>
            LinearMipmapNearest = 0x04,
            /// <summary>
            /// Trilinear Filtering
            /// </summary>
            LinearMipmapLinear = 0x05
        }
    }
}
