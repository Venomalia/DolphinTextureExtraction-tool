namespace DolphinTextureExtraction
{
    public class LogBase : StreamWriter
    {
        protected static readonly object Lock = new();

        public string FullPath { get; private set; }

        public LogBase(string FullPath) : base(FullPath, new FileStreamOptions() { Access = FileAccess.Write, Mode = FileMode.Create, Share = FileShare.Read })
        { }

        public LogBase(string FullPath, bool append) : base(FullPath, append)
            => this.FullPath = FullPath;

        /// <summary>
        /// Convert a thread's id to a base 1 index, increasing in increments of 1 (Makes logs prettier)
        /// </summary>
        protected int ThreadIndex
        {
            get
            {
                int managed = Environment.CurrentManagedThreadId;
                if (!ThreadIndices.TryGetValue(managed, out int id))
                    ThreadIndices.Add(managed, id = ThreadIndices.Count + 1);

                return id;
            }
        }
        private readonly Dictionary<int, int> ThreadIndices = new();

        public override void WriteLine()
        {
            lock (Lock)
            {
                base.WriteLine();
            }
        }

        public override void WriteLine(string value)
        {
            lock (Lock)
            {
                base.WriteLine(value);
            }
        }

        public override void Write(string value)
        {
            lock (Lock)
            {
                base.Write(value);
            }
        }

        public override void Write(ReadOnlySpan<char> value)
        {
            lock (Lock)
            {
                base.Write(value);
            }
        }

        public override void Write(char[] value)
        {
            lock (Lock)
            {
                base.Write(value);
            }
        }
    }
}
