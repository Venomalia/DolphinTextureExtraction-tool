using System.IO;

namespace AFSLib
{
    /// <summary>
    /// Class that represents an empty entry with no data.
    /// </summary>
    public class NullEntry : Entry
    {
        internal NullEntry()
        {

        }

        internal override Stream GetStream()
        {
            return null;
        }
    }
}