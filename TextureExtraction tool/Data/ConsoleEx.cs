using System;

namespace DolphinTextureExtraction_tool
{
    /// <summary>
    /// Extension of the console
    /// </summary>
    public static class ConsoleEx
    {
        /// <summary>
        /// Checks to see if the user presses certain keys null requires an input
        /// </summary>
        /// <param name="defaultvalue">when the user presses enter, null requires an input</param>
        /// <param name="Yes">Yes ConsoleKey</param>
        /// <param name="No">No ConsoleKey</param>
        /// <returns></returns>
        public static bool ReadBool(bool? defaultvalue = null, ConsoleKey Yes = ConsoleKey.Y, ConsoleKey No = ConsoleKey.N)
        {
            ConsoleKey response;
            do response = Console.ReadKey(true).Key;
            while ((defaultvalue == null || response != ConsoleKey.Enter) && response != Yes && response != No);
            if (response == ConsoleKey.Enter) return defaultvalue == true;
            return (response == Yes);
        }

        public static bool WriteBoolPrint(in bool value, ConsoleColor YesColour = ConsoleColor.White, ConsoleColor NoColour = ConsoleColor.White) => WriteBoolPrint(value,true.ToString(),false.ToString(), YesColour, NoColour);


        public static bool WriteBoolPrint(in bool value, in string Yes, in string No, ConsoleColor YesColour = ConsoleColor.White, ConsoleColor NoColour = ConsoleColor.White)
        {
            switch (value)
            {
                case true:
                    WriteLineColoured(Yes, YesColour, Console.BackgroundColor);
                    break;
                case false:
                    WriteLineColoured(No, NoColour, Console.BackgroundColor);
                    break;
            }
            return value;
        }

        /// <summary>
        /// Writes a coloured message to the console
        /// </summary>
        /// <param name="message">Message to print</param>
        /// <param name="ForeColour">ConsoleColor to use for the text</param>
        /// <param name="BackColour">ConsoleColor to use for the background of the text</param>
        public static void WriteColoured(string message, ConsoleColor ForeColour = ConsoleColor.White, ConsoleColor BackColour = ConsoleColor.Black)
        {
            Console.BackgroundColor = BackColour;
            Console.ForegroundColor = ForeColour;
            Console.Write(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Writes a line colored message to the console
        /// </summary>
        /// <param name="message">Message to print</param>
        /// <param name="ForeColour">ConsoleColor to use for the text</param>
        /// <param name="BackColour">ConsoleColor to use for the background of the text</param>
        public static void WriteLineColoured(string message, ConsoleColor ForeColour = ConsoleColor.White, ConsoleColor BackColour = ConsoleColor.Black)
        {
            Console.BackgroundColor = BackColour;
            Console.ForegroundColor = ForeColour;
            Console.WriteLine(message);
            Console.ResetColor();
        }

    }
}
