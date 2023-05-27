namespace AuroraLib.Archives.DiscImage
{
    public interface ISignedBlobHeader
    {
        /// <summary>
        /// Signature type always 0x10001 for RSA-2048 w/ SHA-1
        /// </summary>
        SigTyp SignatureType { get; }

        /// <summary>
        /// Signature covering the main header as well as all CMDs
        /// </summary>
        byte[] Certificate { get; }

        /// <summary>
        /// Padding for 64-byte alignmen
        /// </summary>
        byte[] SigPad { get; }
    }

    /// <summary>
    /// Signature types
    /// </summary>
    public enum SigTyp : uint
    {
        RSA_2048 = 0x00010001,
        RSA_4096 = 0x00010000
    }
}
