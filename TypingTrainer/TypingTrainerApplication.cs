using TypingTrainer.Services;

namespace TypingTrainer;

public sealed class TypingTrainerApplication
{
    private readonly TextProvider _textProvider;
    private readonly IResultRepository _repository;
    private readonly TypingSession _typingSession;

    public TypingTrainerApplication(
        TextProvider textProvider,
        IResultRepository repository,
        TypingSession typingSession)
    {
        _textProvider = textProvider;
        _repository = repository;
        _typingSession = typingSession;
    }

    public void Run()
    {
        bool isRunning = true;

        while (isRunning)
        {
            ShowMainMenu();
            string? choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    StartStandardTraining();
                    break;
                case "2":
                    StartCustomTraining();
                    break;
                case "3":
                    ShowStatistics();
                    break;
                case "4":
                    ShowAbout();
                    break;
                case "0":
                    isRunning = false;
                    break;
                default:
                    ShowMessage("Выберите пункт от 0 до 4.", ConsoleColor.Red);
                    break;
            }
        }

        Console.Clear();
        Console.WriteLine("До встречи! Продолжайте тренироваться.");
    }

    private void StartStandardTraining()
    {
        TrainingLanguage? language = SelectLanguage();
        if (language is null)
        {
            return;
        }

        Difficulty? difficulty = SelectDifficulty();
        if (difficulty is null)
        {
            return;
        }

        string text = _textProvider.GetRandom(language.Value, difficulty.Value);
        CompleteTraining(text, language.Value, difficulty.Value);
    }

    private void StartCustomTraining()
    {
        Console.Clear();
        WriteHeading("СВОЙ ТЕКСТ");
        Console.WriteLine("Введите текст для тренировки и нажмите Enter.");
        Console.WriteLine("Пустая строка вернёт вас в главное меню.");
        Console.WriteLine();
        Console.Write("> ");

        string? text = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        CompleteTraining(text, TrainingLanguage.Custom, Difficulty.Custom);
    }

    private void CompleteTraining(
        string text,
        TrainingLanguage language,
        Difficulty difficulty)
    {
        SessionResult? result = _typingSession.Run(text, language, difficulty);
        if (result is null)
        {
            ShowMessage("Тренировка прервана.", ConsoleColor.Yellow);
            return;
        }

        string? saveError = null;
        try
        {
            _repository.Add(result);
        }
        catch (IOException exception)
        {
            saveError = exception.Message;
        }
        catch (UnauthorizedAccessException exception)
        {
            saveError = exception.Message;
        }

        ShowResult(result, saveError);
    }

    private static void ShowResult(SessionResult result, string? saveError)
    {
        Console.Clear();
        WriteHeading("РЕЗУЛЬТАТ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Текст набран полностью!");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine($"Время:              {result.DurationSeconds:F1} сек.");
        Console.WriteLine($"Скорость:           {result.CharactersPerMinute} зн./мин");
        Console.WriteLine($"Скорость в словах:  {result.WordsPerMinute} слов/мин");
        Console.WriteLine($"Точность:            {result.Accuracy:F1}%");
        Console.WriteLine($"Ошибок:              {result.Mistakes}");

        if (saveError is not null)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Не удалось сохранить результат: {saveError}");
            Console.ResetColor();
        }

        WaitForEnter();
    }

    private void ShowStatistics()
    {
        IReadOnlyList<SessionResult> results = _repository.Load();

        Console.Clear();
        WriteHeading("СТАТИСТИКА");

        if (results.Count == 0)
        {
            Console.WriteLine("Завершённых тренировок пока нет.");
            WaitForEnter();
            return;
        }

        StatisticsSummary summary = StatisticsSummary.From(results);
        Console.WriteLine($"Всего тренировок:       {summary.SessionCount}");
        Console.WriteLine($"Набрано символов:        {summary.TotalCharacters}");
        Console.WriteLine($"Лучшая скорость:         {summary.BestCharactersPerMinute} зн./мин");
        Console.WriteLine($"Средняя скорость:        {summary.AverageCharactersPerMinute:F0} зн./мин");
        Console.WriteLine($"Средняя точность:        {summary.AverageAccuracy:F1}%");
        Console.WriteLine();
        Console.WriteLine("Последние результаты:");
        Console.WriteLine("Дата              Скорость    Точность   Ошибки");
        Console.WriteLine(new string('-', 50));

        foreach (SessionResult result in results.OrderByDescending(item => item.CompletedAt).Take(10))
        {
            Console.WriteLine(
                $"{result.CompletedAt:dd.MM.yyyy HH:mm}  " +
                $"{result.CharactersPerMinute,5} зн/м  " +
                $"{result.Accuracy,6:F1}%  " +
                $"{result.Mistakes,6}");
        }

        WaitForEnter();
    }

    private static void ShowAbout()
    {
        Console.Clear();
        WriteHeading("О ПРОГРАММЕ");
        Console.WriteLine("Тренажёр помогает развивать скорость и точность печати.");
        Console.WriteLine();
        Console.WriteLine("Скорость считается в символах за минуту.");
        Console.WriteLine("Одно условное слово равно пяти символам.");
        Console.WriteLine("Точность учитывает все ошибочно нажатые символы,");
        Console.WriteLine("даже если затем они были исправлены клавишей Backspace.");
        WaitForEnter();
    }

    private static TrainingLanguage? SelectLanguage()
    {
        Console.Clear();
        WriteHeading("ВЫБОР ЯЗЫКА");
        Console.WriteLine("1. Русский");
        Console.WriteLine("2. Английский");
        Console.WriteLine("0. Назад");
        Console.WriteLine();
        Console.Write("Ваш выбор: ");

        return Console.ReadLine()?.Trim() switch
        {
            "1" => TrainingLanguage.Russian,
            "2" => TrainingLanguage.English,
            _ => null
        };
    }

    private static Difficulty? SelectDifficulty()
    {
        Console.Clear();
        WriteHeading("УРОВЕНЬ СЛОЖНОСТИ");
        Console.WriteLine("1. Лёгкий");
        Console.WriteLine("2. Средний");
        Console.WriteLine("3. Сложный");
        Console.WriteLine("0. Назад");
        Console.WriteLine();
        Console.Write("Ваш выбор: ");

        return Console.ReadLine()?.Trim() switch
        {
            "1" => Difficulty.Easy,
            "2" => Difficulty.Medium,
            "3" => Difficulty.Hard,
            _ => null
        };
    }

    private static void ShowMainMenu()
    {
        Console.Clear();
        WriteHeading("ТРЕНАЖЁР НАБОРА ТЕКСТА");
        Console.WriteLine("1. Начать тренировку");
        Console.WriteLine("2. Тренировка со своим текстом");
        Console.WriteLine("3. Посмотреть статистику");
        Console.WriteLine("4. О программе");
        Console.WriteLine("0. Выход");
        Console.WriteLine();
        Console.Write("Ваш выбор: ");
    }

    private static void ShowMessage(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
        WaitForEnter();
    }

    private static void WaitForEnter()
    {
        Console.WriteLine();
        Console.Write("Нажмите Enter, чтобы продолжить...");
        Console.ReadLine();
    }

    private static void WriteHeading(string text)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"=== {text} ===");
        Console.ResetColor();
        Console.WriteLine();
    }
}
