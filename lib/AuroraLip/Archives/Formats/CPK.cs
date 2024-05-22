using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;
using System.Text;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// CRIWARE Compact Archive
    /// </summary>
    public sealed class CPK : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("CPK ");

        public readonly LibCPK.CPK CpkContent = new();

        public CPK()
        {
        }

        public CPK(string name) : base(name)
        {
        }

        public CPK(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            CpkContent.ReadCPK(source, Encoding.UTF8);

            foreach (var entrie in CpkContent.fileTable)
            {
                if (entrie.FileType != LibCPK.FileTypeFlag.FILE)
                    continue;

                DirectoryNode dir;
                if (String.IsNullOrWhiteSpace(entrie.DirName))
                {
                    dir = this;
                }
                else
                {
                    if (TryGet(entrie.DirName, out ObjectNode objectNode))
                    {
                        dir = (DirectoryNode)objectNode;
                    }
                    else
                    {
                        dir = new DirectoryNode(Path.GetFileName(entrie.DirName));
                        AddPath(entrie.DirName, dir);
                    }
                }

                // important files are available multiple times.
                FileNode file = new(entrie.FileName, new SubStream(source, UInt32.Parse(entrie.FileSize.ToString()), (long)entrie.FileOffset));
                if (!dir.TryAdd(file))
                {
                    file.Dispose();
                }
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();
    }
}
