namespace AuroraLib.DiscImage.Revolution
{
    public abstract class SignedBlobHeader
    {
        public SigTyp SignatureType { get; } //Signature type (always 65537 for RSA-2048)
        public readonly byte[] Certificate;
        public readonly byte[] SigPad;

        public SignedBlobHeader(Stream source)
        {
            SignatureType = source.Read<SigTyp>(Endian.Big);
            Certificate = source.Read(256);
            SigPad = source.Read(60);
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
