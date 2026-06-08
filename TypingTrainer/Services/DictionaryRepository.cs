using System.Text.Json;
using System.Text.Json.Serialization;
using TypingTrainer.Models;

namespace TypingTrainer.Services;

// Репозиторий изолирует работу с JSON от страниц. Компонентам не нужно знать,
// где находится файл и как именно он читается или перезаписывается.
public sealed class DictionaryRepository
{
    private readonly string _filePath;

    // Singleton-репозиторий может получить несколько запросов одновременно.
    // SemaphoreSlim пропускает к файлу только одну операцию за раз.
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public DictionaryRepository(string dataDirectory)
    {
        _filePath = Path.Combine(dataDirectory, "dictionaries.json");
    }

    public async Task<IReadOnlyList<TypingDictionary>> GetAllAsync()
    {
        await _gate.WaitAsync();
        try
        {
            List<TypingDictionary> dictionaries = await LoadUnsafeAsync();

            // Сначала показываем встроенные наборы, затем сортируем по имени.
            return dictionaries
                .OrderByDescending(dictionary => dictionary.IsBuiltIn)
                .ThenBy(dictionary => dictionary.Name)
                .ToList();
        }
        finally
        {
            // finally выполняется даже при исключении, поэтому блокировка
            // обязательно освобождается и следующие запросы не зависнут.
            _gate.Release();
        }
    }

    public async Task<TypingDictionary?> GetByIdAsync(Guid id)
    {
        IReadOnlyList<TypingDictionary> dictionaries = await GetAllAsync();
        return dictionaries.FirstOrDefault(dictionary => dictionary.Id == id);
    }

    public async Task SaveAsync(TypingDictionary dictionary)
    {
        if (dictionary.Entries.Count == 0)
        {
            throw new InvalidOperationException("Словарь должен содержать хотя бы один текст.");
        }

        await _gate.WaitAsync();
        try
        {
            List<TypingDictionary> dictionaries = await LoadUnsafeAsync();
            int index = dictionaries.FindIndex(item => item.Id == dictionary.Id);

            // Найденный Id означает редактирование, отсутствующий — создание.
            if (index >= 0)
            {
                dictionaries[index] = dictionary;
            }
            else
            {
                dictionaries.Add(dictionary);
            }

            await SaveUnsafeAsync(dictionaries);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await _gate.WaitAsync();
        try
        {
            List<TypingDictionary> dictionaries = await LoadUnsafeAsync();
            TypingDictionary? dictionary = dictionaries.FirstOrDefault(item => item.Id == id);

            // Встроенные словари разрешено редактировать, но нельзя удалить.
            if (dictionary is null || dictionary.IsBuiltIn)
            {
                return false;
            }

            dictionaries.Remove(dictionary);
            await SaveUnsafeAsync(dictionaries);
            return true;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<TypingDictionary>> LoadUnsafeAsync()
    {
        // При первом запуске файла ещё нет: создаём стартовые словари.
        if (!File.Exists(_filePath))
        {
            List<TypingDictionary> defaults = CreateDefaultDictionaries();
            await SaveUnsafeAsync(defaults);
            return defaults;
        }

        try
        {
            // await using гарантирует закрытие файла после асинхронного чтения.
            await using FileStream stream = File.OpenRead(_filePath);
            return await JsonSerializer.DeserializeAsync<List<TypingDictionary>>(stream, _jsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return CreateDefaultDictionaries();
        }
        catch (IOException)
        {
            return CreateDefaultDictionaries();
        }
    }

    private async Task SaveUnsafeAsync(IReadOnlyCollection<TypingDictionary> dictionaries)
    {
        string? directory = Path.GetDirectoryName(_filePath);
        Directory.CreateDirectory(directory!);

        // Сначала пишем во временный файл, затем атомарно заменяем основной.
        // Так сбой во время записи не оставит наполовину записанный JSON.
        string temporaryPath = _filePath + ".tmp";
        await using (FileStream stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, dictionaries, _jsonOptions);
        }

        File.Move(temporaryPath, _filePath, true);
    }

    private static List<TypingDictionary> CreateDefaultDictionaries()
    {
        // Постоянные Guid позволяют ссылкам и статистике узнавать встроенные
        // словари между разными запусками приложения.
        return
        [
            new TypingDictionary
            {
                Id = Guid.Parse("71f0d4c1-ecf3-48cb-8ac8-319b241b4084"),
                Name = "Обычный русский",
                Description = "Короткие современные тексты для ежедневной тренировки.",
                Language = "Русский",
                Kind = DictionaryKind.Texts,
                Difficulty = "Средняя",
                IsBuiltIn = true,
                Entries =
                [
                    "Регулярная практика помогает печатать быстрее, точнее и увереннее.",
                    "Хорошая привычка формируется постепенно, когда небольшое действие повторяется каждый день.",
                    "За окном шумел летний дождь, а в комнате тихо играла музыка.",
                    "Сначала важна точность набора, и только затем стоит увеличивать скорость.",
                    "Клавиатурный тренажёр запоминает результаты каждой завершённой попытки."
                ]
            },
            new TypingDictionary
            {
                Id = Guid.Parse("9da6c381-cae2-4cf0-aa1c-6acd891a0fe4"),
                Name = "English warm-up",
                Description = "Короткие английские фразы без сложной пунктуации.",
                Language = "English",
                Kind = DictionaryKind.Phrases,
                Difficulty = "Лёгкая",
                IsBuiltIn = true,
                Entries =
                [
                    "Practice every day to type with confidence.",
                    "A quiet workspace helps you focus on every letter.",
                    "Small improvements become impressive results over time.",
                    "Accuracy comes first and speed follows naturally.",
                    "Keep your hands relaxed and look at the screen."
                ]
            },
            new TypingDictionary
            {
                Id = Guid.Parse("5e3015fd-2f96-48b9-b553-011ff04dcb10"),
                Name = "Частые слова",
                Description = "Ритмичные наборы распространённых русских слов.",
                Language = "Русский",
                Kind = DictionaryKind.Words,
                Difficulty = "Лёгкая",
                IsBuiltIn = true,
                Entries =
                [
                    "время человек работа жизнь слово место вопрос дело страна мир",
                    "новый большой первый другой хороший главный высокий маленький",
                    "думать видеть делать понимать работать говорить смотреть идти",
                    "сегодня завтра всегда рядом быстро точно спокойно уверенно",
                    "книга город дорога музыка школа компьютер клавиатура результат"
                ]
            },
            new TypingDictionary
            {
                Id = Guid.Parse("38d22855-db80-48e7-a6c7-b99b0f1493e6"),
                Name = "C# и символы",
                Description = "Строки кода для тренировки скобок, кавычек и операторов.",
                Language = "C#",
                Kind = DictionaryKind.Code,
                Difficulty = "Сложная",
                IsBuiltIn = true,
                Entries =
                [
                    "Console.WriteLine(\"Hello, typing!\");",
                    "int result = numbers.Where(x => x > 0).Sum();",
                    "public string Name { get; init; } = string.Empty;",
                    "if (isReady && attempts < 3) { Start(); }",
                    "var items = new List<string> { \"one\", \"two\", \"three\" };"
                ]
            }
        ];
    }
}
