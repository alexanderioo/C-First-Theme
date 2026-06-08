namespace TypingTrainer.Models;

// Неизменяемый снимок одного завершённого заезда.
public sealed record RaceResult
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;

    public Guid DictionaryId { get; init; }

    public required string DictionaryName { get; init; }

    public int CharacterCount { get; init; }

    public double DurationSeconds { get; init; }

    public int Mistakes { get; init; }

    public int KeyPresses { get; init; }

    public int CharactersPerMinute { get; init; }

    public int WordsPerMinute { get; init; }

    public double Accuracy { get; init; }

    // Фабричный метод держит все формулы в одном месте, чтобы интерфейс
    // не отвечал за вычисления и только отображал готовый результат.
    public static RaceResult Create(
        TypingDictionary dictionary,
        int characterCount,
        TimeSpan duration,
        int mistakes,
        int keyPresses)
    {
        // Нижняя граница защищает от деления на ноль при очень быстром тесте.
        double minutes = Math.Max(duration.TotalMinutes, 1.0 / 60_000);

        // Ошибка учитывается даже после Backspace: keyPresses содержит все
        // введённые символы, а mistakes — все неверные нажатия.
        double accuracy = keyPresses == 0
            ? 100
            : Math.Max(0, (keyPresses - mistakes) * 100.0 / keyPresses);

        return new RaceResult
        {
            DictionaryId = dictionary.Id,
            DictionaryName = dictionary.Name,
            CharacterCount = characterCount,
            DurationSeconds = duration.TotalSeconds,
            Mistakes = mistakes,
            KeyPresses = keyPresses,
            // Знаки в минуту — основной показатель скорости.
            CharactersPerMinute = (int)Math.Round(characterCount / minutes),
            // В международной методике одно условное слово равно 5 символам.
            WordsPerMinute = (int)Math.Round(characterCount / 5.0 / minutes),
            Accuracy = Math.Round(accuracy, 1)
        };
    }
}
