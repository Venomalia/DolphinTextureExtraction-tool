namespace AuroraLib.DiscImage.RVZ
{
    public readonly struct RvzVersion : IComparable<RvzVersion>
    {
        public readonly byte Major;
        public readonly byte Minor;
        public readonly byte Build;
        private readonly byte beta;

        public RvzVersion(byte major, byte minor = 0, byte build = 0, byte beta = 0)
        {
            Major = major;
            Minor = minor;
            Build = build;
            this.beta = beta;
        }

        public readonly bool IsBeta => beta != 0x00 && beta != 0xff;

        public override string ToString()
        {
            string versionString = Build == 0 ? $"{Major}.{Minor}" : $"{Major}.{Minor}.{Build}";
            if (IsBeta)
            {
                versionString += $" beta {beta}";
            }
            return versionString;
        }

        public unsafe int CompareTo(RvzVersion other)
        {
            if (Major != other.Major)
                return Major.CompareTo(other.Major);
            if (Minor != other.Minor)
                return Minor.CompareTo(other.Minor);
            if (Build != other.Build)
                return Build.CompareTo(other.Build);
            return beta.CompareTo(other.beta);
        }
    }
}
