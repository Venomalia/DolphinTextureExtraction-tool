using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
{
    public class CTM_FC : ARC_FS
    {
        public override bool CanWrite => false;

        public new const string Extension = ".ctm";

        public override bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 0x20 && Extension == extension.ToLower() && stream.ReadUInt32(Endian.Big) == stream.Length;

        protected override void Read(Stream stream)
        {
            Root = new ArchiveDirectory() { OwnerArchive = this };
            ArchiveDirectory texturs = new(this, Root) { Name = "Textures" };
            Root.Items.Add(texturs.Name, texturs);
            ArchiveDirectory helper = new();
            Process(stream, helper);
            ArchiveObject modeldata = helper.Items.Values.Last();
            modeldata.Name = "Model";
            Root.Items.Add(modeldata.Name, modeldata);
            Process(((ArchiveFile)helper.Items.Values.First()).FileData, texturs);
        }
    }
}
