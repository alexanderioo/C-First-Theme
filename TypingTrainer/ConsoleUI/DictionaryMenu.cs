using TypingTrainer.Models;
using TypingTrainer.Services;

namespace TypingTrainer.ConsoleUI;

public sealed class DictionaryMenu
{
    private readonly DictionaryRepository _repository;

    public DictionaryMenu(DictionaryRepository repository)
    {
        _repository = repository;
    }

    public async Task ShowAsync()
    {
        bool isOpen = true;

        while (isOpen)
        {
            ConsoleTheme.Clear();
            ConsoleTheme.Heading("Управление словарями");
            await PrintDictionariesAsync();
            Console.WriteLine();
            Console.WriteLine("1. Создать словарь");
            Console.WriteLine("2. Редактировать словарь");
            Console.WriteLine("3. Удалить словарь");
            Console.WriteLine("0. Назад");
            Console.WriteLine();

            int choice = ConsoleInput.ReadMenuChoice("Ваш выбор: ", 0, 3);
            switch (choice)
            {
                case 1:
                    await CreateAsync();
                    break;
                case 2:
                    await EditAsync();
                    break;
                case 3:
                    await DeleteAsync();
                    break;
                case 0:
                    isOpen = false;
                    break;
            }
        }
    }

    public async Task<TypingDictionary?> SelectAsync(string heading)
    {
        // IReadOnlyList не позволяет меню случайно изменить коллекцию репозитория.
        IReadOnlyList<TypingDictionary> dictionaries = await _repository.GetAllAsync();

        ConsoleTheme.Clear();
        ConsoleTheme.Heading(heading);
        PrintDictionaryList(dictionaries);
        Console.WriteLine("0. Назад");
        Console.WriteLine();

        int choice = ConsoleInput.ReadMenuChoice(
            "Выберите словарь: ",
            0,
            dictionaries.Count);

        return choice == 0 ? null : dictionaries[choice - 1];
    }

    private async Task PrintDictionariesAsync()
    {
        IReadOnlyList<TypingDictionary> dictionaries = await _repository.GetAllAsync();
        PrintDictionaryList(dictionaries);
    }

    private static void PrintDictionaryList(IReadOnlyList<TypingDictionary> dictionaries)
    {
        for (int index = 0; index < dictionaries.Count; index++)
        {
            TypingDictionary dictionary = dictionaries[index];
            string builtInLabel = dictionary.IsBuiltIn ? " · встроенный" : string.Empty;

            Console.WriteLine(
                $"{index + 1}. {dictionary.Name} " +
                $"[{dictionary.Language}, {dictionary.Difficulty}{builtInLabel}]");
            ConsoleTheme.Muted(
                $"   {dictionary.Description} " +
                $"({dictionary.Entries.Count} {GetTextWord(dictionary.Entries.Count)})");
        }
    }

    private async Task CreateAsync()
    {
        ConsoleTheme.Clear();
        ConsoleTheme.Heading("Новый словарь");

        string name = ConsoleInput.ReadRequired("Название: ");
        string description = ConsoleInput.ReadRequired("Описание: ");
        string language = ConsoleInput.ReadRequired("Язык: ");
        DictionaryKind kind = SelectKind(DictionaryKind.Texts);
        string difficulty = SelectDifficulty("Средняя");
        List<string> entries = ReadEntries();

        // required-свойства модели необходимо заполнить при создании объекта.
        var dictionary = new TypingDictionary
        {
            Name = name,
            Description = description,
            Language = language,
            Kind = kind,
            Difficulty = difficulty,
            Entries = entries
        };

        await _repository.SaveAsync(dictionary);
        ConsoleTheme.Success("Словарь создан.");
        ConsoleInput.WaitForEnter();
    }

    private async Task EditAsync()
    {
        TypingDictionary? dictionary = await SelectAsync("Редактирование словаря");
        if (dictionary is null)
        {
            return;
        }

        ConsoleTheme.Clear();
        ConsoleTheme.Heading($"Редактирование: {dictionary.Name}");
        ConsoleTheme.Muted("Нажмите Enter, чтобы оставить текущее значение.");
        Console.WriteLine();

        string name = ConsoleInput.ReadOptional("Название", dictionary.Name);
        string description = ConsoleInput.ReadOptional("Описание", dictionary.Description);
        string language = ConsoleInput.ReadOptional("Язык", dictionary.Language);

        Console.WriteLine();
        Console.WriteLine($"Текущий тип: {GetKindName(dictionary.Kind)}");
        DictionaryKind kind = ConsoleInput.Confirm("Изменить тип?")
            ? SelectKind(dictionary.Kind)
            : dictionary.Kind;

        Console.WriteLine($"Текущая сложность: {dictionary.Difficulty}");
        string difficulty = ConsoleInput.Confirm("Изменить сложность?")
            ? SelectDifficulty(dictionary.Difficulty)
            : dictionary.Difficulty;

        List<string> entries = dictionary.Entries;
        Console.WriteLine();
        Console.WriteLine($"Сейчас в словаре {entries.Count} {GetTextWord(entries.Count)}.");
        if (ConsoleInput.Confirm("Заменить все тексты?"))
        {
            entries = ReadEntries();
        }

        // record удобно изменять выражением with: создаётся копия со старыми
        // Id/CreatedAt/IsBuiltIn и только перечисленными новыми значениями.
        TypingDictionary updated = dictionary with
        {
            Name = name,
            Description = description,
            Language = language,
            Kind = kind,
            Difficulty = difficulty,
            Entries = entries
        };

        await _repository.SaveAsync(updated);
        ConsoleTheme.Success("Изменения сохранены.");
        ConsoleInput.WaitForEnter();
    }

    private async Task DeleteAsync()
    {
        TypingDictionary? dictionary = await SelectAsync("Удаление словаря");
        if (dictionary is null)
        {
            return;
        }

        if (dictionary.IsBuiltIn)
        {
            ConsoleTheme.Warning("Встроенный словарь нельзя удалить.");
            ConsoleInput.WaitForEnter();
            return;
        }

        if (!ConsoleInput.Confirm($"Удалить словарь «{dictionary.Name}»?"))
        {
            return;
        }

        bool deleted = await _repository.DeleteAsync(dictionary.Id);
        if (deleted)
        {
            ConsoleTheme.Success("Словарь удалён.");
        }
        else
        {
            ConsoleTheme.Error("Не удалось удалить словарь.");
        }

        ConsoleInput.WaitForEnter();
    }

    private static List<string> ReadEntries()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Введите тексты. Каждый текст — отдельная строка.");
            Console.WriteLine("Пустая строка завершает ввод.");

            var entries = new List<string>();
            while (true)
            {
                Console.Write($"{entries.Count + 1}> ");
                string? entry = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(entry))
                {
                    break;
                }

                string normalized = entry.Trim();
                // Не добавляем полностью совпадающие задания дважды.
                if (!entries.Contains(normalized, StringComparer.Ordinal))
                {
                    entries.Add(normalized);
                }
            }

            if (entries.Count > 0)
            {
                return entries;
            }

            ConsoleTheme.Error("Добавьте хотя бы один текст.");
        }
    }

    private static DictionaryKind SelectKind(DictionaryKind current)
    {
        Console.WriteLine();
        Console.WriteLine("Тип словаря:");
        Console.WriteLine("1. Слова");
        Console.WriteLine("2. Фразы");
        Console.WriteLine("3. Тексты");
        Console.WriteLine("4. Код");
        ConsoleTheme.Muted($"Текущее значение: {GetKindName(current)}");

        return ConsoleInput.ReadMenuChoice("Выберите тип: ", 1, 4) switch
        {
            1 => DictionaryKind.Words,
            2 => DictionaryKind.Phrases,
            3 => DictionaryKind.Texts,
            4 => DictionaryKind.Code,
            _ => current
        };
    }

    private static string SelectDifficulty(string current)
    {
        Console.WriteLine();
        Console.WriteLine("Сложность:");
        Console.WriteLine("1. Лёгкая");
        Console.WriteLine("2. Средняя");
        Console.WriteLine("3. Сложная");
        ConsoleTheme.Muted($"Текущее значение: {current}");

        return ConsoleInput.ReadMenuChoice("Выберите сложность: ", 1, 3) switch
        {
            1 => "Лёгкая",
            2 => "Средняя",
            3 => "Сложная",
            _ => current
        };
    }

    private static string GetKindName(DictionaryKind kind) => kind switch
    {
        DictionaryKind.Words => "Слова",
        DictionaryKind.Phrases => "Фразы",
        DictionaryKind.Texts => "Тексты",
        DictionaryKind.Code => "Код",
        _ => "Словарь"
    };

    private static string GetTextWord(int count)
    {
        int lastTwoDigits = count % 100;
        int lastDigit = count % 10;

        if (lastTwoDigits is >= 11 and <= 14)
        {
            return "текстов";
        }

        return lastDigit switch
        {
            1 => "текст",
            2 or 3 or 4 => "текста",
            _ => "текстов"
        };
    }
}
