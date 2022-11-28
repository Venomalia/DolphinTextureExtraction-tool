using static AuroraLip.Common.Events;

namespace AuroraLip.Common
{
    /// <summary>
    /// Interface to request additional files if necessary.
    /// </summary>
    internal interface IFileRequest
    {
        /// <summary>
        /// Event that is called when the process needs an additional file.
        /// </summary>
        FileRequestDelegate FileRequest { get; set; }
    }
}
