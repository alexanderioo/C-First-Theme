namespace TypingTrainer.ConsoleUI;

public static class ConsoleInput
{
    public static int ReadMenuChoice(string prompt, int minimum, int maximum)
    {
        while (true)
        {
            Console.Write(prompt);
            string? value = Console.ReadLine();

            if (int.TryParse(value, out int choice) &&
                choice >= minimum &&
                choice <= maximum)
            {
                return choice;
            }

            ConsoleTheme.Error($"Введите число от {minimum} до {maximum}.");
        }
    }

    public static string ReadRequired(string prompt, string? currentValue = null)
    {
        while (true)
        {
            Console.Write(prompt);
            string? value = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            if (currentValue is not null)
            {
                return currentValue;
            }

            ConsoleTheme.Error("Значение не может быть пустым.");
        }
    }

    public static string ReadOptional(string prompt, string currentValue)
    {
        Console.Write($"{prompt} [{currentValue}]: ");
        string? value = Console.ReadLine();
        return string.IsNullOrWhiteSpace(value) ? currentValue : value.Trim();
    }

    public static bool Confirm(string prompt)
    {
        Console.Write($"{prompt} [д/Н]: ");
        string? value = Console.ReadLine()?.Trim();
        return string.Equals(value, "д", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, "y", StringComparison.OrdinalIgnoreCase);
    }

    public static int ReadNumber(string prompt, int minimum, int maximum, int currentValue)
    {
        while (true)
        {
            Console.Write($"{prompt} [{currentValue}]: ");
            string? value = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(value))
            {
                return currentValue;
            }

            if (int.TryParse(value, out int number) &&
                number >= minimum &&
                number <= maximum)
            {
                return number;
            }

            ConsoleTheme.Error($"Введите число от {minimum} до {maximum}.");
        }
    }

    public static void WaitForEnter()
    {
        Console.WriteLine();
        Console.Write("Нажмите Enter, чтобы продолжить...");
        Console.ReadLine();
    }
}
