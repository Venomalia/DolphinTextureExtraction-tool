
namespace AFSLib
{
    /// <summary>
    /// Enumeration that contains all possible attribute info locations in an AFS archive.
    /// </summary>
    public enum AttributesInfoType
    {
        /// <summary>
        /// The AFS file doesn't contain an attributes block.
        /// </summary>
        NoAttributes,

        /// <summary>
        /// Info about the attributes block is located at the beginning of the attributes info block.
        /// </summary>
        InfoAtBeginning,

        /// <summary>
        /// Info about the attributes block is located at the end of the attributes info block.
        /// </summary>
        InfoAtEnd
    }
}