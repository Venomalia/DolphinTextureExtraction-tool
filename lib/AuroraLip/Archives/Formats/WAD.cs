using AuroraLib.Archives.DiscImage;
using AuroraLib.Common;
using AuroraLib.Common.Struct;
using System.Security.Cryptography;

namespace AuroraLib.Archives.Formats
{
    public class WAD : Archive, IFileAccess, IHasIdentifier
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => Magic;

        public static readonly Identifier32 Magic = new (0, 0, (byte)'s', (byte)'I');

        public WADHeader Header;

        public TMD TMD;

        public V0Ticket Ticket;

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
        {
            if (stream.Length > 4096)
            {
                WADHeader Header = stream.Read<WADHeader>(Endian.Big);
                if (Header.HeaderSize == 32 && Header.Magic == Magic)
                    return true;
            }
            return false;
        }

        protected override void Read(Stream stream)
        {
            Header = stream.Read<WADHeader>(Endian.Big);

            //Cert
            stream.Align(64);
            long CertPos = stream.Position;
            //Cert = new Cert(stream);
            stream.Seek(CertPos + Header.CertSize, SeekOrigin.Begin);
            //Ticket
            stream.Align(64);
            long TicketPos = stream.Position;
            Ticket = new V0Ticket(stream);
            stream.Seek(TicketPos + Header.TicketSize, SeekOrigin.Begin);
            //TMD
            stream.Align(64);
            long TMDPos = stream.Position;
            TMD = new TMD(stream);
            stream.Seek(TMDPos + Header.TMDSize, SeekOrigin.Begin);

            Root = new ArchiveDirectory() { Name = $"[{this.TMD.TitleID}]", OwnerArchive = this };
            Root.AddArchiveFile(stream, Header.HeaderSize, 0, "header.bin");
            Root.AddArchiveFile(stream, Header.CertSize, CertPos, "cert.bin");
            Root.AddArchiveFile(stream, Header.TicketSize, TicketPos, "ticket.bin");
            Root.AddArchiveFile(stream, Header.TMDSize, TMDPos, "tmd.bin");
            ArchiveDirectory filesDirectory = new ArchiveDirectory(this, Root) { Name = "files" };

            //Content
            Aes aes = Aes.Create();
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            aes.Key = Ticket.GetPartitionKey();

            long ContentPos = StreamEx.AlignPosition(stream.Position, 64);

            for (int i = 0; i < TMD.Content; i++)
            {
                stream.Align(64);

                SubStream Content = new SubStream(stream, StreamEx.AlignPosition((long)TMD.CMDs[i].Size, 16));

                aes.IV = TMD.CMDs[i].GetContentIV();

                byte[] buffer = new byte[Content.Length];
                using (CryptoStream cs = new CryptoStream(Content, aes.CreateDecryptor(), CryptoStreamMode.Read, false))
                {
                    cs.Read(buffer, 0, buffer.Length);
                }

                //sha1 comparison
                SHA1 sha1 = SHA1.Create();
                byte[] thishash = sha1.ComputeHash(buffer);
                if (!thishash.ArrayEqual(TMD.CMDs[i].Hash))
                    Events.NotificationEvent.Invoke(NotificationType.Warning, $"{nameof(WAD)} sha1 hash doesn't match in:'{TMD.CMDs[i].ContentId}_{TMD.CMDs[i].Type}.app'");

                MemoryStream de = new MemoryStream(buffer);
                filesDirectory.AddArchiveFile(de, $"{TMD.CMDs[i].ContentId}_{TMD.CMDs[i].Type}.app");
            }
            Root.Items.Add(filesDirectory.Name, filesDirectory);

            //Footer
            if (Header.FooterSize != 0)
            {
                stream.Seek(ContentPos + Header.ContentSize, SeekOrigin.Begin);
                stream.Align(64);
                Root.AddArchiveFile(stream, Header.FooterSize, CertPos, "footer.bin");
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        public struct WADHeader
        {
            public uint HeaderSize { get; }
            public Identifier32 Magic { get; }
            public uint CertSize { get; }
            private uint Reserved;
            public uint TicketSize { get; }
            public uint TMDSize;
            public uint ContentSize;
            public uint FooterSize;

            public WADHeader(uint tmdSize, uint contentSize)
            {
                HeaderSize = 32;
                Magic = WAD.Magic;
                CertSize = 2560;
                Reserved = 0;
                TicketSize = 676;
                TMDSize = tmdSize;
                ContentSize = contentSize;
                FooterSize = 0;
            }
        }
    }
}
