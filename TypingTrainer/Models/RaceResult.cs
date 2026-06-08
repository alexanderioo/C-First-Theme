namespace TypingTrainer.Models;

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

    public static RaceResult Create(
        TypingDictionary dictionary,
        int characterCount,
        TimeSpan duration,
        int mistakes,
        int keyPresses)
    {
        double minutes = Math.Max(duration.TotalMinutes, 1.0 / 60_000);
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
            CharactersPerMinute = (int)Math.Round(characterCount / minutes),
            WordsPerMinute = (int)Math.Round(characterCount / 5.0 / minutes),
            Accuracy = Math.Round(accuracy, 1)
        };
    }
}
