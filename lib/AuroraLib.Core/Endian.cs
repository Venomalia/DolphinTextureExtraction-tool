namespace AuroraLib.Core
{
    /// <summary>
    /// Byte order, in which bytes are read and written.
    /// </summary>
    public enum Endian : ushort
    {
        /// <summary>
        /// stores the least-significant byte at the smallest address
        /// </summary>
        Little = 0xFEFF,

        /// <summary>
        /// stores the most significant byte at the smallest address
        /// </summary>
        Big = 0xFFFE,
    }
}
