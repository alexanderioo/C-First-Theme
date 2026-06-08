namespace TypingTrainer;

public sealed record StatisticsSummary(
    int SessionCount,
    int TotalCharacters,
    int BestCharactersPerMinute,
    double AverageCharactersPerMinute,
    double AverageAccuracy)
{
    public static StatisticsSummary From(IReadOnlyCollection<SessionResult> results)
    {
        if (results.Count == 0)
        {
            return new StatisticsSummary(0, 0, 0, 0, 0);
        }

        return new StatisticsSummary(
            results.Count,
            results.Sum(result => result.CharacterCount),
            results.Max(result => result.CharactersPerMinute),
            results.Average(result => result.CharactersPerMinute),
            results.Average(result => result.Accuracy));
    }
}
