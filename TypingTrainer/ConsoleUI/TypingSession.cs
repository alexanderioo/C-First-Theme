using System.Diagnostics;
using TypingTrainer.Models;

namespace TypingTrainer.ConsoleUI;

public sealed class TypingSession
{
    public RaceResult? Run(TypingDictionary dictionary, UserSettings settings)
    {
        string targetText = ChooseTargetText(dictionary);
        if (string.IsNullOrWhiteSpace(targetText))
        {
            ConsoleTheme.Error("В выбранном словаре нет текстов.");
            return null;
        }

        ShowPreparation(dictionary, targetText, settings);

        ConsoleKeyInfo startKey;
        do
        {
            startKey = Console.ReadKey(intercept: true);
            if (startKey.Key == ConsoleKey.Escape)
            {
                return null;
            }
        }
        while (startKey.Key != ConsoleKey.Enter);

        RunCountdown(settings.CountdownSeconds);

        var typedCharacters = new List<char>(targetText.Length);
        int mistakes = 0;
        int keyPresses = 0;
        var stopwatch = Stopwatch.StartNew();

        while (true)
        {
            Render(dictionary, targetText, typedCharacters, mistakes, keyPresses, stopwatch.Elapsed, settings);

            if (IsComplete(targetText, typedCharacters))
            {
                break;
            }

            ConsoleKeyInfo key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Escape)
            {
                return null;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (typedCharacters.Count > 0)
                {
                    typedCharacters.RemoveAt(typedCharacters.Count - 1);
                }

                continue;
            }

            if (char.IsControl(key.KeyChar) || typedCharacters.Count >= targetText.Length)
            {
                continue;
            }

            char expectedCharacter = targetText[typedCharacters.Count];
            typedCharacters.Add(key.KeyChar);
            keyPresses++;

            if (key.KeyChar != expectedCharacter)
            {
                mistakes++;
            }
        }

        stopwatch.Stop();
        RaceResult result = RaceResult.Create(
            dictionary,
            targetText.Length,
            stopwatch.Elapsed,
            mistakes,
            keyPresses);

        ShowResult(result, dictionary);
        return result;
    }

    private static void ShowPreparation(
        TypingDictionary dictionary,
        string targetText,
        UserSettings settings)
    {
        ConsoleTheme.Clear();
        ConsoleTheme.Heading("Тренировка");
        Console.WriteLine($"Словарь: {dictionary.Name}");
        Console.WriteLine("Наберите текст символ в символ.");
        Console.WriteLine("Backspace — исправить, Esc — прервать тренировку.");
        Console.WriteLine();
        WriteWrapped(targetText, settings.TextWidth);
        Console.WriteLine();
        ConsoleTheme.Warning("Нажмите Enter, когда будете готовы...");
    }

    private static void RunCountdown(int seconds)
    {
        for (int value = seconds; value > 0; value--)
        {
            ConsoleTheme.Clear();
            ConsoleTheme.Heading("Старт через");
            ConsoleTheme.Warning(value.ToString());
            Thread.Sleep(1_000);
        }
    }

    private static void Render(
        TypingDictionary dictionary,
        string targetText,
        IReadOnlyList<char> typedCharacters,
        int mistakes,
        int keyPresses,
        TimeSpan elapsed,
        UserSettings settings)
    {
        ConsoleTheme.Clear();
        ConsoleTheme.Heading("Тренировка");
        Console.WriteLine($"Словарь: {dictionary.Name}");
        ConsoleTheme.Muted("Зелёный — верно, красный — ошибка, жёлтый — текущий символ.");
        Console.WriteLine();

        WriteColoredText(targetText, typedCharacters, settings.TextWidth);
        Console.WriteLine();
        Console.WriteLine();

        if (settings.ShowLiveMetrics)
        {
            int cpm = GetLiveCharactersPerMinute(typedCharacters.Count, elapsed);
            double accuracy = GetLiveAccuracy(mistakes, keyPresses);
            Console.WriteLine(
                $"Введено: {typedCharacters.Count}/{targetText.Length}   " +
                $"Скорость: {cpm} зн/мин   " +
                $"Точность: {accuracy:F1}%   " +
                $"Ошибок: {mistakes}   " +
                $"Время: {FormatTime(elapsed)}");
        }
        else
        {
            Console.WriteLine(
                $"Введено: {typedCharacters.Count}/{targetText.Length}   " +
                $"Время: {FormatTime(elapsed)}");
        }

        if (typedCharacters.Count == targetText.Length &&
            !IsComplete(targetText, typedCharacters))
        {
            ConsoleTheme.Error("В тексте осталась ошибка. Используйте Backspace.");
        }
    }

    private static void ShowResult(RaceResult result, TypingDictionary dictionary)
    {
        ConsoleTheme.Clear();
        ConsoleTheme.Heading("Результат");
        ConsoleTheme.Success("Текст набран полностью!");
        Console.WriteLine();
        Console.WriteLine($"Словарь:             {dictionary.Name}");
        Console.WriteLine($"Время:               {result.DurationSeconds:F1} сек.");
        Console.WriteLine($"Скорость:            {result.CharactersPerMinute} зн./мин");
        Console.WriteLine($"Скорость в словах:   {result.WordsPerMinute} слов/мин");
        Console.WriteLine($"Точность:            {result.Accuracy:F1}%");
        Console.WriteLine($"Ошибок:              {result.Mistakes}");
    }

    private static string ChooseTargetText(TypingDictionary dictionary)
    {
        return dictionary.Entries.Count == 0
            ? string.Empty
            : dictionary.Entries[Random.Shared.Next(dictionary.Entries.Count)];
    }

    private static void WriteColoredText(
        string targetText,
        IReadOnlyList<char> typedCharacters,
        int width)
    {
        int column = 0;

        for (int index = 0; index < targetText.Length; index++)
        {
            if (column >= width && targetText[index] != ' ')
            {
                Console.WriteLine();
                column = 0;
            }

            ConsoleColor foreground = GetCharacterColor(index, targetText, typedCharacters);
            ConsoleColor background = Console.BackgroundColor;

            if (index == typedCharacters.Count)
            {
                background = ConsoleColor.DarkYellow;
            }

            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            Console.Write(targetText[index]);
            ConsoleTheme.Reset();
            column++;
        }
    }

    private static ConsoleColor GetCharacterColor(
        int index,
        string targetText,
        IReadOnlyList<char> typedCharacters)
    {
        if (index < typedCharacters.Count)
        {
            return typedCharacters[index] == targetText[index]
                ? ConsoleColor.Green
                : ConsoleColor.Red;
        }

        return index == typedCharacters.Count
            ? ConsoleColor.Black
            : ConsoleColor.DarkGray;
    }

    private static void WriteWrapped(string text, int width)
    {
        int column = 0;
        foreach (char character in text)
        {
            if (column >= width && character != ' ')
            {
                Console.WriteLine();
                column = 0;
            }

            Console.Write(character);
            column++;
        }

        Console.WriteLine();
    }

    private static bool IsComplete(string targetText, IReadOnlyList<char> typedCharacters)
    {
        if (typedCharacters.Count != targetText.Length)
        {
            return false;
        }

        for (int index = 0; index < targetText.Length; index++)
        {
            if (targetText[index] != typedCharacters[index])
            {
                return false;
            }
        }

        return true;
    }

    private static int GetLiveCharactersPerMinute(int characterCount, TimeSpan elapsed)
    {
        double minutes = elapsed.TotalMinutes;
        return minutes <= 0 ? 0 : (int)Math.Round(characterCount / minutes);
    }

    private static double GetLiveAccuracy(int mistakes, int keyPresses)
    {
        return keyPresses == 0
            ? 100
            : Math.Max(0, (keyPresses - mistakes) * 100.0 / keyPresses);
    }

    private static string FormatTime(TimeSpan time)
    {
        return time.TotalMinutes >= 60
            ? time.ToString(@"hh\:mm\:ss")
            : time.ToString(@"mm\:ss");
    }
}
