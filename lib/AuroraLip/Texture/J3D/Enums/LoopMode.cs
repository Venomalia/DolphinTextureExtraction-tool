namespace AuroraLib.Texture.J3D
{
    public static partial class J3DGraph
    {
        /// <summary>
        /// J3D Looping Modes
        /// </summary>
        public enum LoopMode : byte
        {
            /// <summary>
            /// Play Once then Stop.
            /// </summary>
            ONCE = 0x00,

            /// <summary>
            /// Play Once then Stop and reset to the first frame.
            /// </summary>
            ONCERESET = 0x01,

            /// <summary>
            /// Constantly play the animation.
            /// </summary>
            REPEAT = 0x02,

            /// <summary>
            /// Play the animation to the end. then reverse the animation and play to the start, then Stop.
            /// </summary>
            ONCEANDMIRROR = 0x03,

            /// <summary>
            /// Play the animation to the end. then reverse the animation and play to the start, repeat.
            /// </summary>
            REPEATANDMIRROR = 0x04
        }
    }
}
