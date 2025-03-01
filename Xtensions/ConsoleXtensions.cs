namespace Bronya.Xtensions
{
    public static class ConsoleXtensions
    {
        public static void ColoredPrint(string text, ConsoleColor consoleColor)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
