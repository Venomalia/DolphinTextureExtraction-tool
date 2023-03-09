namespace AuroraLip.Common
{

    /// <summary>
    /// Simple interface for a file access.
    /// </summary>
    public interface IFileAccess
    {
        /// <summary>
        /// Can be read
        /// </summary>
        bool CanRead { get; }
        /// <summary>
        /// Can be Write
        /// </summary>
        bool CanWrite { get; }
        /// <summary>
        /// Checks if the data Match with this FileFormat.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="extension"></param>
        /// <returns>"True" if match.</returns>
        bool IsMatch(Stream stream, in string extension = "");

    }
}
