namespace AuroraLib.Common
{
    /// <summary>
    /// Class full of odds and ends that don't belong to a certain group
    /// </summary>
    public static class GenericEx
    {
        /// <summary>
        /// Swaps two values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Left">Our first contestant</param>
        /// <param name="Right">Our second contestant</param>
        public static void SwapValues<T>(ref this T Left, ref T Right) where T : struct
            => (Right, Left) = (Left, Right);

        /// <summary>
        /// Searches for a sequence within an <see cref="ReadOnlySpan{T}"/> and returns the index of the first occurrence.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="values">The array to search in.</param>
        /// <param name="pattern">The sequence to search for.</param>
        /// <param name="start">The starting index for the search. Default is 0.</param>
        /// <returns>The index of the first occurrence of the sequence, or -1 if not found.</returns>
        public static int SequenceSearch<T>(this ReadOnlySpan<T> values, ReadOnlySpan<T> pattern, int start = 0)
        {
            int len = pattern.Length;
            int maxIndex = values.Length - len;

            for (int i = start; i <= maxIndex; i++)
            {
                int p;
                for (p = 0; p < len; p++)
                {
                    if (!pattern[p].Equals(values[i + p]))
                        break;
                }

                if (p == len)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Calculates the difference between two <see cref="ReadOnlySpan{byte}"/> instances.
        /// </summary>
        /// <param name="span1">The first ReadOnlySpan instance to compare</param>
        /// <param name="span2">The second ReadOnlySpan instance to compare.</param>
        /// <returns>A float value representing the difference</returns>
        public static float Compare(this ReadOnlySpan<byte> span1, ReadOnlySpan<byte> span2)
        {
            if (span1.Length != span2.Length)
            {
                return float.MaxValue;
            }

            int diff = 0;

            for (int i = 0; i < span1.Length; i++)
            {
                diff += Math.Abs(span1[i] - span2[i]);
            }

            return (float)diff / (float)span1.Length;
        }
    }
}
