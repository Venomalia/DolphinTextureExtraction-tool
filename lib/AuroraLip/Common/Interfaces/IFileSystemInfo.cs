using AuroraLib.Core.Interfaces;

namespace AuroraLib.Common
{
    /// <summary>
    /// Provides standard file properties.
    /// </summary>
    public interface IFileSystemInfo : IObjectName, IDataTime
    {
        /// <summary>
        /// Represents the full path of the directory or file.
        /// </summary>
        string FullPath { get; }
    }
}
