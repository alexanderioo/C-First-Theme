using TypingTrainer.Models;

namespace TypingTrainer.ConsoleUI;

public static class ConsoleTheme
{
    private static UserSettings _settings = UserSettings.Default;

    public static void Apply(UserSettings settings)
    {
        _settings = settings;

        try
        {
            Console.BackgroundColor = settings.Theme == AppTheme.Dark
                ? ConsoleColor.Black
                : ConsoleColor.White;
            Console.ForegroundColor = settings.Theme == AppTheme.Dark
                ? ConsoleColor.Gray
                : ConsoleColor.Black;
            Console.Clear();
        }
        catch (IOException)
        {
            // Некоторые перенаправленные терминалы не поддерживают цвета.
        }
    }

    public static void Clear()
    {
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            Console.WriteLine();
        }
    }

    public static void Heading(string title)
    {
        WriteColored($"=== {title.ToUpperInvariant()} ===", ConsoleColor.Cyan);
        Console.WriteLine();
    }

    public static void Success(string message) =>
        WriteColored(message, ConsoleColor.Green);

    public static void Warning(string message) =>
        WriteColored(message, ConsoleColor.Yellow);

    public static void Error(string message) =>
        WriteColored(message, ConsoleColor.Red);

    public static void Muted(string message)
    {
        Console.ForegroundColor = _settings.Theme == AppTheme.Dark
            ? ConsoleColor.DarkGray
            : ConsoleColor.DarkGray;
        Console.WriteLine(message);
        Reset();
    }

    public static void WriteColored(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Reset();
    }

    public static void Reset()
    {
        Console.BackgroundColor = _settings.Theme == AppTheme.Dark
            ? ConsoleColor.Black
            : ConsoleColor.White;
        Console.ForegroundColor = _settings.Theme == AppTheme.Dark
            ? ConsoleColor.Gray
            : ConsoleColor.Black;
    }
}
