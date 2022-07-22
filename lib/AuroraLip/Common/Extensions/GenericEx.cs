using System;

namespace AuroraLip.Common
{

    /*
    * Super Hackio Incorporated
    * "Copyright © Super Hackio Incorporated 2020-2021"
    * https://github.com/SuperHackio/Hack.io
    */

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
        public static void SwapValues<T>(ref T Left, ref T Right)
        {
            T temp = Left;
            Left = Right;
            Right = temp;
        }

        /// <summary>
        /// Swaps two values using a Tuple
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Values">The tuple to swap values of</param>
        public static void SwapValues<T>(ref Tuple<T, T> Values)
            => Values = new Tuple<T, T>(Values.Item2, Values.Item1);

        //public static bool ArrayEqual<T>(this T[] values, T[] other) => values.SequenceEqual(other);

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
    }
}