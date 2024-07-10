using System;

namespace PayrollEngine.SqlServer.DbQuery;

internal class CommandBase
{
    protected static void WriteTitleLine(string text) =>
        WriteColorLine(text, ConsoleColor.Cyan);

    protected static void WriteSuccessLine(string text) =>
        WriteColorLine(text, ConsoleColor.Green);

    protected static void WriteErrorLine(string text) =>
        WriteColorLine(text, ConsoleColor.Red);

    private static void WriteColorLine(string text, ConsoleColor color)
    {
        var foregroundColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = foregroundColor;
    }

    protected static void Wait()
    {
        Console.WriteLine();
        Console.Write("Press any key...");
        Console.ReadKey(true);
    }
}