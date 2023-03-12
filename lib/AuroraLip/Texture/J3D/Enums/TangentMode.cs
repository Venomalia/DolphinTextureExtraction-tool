namespace AuroraLib.Texture.J3D
{


    public static partial class J3DGraph
    {
        /// <summary>
        /// J3D Tangent Modes
        /// </summary>
        public enum TangentMode : short
        {
            /// <summary>
            /// One tangent value is stored, used for both the incoming and outgoing tangents
            /// </summary>
            SYNC = 0x00,
            /// <summary>
            /// Two tangent values are stored, the incoming and outgoing tangents, respectively
            /// </summary>
            DESYNC = 0x01
        }
    }
}
