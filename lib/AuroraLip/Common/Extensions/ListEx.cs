namespace AuroraLib.Common
{
    /*
    * Super Hackio Incorporated
    * "Copyright © Super Hackio Incorporated 2020-2021"
    * https://github.com/SuperHackio/Hack.io
    */

    /// <summary>
    /// Extra List functions
    /// </summary>
    public static class ListEx
    {
        /// <summary>
        /// Finds out if a sequence exists in a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence"></param>
        /// <param name="subsequence"></param>
        /// <returns></returns>
        public static bool ContainsSubsequence<T>(this IList<T> sequence, IList<T> subsequence)
        {
            if (sequence.Count == 0 || subsequence.Count > sequence.Count)
                return false;
            var yee = Enumerable.Range(0, sequence.Count - subsequence.Count + 1).Any(n => sequence.Skip(n).Take(subsequence.Count).SequenceEqual(subsequence));
            return yee;
        }

        /// <summary>
        /// Finds a list inside a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="start">The index to Start searching from</param>
        /// <param name="sublist">The list to find</param>
        /// <returns></returns>
        public static int SubListIndex<T>(this IList<T> list, int start, IList<T> sublist)
        {
            for (int listIndex = start; listIndex < list.Count - sublist.Count + 1; listIndex++)
            {
                int count = 0;
                while (count < sublist.Count && sublist[count].Equals(list[listIndex + count]))
                    count++;
                if (count == sublist.Count)
                    return listIndex;
            }
            return -1;
        }

        /// <summary>
        /// Moves an item X distance in a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="OldIndex">The original index of the item</param>
        /// <param name="NewIndex">The new index of the item</param>
        /// <returns></returns>
        public static void Move<T>(this IList<T> list, int OldIndex, int NewIndex)
        {
            T item = list[OldIndex];
            list.RemoveAt(OldIndex);
            list.Insert(NewIndex, item);
        }

        /// <summary>
        /// Sort a list of items based on an array of items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="OriginalList"></param>
        /// <param name="sortref">The list to reference while sorting</param>
        /// <returns></returns>
        public static List<T> SortBy<T>(this List<T> OriginalList, T[] sortref)
        {
            List<T> FinalList = new List<T>();

            for (int i = 0; i < sortref.Length; i++)
                if (OriginalList.Contains(sortref[i]))
                    FinalList.Add(sortref[i]);

            return FinalList;
        }

        /// <summary>
        /// Compares the contents of two lists to see if they match
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool Equals<T>(this List<T> left, List<T> right)
        {
            if (left.Count != right.Count)
                return false;

            for (int i = 0; i < left.Count; i++)
                if (!left[i].Equals(right[i]))
                    return false;
            return true;
        }

        /// <summary>
        /// Compares the contents of the two lists using a custom function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="comparefunc"></param>
        /// <returns></returns>
        public static bool Equals<T>(List<T> left, List<T> right, Func<T, T, bool> comparefunc)
        {
            if (left.Count != right.Count)
                return false;

            for (int i = 0; i < left.Count; i++)
                if (!comparefunc(left[i], right[i]))
                    return false;
            return true;
        }

        /// <summary>
        /// Gets a hash code from a List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="starting"></param>
        /// <param name="additive"></param>
        /// <returns></returns>
        public static int GetHashCode<T>(List<T> list, int starting = 0, int additive = 1)
        {
            int hashcode = starting;
            for (int i = 0; i < list.Count; i++)
            {
                hashcode = hashcode * additive + list[i].GetHashCode();
            }
            return hashcode;
        }
    }
}
