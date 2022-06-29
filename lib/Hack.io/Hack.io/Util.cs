using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Hack.ConsoleEx
{
    /// <summary>
    /// Extension of the console
    /// </summary>
    public static class ConsoleEx
    {
        /// <summary>
        /// Checks to see if the user presses certain keys
        /// </summary>
        /// <param name="YesKey">The key to indicate "Yes"</param>
        /// <param name="NoKey">The key to indicate "No"</param>
        /// <returns>true if the user presses the YesKey</returns>
        public static bool Confirm(ConsoleKey YesKey = ConsoleKey.Y, ConsoleKey NoKey = ConsoleKey.N)
        {
            ConsoleKey response;
            Console.WriteLine();
            do
            {
                response = Console.ReadKey(false).Key;
            } while (response != YesKey && response != NoKey);

            return (response == YesKey);
        }
        /// <summary>
        /// Writes a coloured message to the console
        /// </summary>
        /// <param name="message">Message to print</param>
        /// <param name="ForeColour">ConsoleColor to use for the text</param>
        /// <param name="BackColour">ConsoleColor to use for the background of the text</param>
        /// <param name="newline">Switch to the next line?</param>
        public static void WriteColoured(string message, ConsoleColor ForeColour = ConsoleColor.White, ConsoleColor BackColour = ConsoleColor.Black, bool newline = false)
        {
            Console.BackgroundColor = BackColour;
            Console.ForegroundColor = ForeColour;
            if (newline)
                Console.WriteLine(message);
            else
                Console.Write(message);
            Console.ResetColor();
        }
        /// <summary>
        /// Quits the Console with a given error message
        /// </summary>
        /// <param name="ExitCode">Code to exit with.</param>
        /// <param name="ErrorMsg">The error message</param>
        public static void Quit(int ExitCode = 0, string ErrorMsg = "An error has occured")
        {
            System.Console.WriteLine();
            WriteColoured($"ERROR: {ErrorMsg}", ConsoleColor.Red, newline: true);
            WriteColoured($"[Error Code 0x{ExitCode.ToString().PadLeft(4, '0')}]", ConsoleColor.DarkRed);
            System.Console.WriteLine();
            System.Console.WriteLine();
            System.Console.WriteLine("Press any key to exit");
            System.Console.ReadKey();
            Environment.Exit(ExitCode);
        }
    }
}

namespace Hack.io.Util
{
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
        public static void Move<T>(this IList<T> list,int OldIndex, int NewIndex)
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
    /// <summary>
    /// Extra String functions
    /// </summary>
    public static class StringEx
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string WildCardToRegular(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("message", nameof(value));

            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }
    }
    /// <summary>
    /// Extra FileInfo functions
    /// </summary>
    public static class FileInfoEx
    {
        /// <summary>
        /// Check if a file cannot be opened.
        /// </summary>
        /// <param name="file">File to check for</param>
        /// <returns>If the file is locked, returns true.</returns>
        [DebuggerStepThrough]
        public static bool IsFileLocked(this FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Dir"></param>
        [DebuggerStepThrough]
        public static void CreateDirectoryIfNotExist(string Dir)
        {
            if (!Directory.Exists(Dir))
                Directory.CreateDirectory(Dir);
        }
    }
    /// <summary>
    /// Extra BitArray functions
    /// </summary>
    public static class BitArrayEx
    {
        /// <summary>
        /// Converts this BitArray to an Int32
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int ToInt32(this BitArray array)
        {
            if (array.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");

            int[] Finalarray = new int[1];
            array.CopyTo(Finalarray, 0);
            return Finalarray[0];
        }
    }
    /// <summary>
    /// Extra BitConverter functions
    /// </summary>
    public static class BitConverterEx
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="StartIndex"></param>
        /// <returns></returns>
        public static Int24 ToInt24(byte[] value, int StartIndex) => new Int24(value[StartIndex] | value[StartIndex + 1] << 8 | value[StartIndex + 2] << 16);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(Int24 value) => new byte[3] { (byte)value.Value, (byte)(value.Value >> 8), (byte)(value.Value >> 16) };
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="StartIndex"></param>
        /// <returns></returns>
        public static UInt24 ToUInt24(byte[] value, int StartIndex) => new UInt24((uint)(value[StartIndex] | value[StartIndex + 1] << 8 | value[StartIndex + 2] << 16));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(UInt24 value) => new byte[3] { (byte)value.Value, (byte)(value.Value >> 8), (byte)(value.Value >> 16) };
    }
    /// <summary>
    /// 
    /// </summary>
    public static class Benchmark
    {
        private static DateTime startDate = DateTime.MinValue;
        private static DateTime endDate = DateTime.MinValue;
        /// <summary>
        /// 
        /// </summary>
        public static TimeSpan Span => endDate.Subtract(startDate);
        /// <summary>
        /// Starts the timer
        /// </summary>
        public static void Start() => startDate = DateTime.Now;
        /// <summary>
        /// Ends the Timer
        /// </summary>
        public static void End() => endDate = DateTime.Now;
        /// <summary>
        /// Gets the elapsed seconds
        /// </summary>
        /// <returns></returns>
        public static double GetSeconds() => endDate == DateTime.MinValue ? 0.0 : Span.TotalSeconds;
    }
    /// <summary>
    /// Extra Encoding functions
    /// </summary>
    public static class EncodingEx
    {
        /// <summary>
        /// Gets the amount of bytes this Encoding uses
        /// </summary>
        /// <param name="enc"></param>
        /// <returns></returns>
        public static int GetStride(this Encoding enc) => enc.GetMaxByteCount(0);
    }
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
    }
    /// <summary>
    /// 
    /// </summary>
    public static class BitmapEx
    {
        /// <summary>
        /// Converts a bitmap to a byte[]
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(this Bitmap bitmap)
        {
            BitmapData bmpdata = null;
            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }

        }
        /// <summary>
        /// Creates a bitmap from a byte[] (TODO: TEST THIS)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="stride"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static Bitmap FromByteArray(byte[] data, int width, int height, int stride, PixelFormat format)
        {
            Bitmap result = new Bitmap(width, height, format);
            BitmapData d = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.ReadWrite, format);
            Marshal.Copy(data, 0, d.Scan0, data.Length);
            result.UnlockBits(d);
            return result;
        }
        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <param name="InterpolationMode"></param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height, InterpolationMode InterpolationMode = InterpolationMode.HighQualityBicubic)
        {
            Rectangle destRect = new Rectangle(0, 0, width, height);
            Bitmap destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (Graphics graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (ImageAttributes wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
        /// <summary>
        /// Generates a lerped bitmap. Useful for mipmapping
        /// </summary>
        /// <param name="imageA"></param>
        /// <param name="imageB"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="t"></param>
        /// <param name="interpolationMode"></param>
        /// <returns></returns>
        public static Bitmap Lerp(Bitmap imageA, Bitmap imageB, int width, int height, float t, InterpolationMode interpolationMode = InterpolationMode.Bilinear)
        {
            t = t.Clamp(0, 1);
            if (imageA.PixelFormat != imageB.PixelFormat)
                throw new Exception("Pixel format mis-match!");
            
            Bitmap imageARescale = ResizeImage(imageA, width, height, interpolationMode);
            Bitmap imageBRescale = ResizeImage(imageB, width, height, interpolationMode);
            
            byte[] imageAbytes = imageARescale.ToByteArray();
            byte[] imageBbytes = imageBRescale.ToByteArray();
            byte[] resultbytes = new byte[imageAbytes.Length];

            for (int i = 0; i < imageAbytes.Length; i++)
            {
                resultbytes[i] = MathEx.Lerp(imageAbytes[i], imageBbytes[i], t);
            }
            imageARescale.Dispose();
            imageBRescale.Dispose();
            return FromByteArray(resultbytes, width, height, 4, imageA.PixelFormat);
        }
    }
    /// <summary>
    /// Class full of odds and ends that don't belong to a certain group
    /// </summary>
    public static class GenericExtensions
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
        public static void SwapValues<T>(ref Tuple<T, T> Values) => Values = new Tuple<T, T>(Values.Item2, Values.Item1);
        /// <summary>
        /// Cycles an set of objects. Can be cycled backwards
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Reverse">If true, cycles from left to right instead of right to left</param>
        /// <param name="Values">The values to cycle</param>
        public static void CycleValues<T>(bool Reverse = false, params T[] Values)
        {
            T temp;
            if (Reverse)
            {
                temp = Values[Values.Length - 1];
                for (int i = Values.Length - 1; i >= 1; i--)
                    Values[i + 1] = Values[i];
                Values[0] = temp;
            }
            else
            {
                temp = Values[0];
                for (int i = 1; i < Values.Length; i++)
                    Values[i - 1] = Values[i];
                Values[Values.Length - 1] = temp;
            }
        }

        /// <summary>
        /// Compares the contents of two dictionaries
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool Equals<TKey, TValue>(Dictionary<TKey, TValue> left, Dictionary<TKey, TValue> right)
        {
            if (left.Count != right.Count)
                return false;

            bool IsEqual = true;
            foreach (KeyValuePair<TKey, TValue> pair in left)
            {
                if (right.TryGetValue(pair.Key, out TValue value))
                {
                    // Require value be equal.
                    if (!value.Equals(pair.Value))
                    {
                        IsEqual = false;
                        break;
                    }
                }
                else
                {
                    // Require key be present.
                    IsEqual = false;
                    break;
                }
            }
            return IsEqual;
        }

        /// <summary>
        /// Multiplies both width and height by the same number
        /// </summary>
        /// <param name="size"></param>
        /// <param name="scale"></param>
        public static void Multiply(this Size size, int scale)
        {
            size.Width *= scale;
            size.Height *= scale;
        }
        /// <summary>
        /// Divides both width and height by the same number
        /// </summary>
        /// <param name="size"></param>
        /// <param name="scale"></param>
        public static void Divide(this Size size, int scale)
        {
            size.Width /= scale;
            size.Height /= scale;
        }
        /// <summary>
        /// Creates a new ICollection (Such as a <see cref="List{T}"/>) with a specified value for every entry
        /// </summary>
        /// <typeparam name="Class"></typeparam>
        /// <typeparam name="Type"></typeparam>
        /// <param name="InitValue"></param>
        /// <param name="Size"></param>
        /// <returns></returns>
        public static Class NewICollection<Class, Type>(Type InitValue, int Size) where Class : ICollection<Type>, new()
        {
            Class output = new Class();
            for (int i = 0; i < Size; i++)
            {
                output.Add(InitValue);
            }
            return output;
        }
    }
}