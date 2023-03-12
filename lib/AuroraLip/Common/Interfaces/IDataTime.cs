namespace AuroraLib.Common
{
    /// <summary>
    /// Provides typical data time properties.
    /// </summary>
    public interface IDataTime
    {
        /// <summary>
        /// The creation date and time in UTC format.
        /// </summary>
        DateTime CreationTimeUtc { get; }

        /// <summary>
        /// The last modification date and time in UTC format.
        /// </summary>
        DateTime LastWriteTimeUtc { get; }

        /// <summary>
        /// The last accessed date and time in UTC format.
        /// </summary>
        DateTime LastAccessTimeUtc { get; }
    }
}
