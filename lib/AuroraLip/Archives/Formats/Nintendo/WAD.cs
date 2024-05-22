using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Cryptography;
using AuroraLib.Core.Interfaces;
using AuroraLib.DiscImage.Revolution;
using System.Security.Cryptography;

namespace AuroraLib.Archives.Formats.Nintendo
{
    /// <summary>
    /// Nintendo Wii WAD Container.
    /// </summary>
    public sealed class WAD : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => Magic;

        public static readonly Identifier32 Magic = new((byte)'I', (byte)'s', 0, 0);

        public WADHeader Header;

        public TMD TMD;

        public V0Ticket Ticket;

        public WAD()
        { }

        public WAD(string name) : base(name)
        { }

        public WAD(FileNode source) : base(source)
        { }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
        {
            if (stream.Length > 4096)
            {
                WADHeader Header = stream.Read<WADHeader>(Endian.Big);
                if (Header.HeaderSize == 32 && Header.Magic == Magic)
                    return true;
            }
            return false;
        }

        protected override void Deserialize(Stream source)
        {
            Header = source.Read<WADHeader>(Endian.Big);

            //Cert
            source.Align(64);
            long CertPos = source.Position;
            source.Seek(CertPos + Header.CertSize, SeekOrigin.Begin);
            //Ticket
            source.Align(64);
            long TicketPos = source.Position;
            Ticket = new V0Ticket(source);
            source.Seek(TicketPos + Header.TicketSize, SeekOrigin.Begin);
            //TMD
            source.Align(64);
            long TMDPos = source.Position;
            TMD = new TMD(source);
            source.Seek(TMDPos + Header.TMDSize, SeekOrigin.Begin);
            Name = $"[{TMD.TitleID}]";
            Add(new FileNode("header.bin", new SubStream(source, Header.HeaderSize, 0)));
            Add(new FileNode("cert.bin", new SubStream(source, Header.CertSize, CertPos)));
            Add(new FileNode("ticket.bin", new SubStream(source, Header.TicketSize, TicketPos)));
            Add(new FileNode("tmd.bin", new SubStream(source, Header.TMDSize, TMDPos)));
            DirectoryNode filesDirectory = new("files");
            Add(filesDirectory);

            //Content
            BaseHash sha1 = new(SHA1.Create());
            Aes aes = Aes.Create();
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            aes.Key = Ticket.GetPartitionKey();

            long ContentPos = StreamEx.AlignPosition(source.Position, 64);

            for (int i = 0; i < TMD.CMDs.Count; i++)
            {
                //Decrypt
                aes.IV = TMD.CMDs[i].GetContentIV();
                source.Align(64);
                SubStream Content = new(source, StreamEx.AlignPosition((long)TMD.CMDs[i].Size, 16));
                MemoryPoolStream de = new((int)Content.Length, true);
                using (CryptoStream cs = new(Content, aes.CreateDecryptor(), CryptoStreamMode.Read, false))
                {
                    cs.Read(de.UnsaveAsSpan());
                }

                string name = $"{TMD.CMDs[i].ContentId}_{TMD.CMDs[i].Type}.app";
                filesDirectory.Add(new FileNode(name, de));

                //sha1 hash comparison
                sha1.Reset();
                sha1.Compute(de.UnsaveAsSpan());
                if (!sha1.GetBytes().SequenceEqual(TMD.CMDs[i].Hash))
                    Events.NotificationEvent.Invoke(NotificationType.Warning, $"{nameof(WAD)} sha1 hash doesn't match in:'{name}'");

            }

            //Footer
            if (Header.FooterSize != 0)
            {
                source.Seek(ContentPos + Header.ContentSize, SeekOrigin.Begin);
                source.Align(64);
                Add(new FileNode("footer.bin", new SubStream(source, Header.FooterSize, CertPos)));
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        public readonly struct WADHeader
        {
            public readonly uint HeaderSize;
            public readonly Identifier32 Magic;
            public readonly uint CertSize;
            private readonly uint Reserved;
            public readonly uint TicketSize;
            public readonly uint TMDSize;
            public readonly uint ContentSize;
            public readonly uint FooterSize;

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
