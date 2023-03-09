using AuroraLip.Common;

namespace AuroraLip.Archives.DiscImage
{
    /// <summary>
    /// consist of 6 Chars(byte) and is composed of the SystemCode, GameCode, RegionCode and MakerCode.
    /// </summary>
    public partial struct GameID
    {
        public byte[] Value
        {
            get
            {
                byte[] value = new byte[6];
                value[0] = systemCode;
                value[1] = gameCode0;
                value[2] = gameCode1;
                value[3] = regionCode;
                value[4] = makercode0;
                value[5] = makercode1;
                return value;
            }
        }

        public SystemCode SystemCode { get => (SystemCode)systemCode; set => systemCode = (byte)value; }
        private byte systemCode;


        public char[] GameCode
        {
            get => new char[] { (char)gameCode0, (char)gameCode1 };
            set
            {
                if (value.Length != 2)
                    throw new ArgumentException($"A {nameof(GameCode)} must consist of 2 characters");
                gameCode0 = (byte)value[0];
                gameCode1 = (byte)value[1];
            }
        }
        private byte gameCode0;
        private byte gameCode1;

        public RegionCode RegionCode { get => (RegionCode)regionCode; set => regionCode = (byte)value; }
        private byte regionCode;

        public char[] MakerCode
        {
            get => new char[] { (char)makercode0, (char)makercode1 };
            set
            {
                if (value.Length != 2)
                    throw new ArgumentException($"A {nameof(MakerCode)} must consist of 2 characters");
                makercode0 = (byte)value[0];
                makercode1 = (byte)value[1];
            }
        }
        private byte makercode0;
        private byte makercode1;

        public GameID(ReadOnlySpan<char> Value)
        {
            if (Value.Length != 6)
                throw new ArgumentException($"A {nameof(GameID)} must consist of 6 characters");

            systemCode = (byte)Value[0];
            gameCode0 = (byte)Value[1];
            gameCode1 = (byte)Value[2];
            regionCode = (byte)Value[3];
            makercode0 = (byte)Value[4];
            makercode1 = (byte)Value[5];
        }
        public GameID(in byte[] Value)
        {
            if (Value.Length != 6)
                throw new ArgumentException($"A {nameof(GameID)} must consist of 6 characters");

            systemCode = Value[0];
            gameCode0 = Value[1];
            gameCode1 = Value[2];
            regionCode = Value[3];
            makercode0 = Value[4];
            makercode1 = Value[5];
        }

        public string GetMaker()
        {
            MakerCodes.TryGetValue(new string(MakerCode), out string maker);
            return maker;
        }

        public static bool operator ==(GameID l, GameID r) => l.Value == r.Value;
        public static bool operator !=(GameID l, GameID r) => l.Value == r.Value;
        public override int GetHashCode() => Value.GetHashCode();
        public override bool Equals(object obj) => obj is GameID ID && ID.Value == Value;
        public override string ToString() => Value.ToString();

        public static explicit operator GameID(in byte[] x) => new GameID(x);
        public static explicit operator byte[](GameID x) => x.Value;
        public static explicit operator GameID(string x) => new GameID(x.AsSpan());
        public static explicit operator GameID(ReadOnlySpan<char> x) => new GameID(x);
        public static explicit operator string(GameID x) => x.Value.ToValidString();
    }
}
