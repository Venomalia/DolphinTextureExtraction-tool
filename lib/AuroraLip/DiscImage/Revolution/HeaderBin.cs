using AuroraLib.DiscImage.Dolphin;

namespace AuroraLib.DiscImage.Revolution
{
    public class HeaderBin : GameHeader
    {
        /// <summary>
        /// 'False' don't work on retail consoles
        /// </summary>
        public bool UseVerification;

        /// <summary>
        /// 'False' don't work on retail consoles
        /// </summary>
        public bool UseEncryption;

        public HeaderBin(Stream source) : base(source)
        { }

        protected override void ReadData(Stream dest)
        {
            GameName = dest.ReadString(64);
            UseVerification = dest.ReadUInt8() == 0;
            UseEncryption = dest.ReadUInt8() == 0;
        }

        protected override void WriteData(Stream dest)
        {
            dest.WriteString(GameName, 64, 0);
            dest.WriteByte((byte)(UseVerification ? 0 : 1));
            dest.WriteByte((byte)(UseEncryption ? 0 : 1));
        }
    }
}
