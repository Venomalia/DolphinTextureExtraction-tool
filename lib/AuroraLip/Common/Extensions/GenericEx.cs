namespace AuroraLip.Common
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
        /// Swaps two values using a Tuple
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Values">The tuple to swap values of</param>
        public static void SwapValues<T>(ref Tuple<T, T> Values)
            => Values = new Tuple<T, T>(Values.Item2, Values.Item1);

        /// <summary>
        /// Determines whether two Arrays are equal by comparing the elements by using the default equality comparer for their type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="other"></param>
        /// <returns>"true" if the two source sequences are of equal</returns>
        public static bool ArrayEqual<T>(this T[] values, T[] other)
        {
            if (values.Length != other.Length)
                return false;

            for (int i = 0; i < values.Length; i++)
            {
                if (!values[i].Equals(other[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// searches for a specific pattern.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="pattern">the pattern to search for</param>
        /// <param name="start"></param>
        /// <returns>the position where the pattern was found or -1 if none was found.</returns>
        public static int SequenceSearch<T>(this T[] values, T[] pattern, int start = 0)
        {
            int len = pattern.Length;
            for (int i = start; i <= values.Length - len; i++)
            {
                int p;
                for (p = 0; p < len; p++)
                    if (!pattern[p].Equals(values[i + p])) break;
                if (p == len) return i - pattern.Length;
            }
            return -1;
        }
    }
}
