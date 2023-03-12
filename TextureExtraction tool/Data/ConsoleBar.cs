namespace DolphinTextureExtraction
{
    public class ConsoleBar
    {

        public ConsoleColor Color { get; private set; }
        public double Max { get; private set; }
        public int Length { get; private set; }
        public double Value { get; set; } = 0;

        public int CursorTop { get; set; }
        public int CursorLeft { get; set; }

        public ConsoleBar(double max, int length = 30, ConsoleColor color = ConsoleColor.Green)
        {
            CursorTop = Console.CursorTop;
            CursorLeft = Console.CursorLeft;
            Max = max;
            Length = length;
            Color = color;
        }

        public void Print()
        {
            lock (Console.Out)
                lock (Console.Error)
                {
                    Console.SetCursorPosition(CursorLeft, CursorTop);
                    ConsoleColor color = Console.ForegroundColor;
                    Console.ForegroundColor = Color;

                    Console.Write("│");
                    float PL = (float)(Value * Length / Max);
                    if (PL >= 1) Console.Write("".PadLeft((int)PL, '█'));
                    if (PL < Length)
                    {
                        if ((PL - (int)PL) >= 0.75) Console.Write('▓');
                        else if ((PL - (int)PL) >= 0.5) Console.Write('▒');
                        else if ((PL - (int)PL) >= 0.25) Console.Write('░');
                        else Console.Write(' ');
                        Console.Write("".PadLeft(Length - (int)PL - 1, ' '));
                    }
                    Console.Write("│");
                    Console.ForegroundColor = color;
                }
        }
    }
}
