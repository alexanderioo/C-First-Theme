using System.Diagnostics;

namespace TypingTrainer;

public sealed class TypingSession
{
    public SessionResult? Run(
        string targetText,
        TrainingLanguage language,
        Difficulty difficulty)
    {
        ShowPreparation(targetText);

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

        var typedCharacters = new List<char>(targetText.Length);
        int mistakes = 0;
        int characterKeyPresses = 0;
        var stopwatch = Stopwatch.StartNew();

        while (true)
        {
            Render(targetText, typedCharacters, mistakes, stopwatch.Elapsed);

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
            characterKeyPresses++;

            if (key.KeyChar != expectedCharacter)
            {
                mistakes++;
            }
        }

        stopwatch.Stop();
        return CreateResult(
            targetText.Length,
            language,
            difficulty,
            mistakes,
            characterKeyPresses,
            stopwatch.Elapsed);
    }

    private static SessionResult CreateResult(
        int characterCount,
        TrainingLanguage language,
        Difficulty difficulty,
        int mistakes,
        int characterKeyPresses,
        TimeSpan elapsed)
    {
        double minutes = Math.Max(elapsed.TotalMinutes, 1.0 / 60_000);
        double accuracy = characterKeyPresses == 0
            ? 100
            : Math.Max(0, (characterKeyPresses - mistakes) * 100.0 / characterKeyPresses);

        return new SessionResult
        {
            CompletedAt = DateTime.Now,
            Language = language,
            Difficulty = difficulty,
            CharacterCount = characterCount,
            DurationSeconds = elapsed.TotalSeconds,
            Mistakes = mistakes,
            CharactersPerMinute = (int)Math.Round(characterCount / minutes),
            WordsPerMinute = (int)Math.Round(characterCount / 5.0 / minutes),
            Accuracy = Math.Round(accuracy, 1)
        };
    }

    private static void ShowPreparation(string targetText)
    {
        Console.Clear();
        WriteHeading("ТРЕНИРОВКА");
        Console.WriteLine("Наберите предложенный текст символ в символ.");
        Console.WriteLine("Backspace — исправить, Esc — прервать тренировку.");
        Console.WriteLine();
        Console.WriteLine(targetText);
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Нажмите Enter, когда будете готовы...");
        Console.ResetColor();
    }

    private static void Render(
        string targetText,
        IReadOnlyList<char> typedCharacters,
        int mistakes,
        TimeSpan elapsed)
    {
        Console.Clear();
        WriteHeading("ТРЕНИРОВКА");
        Console.WriteLine("Зелёный — верно, красный — ошибка, жёлтый — следующий символ.");
        Console.WriteLine();

        for (int index = 0; index < targetText.Length; index++)
        {
            if (index < typedCharacters.Count)
            {
                Console.ForegroundColor = typedCharacters[index] == targetText[index]
                    ? ConsoleColor.Green
                    : ConsoleColor.Red;
            }
            else if (index == typedCharacters.Count)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Yellow;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            Console.Write(targetText[index]);
            Console.ResetColor();
        }

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine(
            $"Введено: {typedCharacters.Count}/{targetText.Length}   " +
            $"Ошибок: {mistakes}   " +
            $"Время: {elapsed:mm\\:ss}");

        if (typedCharacters.Count == targetText.Length &&
            !IsComplete(targetText, typedCharacters))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("В тексте осталась ошибка. Используйте Backspace для исправления.");
            Console.ResetColor();
        }
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

    private static void WriteHeading(string text)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"=== {text} ===");
        Console.ResetColor();
    }
}
