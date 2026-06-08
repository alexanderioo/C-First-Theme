using TypingTrainer.Models;
using TypingTrainer.Services;

namespace TypingTrainer.ConsoleUI;

public sealed class StatisticsMenu
{
    private readonly StatisticsRepository _repository;

    public StatisticsMenu(StatisticsRepository repository)
    {
        _repository = repository;
    }

    public async Task ShowAsync()
    {
        bool isOpen = true;

        while (isOpen)
        {
            // Новые заезды выводятся первыми в таблице истории.
            IReadOnlyList<RaceResult> results = (await _repository.GetAllAsync())
                .OrderByDescending(result => result.CompletedAt)
                .ToList();

            ConsoleTheme.Clear();
            ConsoleTheme.Heading("Статистика");

            if (results.Count == 0)
            {
                Console.WriteLine("Завершённых тренировок пока нет.");
                Console.WriteLine();
                Console.WriteLine("0. Назад");
                Console.WriteLine();
                ConsoleInput.ReadMenuChoice("Ваш выбор: ", 0, 0);
                return;
            }

            PrintSummary(StatisticsSummary.From(results));
            Console.WriteLine();
            Console.WriteLine("1. Последние результаты");
            Console.WriteLine("2. Статистика по словарям");
            Console.WriteLine("3. Очистить историю");
            Console.WriteLine("0. Назад");
            Console.WriteLine();

            int choice = ConsoleInput.ReadMenuChoice("Ваш выбор: ", 0, 3);
            switch (choice)
            {
                case 1:
                    ShowHistory(results);
                    break;
                case 2:
                    ShowDictionaryStatistics(results);
                    break;
                case 3:
                    await ClearAsync();
                    break;
                case 0:
                    isOpen = false;
                    break;
            }
        }
    }

    private static void PrintSummary(StatisticsSummary summary)
    {
        Console.WriteLine($"Всего тренировок:       {summary.RaceCount}");
        Console.WriteLine($"Набрано символов:        {summary.TotalCharacters:N0}");
        Console.WriteLine($"Лучшая скорость:         {summary.BestCharactersPerMinute} зн./мин");
        Console.WriteLine($"Средняя скорость:        {summary.AverageCharactersPerMinute:F0} зн./мин");
        Console.WriteLine($"Средняя точность:        {summary.AverageAccuracy:F1}%");
        Console.WriteLine($"Время практики:          {FormatPracticeTime(summary.TotalPracticeTime)}");
        Console.WriteLine($"Любимый словарь:         {summary.FavoriteDictionary}");
    }

    private static void ShowHistory(IReadOnlyList<RaceResult> results)
    {
        ConsoleTheme.Clear();
        ConsoleTheme.Heading("Последние результаты");
        Console.WriteLine(
            $"{"Дата",-17} {"Словарь",-22} {"Скорость",10} {"Точность",9} {"Ошибки",7}");
        Console.WriteLine(new string('-', 72));

        foreach (RaceResult result in results.Take(20))
        {
            string dictionaryName = Truncate(result.DictionaryName, 22);
            Console.WriteLine(
                $"{result.CompletedAt.ToLocalTime():dd.MM.yy HH:mm}  " +
                $"{dictionaryName,-22} " +
                $"{result.CharactersPerMinute,6} зн/м " +
                $"{result.Accuracy,7:F1}% " +
                $"{result.Mistakes,7}");
        }

        ConsoleInput.WaitForEnter();
    }

    private static void ShowDictionaryStatistics(IReadOnlyCollection<RaceResult> results)
    {
        // Группировка и средние значения рассчитываются моделью, меню их выводит.
        IReadOnlyList<DictionaryStatistics> statistics =
            DictionaryStatistics.From(results);

        ConsoleTheme.Clear();
        ConsoleTheme.Heading("Статистика по словарям");
        Console.WriteLine(
            $"{"Словарь",-24} {"Заезды",7} {"Средняя",12} {"Рекорд",10} {"Точность",10}");
        Console.WriteLine(new string('-', 70));

        foreach (DictionaryStatistics item in statistics)
        {
            string dictionaryName = Truncate(item.DictionaryName, 24);
            Console.WriteLine(
                $"{dictionaryName,-24} " +
                $"{item.RaceCount,7} " +
                $"{item.AverageCharactersPerMinute,8:F0} зн/м " +
                $"{item.BestCharactersPerMinute,6} зн/м " +
                $"{item.AverageAccuracy,8:F1}%");
        }

        ConsoleInput.WaitForEnter();
    }

    private async Task ClearAsync()
    {
        if (!ConsoleInput.Confirm("Удалить всю историю тренировок?"))
        {
            return;
        }

        await _repository.ClearAsync();
        ConsoleTheme.Success("История очищена.");
        ConsoleInput.WaitForEnter();
    }

    private static string FormatPracticeTime(TimeSpan time)
    {
        if (time.TotalHours >= 1)
        {
            return $"{(int)time.TotalHours} ч {time.Minutes} мин";
        }

        return time.TotalMinutes >= 1
            ? $"{(int)time.TotalMinutes} мин {time.Seconds} сек"
            : $"{Math.Max(1, (int)time.TotalSeconds)} сек";
    }

    private static string Truncate(string value, int maximumLength)
    {
        return value.Length <= maximumLength
            ? value
            : value[..(maximumLength - 1)] + "…";
    }
}
