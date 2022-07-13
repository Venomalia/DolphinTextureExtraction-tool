using System.IO;

namespace AFSLib
{
    /// <summary>
    /// Abstract class that represents an entry. All types of entries derive from Entry.
    /// </summary>
    public abstract class Entry
    {
        internal abstract Stream GetStream();
    }
}