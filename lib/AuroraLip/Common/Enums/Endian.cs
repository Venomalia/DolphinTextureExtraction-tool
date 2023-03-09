namespace AuroraLip.Common
{
    /// <summary>
    /// Byte order, in which bytes are read and written.
    /// </summary>
    public enum Endian
    {
        /// <summary>
        /// stores the least-significant byte at the smallest address
        /// </summary>
        Little,
        /// <summary>
        /// stores the most significant byte at the smallest address
        /// </summary>
        Big,
        /// <summary>
        /// switch between big endian and little endian ordering
        /// </summary>
        Middle
    }
}
