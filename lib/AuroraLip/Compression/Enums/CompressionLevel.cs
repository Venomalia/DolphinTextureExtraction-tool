namespace AuroraLib.Compression
{
    public enum CompressionLevel
    {
        /// <summary>
        /// No compression should be applied to the file.
        /// </summary>
        NoCompression = -1,

        /// <summary>
        /// The compression process should be executed optimally,
        /// even if the process takes a longer time
        /// </summary>
        Optimal = default,

        /// <summary>
        /// The compression process should be finished as soon as possible,
        /// even if the resulting file is not optimally compressed.
        /// </summary>
        Fastest,

        /// <summary>
        /// The compression operation should create output as small as possible,
        /// even if the operation takes a longer time to complete.
        /// </summary>
        SmallestSize
    }
}
