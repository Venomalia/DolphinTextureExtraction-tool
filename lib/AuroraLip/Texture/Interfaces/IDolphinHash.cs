namespace AuroraLib.Texture.Interfaces
{
    /// <summary>
    /// Interface representing an Dolphin hash with Hash and TlutHash.
    /// </summary>
    public interface IDolphinHash
    {
        /// <summary>
        /// The hash of the texture.
        /// </summary>
        public ulong Hash { get; }

        /// <summary>
        /// The hash of the texture look-up table (TLUT), also known as texture palette.
        /// </summary>
        public ulong TlutHash { get; }
    }

}
