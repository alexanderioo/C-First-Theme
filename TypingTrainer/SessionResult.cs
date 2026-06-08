namespace TypingTrainer;

public sealed record SessionResult
{
    public DateTime CompletedAt { get; init; }

    public TrainingLanguage Language { get; init; }

    public Difficulty Difficulty { get; init; }

    public int CharacterCount { get; init; }

    public double DurationSeconds { get; init; }

    public int Mistakes { get; init; }

    public int CharactersPerMinute { get; init; }

    public int WordsPerMinute { get; init; }

    public double Accuracy { get; init; }
}
