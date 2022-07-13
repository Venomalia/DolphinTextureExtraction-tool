
namespace AFSLib
{
    /// <summary>
    /// Enumeration containing each type of header magic that can be found in an AFS archive.
    /// </summary>
    public enum HeaderMagicType
    {
        /// <summary>
        /// Some AFS files contain a 4-byte header magic with 'AFS' followed by 0x00.
        /// </summary>
        AFS_00,

        /// <summary>
        /// Some AFS files contain a 4-byte header magic with 'AFS' followed by 0x20.
        /// </summary>
        AFS_20
    }
}