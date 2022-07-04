namespace AuroraLip.Texture.J3D
{
    public static partial class JUtility
    {
        /// <summary>
        /// Defines how textures handle going out of [0..1] range for texcoords.
        /// </summary>
        public enum GXWrapMode : short
        {
            /// <summary>
            /// 
            /// </summary>
            CLAMP = 0x00,
            /// <summary>
            /// 
            /// </summary>
            REPEAT = 0x01,
            /// <summary>
            /// 
            /// </summary>
            MIRRORREAPEAT = 0x02
        }
    }
}
