namespace TypingTrainer.Models;

// Сводка для верхних карточек страницы статистики.
public sealed record StatisticsSummary(
    int RaceCount,
    int TotalCharacters,
    int BestCharactersPerMinute,
    double AverageCharactersPerMinute,
    double AverageAccuracy,
    TimeSpan TotalPracticeTime,
    string FavoriteDictionary)
{
    public static StatisticsSummary Empty { get; } =
        new(0, 0, 0, 0, 0, TimeSpan.Zero, "—");

    public static StatisticsSummary From(IReadOnlyCollection<RaceResult> results)
    {
        if (results.Count == 0)
        {
            return Empty;
        }

        // GroupBy собирает заезды по названию словаря, а самая большая группа
        // определяет режим, который пользователь запускал чаще всего.
        string favoriteDictionary = results
            .GroupBy(result => result.DictionaryName)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .First()
            .Key;

        return new StatisticsSummary(
            results.Count,
            results.Sum(result => result.CharacterCount),
            results.Max(result => result.CharactersPerMinute),
            results.Average(result => result.CharactersPerMinute),
            results.Average(result => result.Accuracy),
            TimeSpan.FromSeconds(results.Sum(result => result.DurationSeconds)),
            favoriteDictionary);
    }
}

// Отдельная сводка для одной строки таблицы "Результаты по словарям".
public sealed record DictionaryStatistics(
    Guid DictionaryId,
    string DictionaryName,
    int RaceCount,
    int BestCharactersPerMinute,
    double AverageCharactersPerMinute,
    double AverageAccuracy)
{
    public static IReadOnlyList<DictionaryStatistics> From(
        IReadOnlyCollection<RaceResult> results)
    {
        // После группировки Select превращает каждую группу в готовую строку.
        return results
            .GroupBy(result => new { result.DictionaryId, result.DictionaryName })
            .Select(group => new DictionaryStatistics(
                group.Key.DictionaryId,
                group.Key.DictionaryName,
                group.Count(),
                group.Max(result => result.CharactersPerMinute),
                group.Average(result => result.CharactersPerMinute),
                group.Average(result => result.Accuracy)))
            .OrderByDescending(item => item.RaceCount)
            .ThenBy(item => item.DictionaryName)
            .ToList();
    }
}
