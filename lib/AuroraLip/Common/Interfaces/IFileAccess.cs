using AuroraLib.Core.Interfaces;

namespace AuroraLib.Common
{
    /// <summary>
    /// Simple interface for a file access.
    /// </summary>
    public interface IFileAccess : IFormatRecognition
    {
        /// <summary>
        /// Can be read
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// Can be Write
        /// </summary>
        bool CanWrite { get; }

    }
}
