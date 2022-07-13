using System.IO;

namespace AFSLib
{
    /// <summary>
    /// Class that represents an entry with data referenced from a stream.
    /// </summary>
    public sealed class StreamEntry : DataEntry
    {
        private readonly Stream baseStream;
        private readonly uint baseStreamDataOffset;

        internal StreamEntry(Stream baseStream, StreamEntryInfo info)
        {
            this.baseStream = baseStream;
            baseStreamDataOffset = info.Offset;

            Name = info.Name;
            Size = info.Size;
            LastWriteTime = info.LastWriteTime;
            UnknownAttribute = info.UnknownAttribute;
        }

        internal override Stream GetStream()
        {
            baseStream.Position = baseStreamDataOffset;
            return new SubStream(baseStream, 0, Size, true);
        }

        public Stream GetSubStream() => GetStream();
    }
}