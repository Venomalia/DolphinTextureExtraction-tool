using System;

namespace AuroraLip.Common
{

    /*
    * Super Hackio Incorporated
    * "Copyright © Super Hackio Incorporated 2020-2021"
    * https://github.com/SuperHackio/Hack.io
    */

    /// <summary>
    /// Extra math functions
    /// </summary>
    public static class MathEx
    {
        /// <summary>
        /// Clamps a value to the specified minimum and maximum value
        /// </summary>
        /// <typeparam name="T">IComparable</typeparam>
        /// <param name="val">The value to clamp</param>
        /// <param name="min">Minimum value to clamp to</param>
        /// <param name="max">Maximum value to clamp to</param>
        /// <returns>Max or Min, depending on Val</returns>
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
        /// <summary>
        /// Lerp 2 bytes via a time
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static byte Lerp(byte min, byte max, float t) => (byte)(((1 - t) * min) + (t * max)).Clamp(0, 255);
        /// <summary>
        /// Lerp 2 floats via a time
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static float Lerp(float min, float max, float t) => ((1 - t) * min) + (t * max);
        /// <summary>
        /// Lerp 2 floats via a time
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double Lerp(double min, double max, double t) => ((1 - t) * min) + (t * max);

        /// <summary>
        /// Gets the percent value of a given number. Usually used by Background Workers
        /// </summary>
        /// <param name="Current"></param>
        /// <param name="Max"></param>
        /// <param name="OutOf"></param>
        /// <returns></returns>
        public static float GetPercentOf(float Current, float Max, float OutOf = 100f) => Current / Max * OutOf;
        /// <summary>
        /// Scales a number between W and X to be between Y and Z
        /// </summary>
        /// <param name="valueIn"></param>
        /// <param name="baseMin"></param>
        /// <param name="baseMax"></param>
        /// <param name="limitMin"></param>
        /// <param name="limitMax"></param>
        /// <returns></returns>
        public static double Scale(double valueIn, double baseMin, double baseMax, double limitMin, double limitMax) => ((limitMax - limitMin) * (valueIn - baseMin) / (baseMax - baseMin)) + limitMin;
        /// <summary>
        /// Returns the decimal part of a number
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static double GetDecimal(double number) => (int)((decimal)number % 1 * 100);
        /// <summary>
        /// converts the size in bits to the appropriate size adds the corresponding extension
        /// </summary>
        /// <param name="value">A long that indicates the size in bytes.</param>
        /// <param name="decimalPlaces">The number of decimal places in the return value.</param>
        /// <returns></returns>
        public static string SizeSuffix(long value, int decimalPlaces = 0)
        {
            int ex = (int)Math.Max(0, Math.Log(value, 1024));
            double adjustedSize = Math.Round(value / Math.Pow(1024, ex), decimalPlaces);
            return adjustedSize + SizeSuffixes[ex];
        }
        private static readonly string[] SizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

        /// <summary>
        /// Rounds a double-precision floating-point value to an integer, and rounds midpoint values to the nearest even number
        /// </summary>
        /// <param name="value">A double-precision floating-point number to be rounded</param>
        /// <returns>The whole number nearest to value</returns>
        public static int RoundToInt(double value) => (int)Math.Round(value, MidpointRounding.ToEven);

        /// <summary>
        /// Rounds a double-precision floating-point value to an integer, and rounds midpoint values to the specified mode
        /// </summary>
        /// <param name="value">A double-precision floating-point number to be rounded</param>
        /// <param name="mode">Specification for how to round value if it is midway between two other numbers</param>
        /// <returns>The whole number nearest to value</returns>
        public static int RoundToInt(double value, MidpointRounding mode) => (int)Math.Round(value, mode);
    }
}
