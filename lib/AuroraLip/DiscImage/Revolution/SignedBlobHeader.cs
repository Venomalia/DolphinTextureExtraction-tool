namespace AuroraLib.DiscImage.Revolution
{
    public abstract class SignedBlobHeader
    {
        public SigTyp SignatureType { get; } //Signature type (always 65537 for RSA-2048)
        public readonly byte[] Certificate = new byte[256];
        public readonly byte[] SigPad = new byte[60];

        public SignedBlobHeader(Stream source)
        {
            SignatureType = source.Read<SigTyp>(Endian.Big);
            source.Read(Certificate);
            source.Read(SigPad);
        }

        public void Write(Stream dest)
        {
            dest.Write(SignatureType, Endian.Big);
            dest.Write(Certificate);
            dest.Write(SigPad);
            WriteData(dest);
        }

        protected abstract void WriteData(Stream dest);
    }
}
