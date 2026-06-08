using TypingTrainer.Services;

namespace TypingTrainer.ConsoleUI;

public sealed class ConsoleApplication
{
    private readonly SettingsRepository _settingsRepository;
    private readonly StatisticsRepository _statisticsRepository;
    private readonly DictionaryMenu _dictionaryMenu;
    private readonly SettingsMenu _settingsMenu;
    private readonly StatisticsMenu _statisticsMenu;
    private readonly TypingSession _typingSession = new();

    public ConsoleApplication(
        DictionaryRepository dictionaryRepository,
        SettingsRepository settingsRepository,
        StatisticsRepository statisticsRepository)
    {
        _settingsRepository = settingsRepository;
        _statisticsRepository = statisticsRepository;
        _dictionaryMenu = new DictionaryMenu(dictionaryRepository);
        _settingsMenu = new SettingsMenu(settingsRepository);
        _statisticsMenu = new StatisticsMenu(statisticsRepository);
    }

    public async Task RunAsync()
    {
        // Перед первым выводом читаем сохранённую тему и применяем её к Terminal.
        Models.UserSettings settings = await _settingsRepository.GetAsync();
        ConsoleTheme.Apply(settings);

        // Главное меню работает в цикле, пока пользователь не выберет выход.
        bool isRunning = true;
        while (isRunning)
        {
            ConsoleTheme.Clear();
            ConsoleTheme.Heading("Клавиши — тренажёр печати");
            Console.WriteLine("1. Начать тренировку");
            Console.WriteLine("2. Словари");
            Console.WriteLine("3. Статистика");
            Console.WriteLine("4. Настройки");
            Console.WriteLine("5. О программе");
            Console.WriteLine("0. Выход");
            Console.WriteLine();

            int choice = ConsoleInput.ReadMenuChoice("Ваш выбор: ", 0, 5);
            switch (choice)
            {
                case 1:
                    await StartTrainingAsync();
                    break;
                case 2:
                    await _dictionaryMenu.ShowAsync();
                    break;
                case 3:
                    await _statisticsMenu.ShowAsync();
                    break;
                case 4:
                    await _settingsMenu.ShowAsync();
                    break;
                case 5:
                    ShowAbout();
                    break;
                case 0:
                    isRunning = false;
                    break;
            }
        }

        ConsoleTheme.Clear();
        ConsoleTheme.Success("До встречи! Регулярная практика даёт лучший результат.");
        ConsoleTheme.Reset();
    }

    private async Task StartTrainingAsync()
    {
        // DictionaryMenu возвращает выбранный объект или null при пункте "Назад".
        Models.TypingDictionary? dictionary =
            await _dictionaryMenu.SelectAsync("Выбор словаря");

        if (dictionary is null)
        {
            return;
        }

        Models.UserSettings settings = await _settingsRepository.GetAsync();
        ConsoleTheme.Apply(settings);

        // TypingSession отвечает только за сам набор и вычисление результата.
        Models.RaceResult? result = _typingSession.Run(dictionary, settings);
        if (result is null)
        {
            ConsoleTheme.Warning("Тренировка прервана.");
            ConsoleInput.WaitForEnter();
            return;
        }

        try
        {
            // Сохранение отделено от тренировки: при ошибке диска пользователь
            // всё равно увидит рассчитанные скорость и точность.
            await _statisticsRepository.AddAsync(result);
        }
        catch (IOException exception)
        {
            ConsoleTheme.Warning($"Результат показан, но не сохранён: {exception.Message}");
        }
        catch (UnauthorizedAccessException exception)
        {
            ConsoleTheme.Warning($"Результат показан, но не сохранён: {exception.Message}");
        }

        ConsoleInput.WaitForEnter();
    }

    private static void ShowAbout()
    {
        ConsoleTheme.Clear();
        ConsoleTheme.Heading("О программе");
        Console.WriteLine("Консольный тренажёр набора текста на C# и .NET 9.");
        Console.WriteLine("Работает в Terminal на macOS, а также в Linux и Windows.");
        Console.WriteLine();
        Console.WriteLine("Размер шрифта терминала на Mac:");
        Console.WriteLine("  Cmd + Plus  — увеличить");
        Console.WriteLine("  Cmd + Minus — уменьшить");
        ConsoleInput.WaitForEnter();
    }
}
