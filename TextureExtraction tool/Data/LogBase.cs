using System.IO;

namespace DolphinTextureExtraction_tool
{
    public class LogBase : StreamWriter
    {
        private static readonly object Lock = new object();

        public string FullPath { get; private set; }

        public LogBase(string FullPath) : this(FullPath, false) { }

        public LogBase(string FullPath, bool append) : base(FullPath, append)
        {
            this.FullPath = FullPath;
        }
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
    }
}
