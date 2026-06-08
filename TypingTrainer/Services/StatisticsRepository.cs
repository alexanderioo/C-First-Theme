using System.Text.Json;
using TypingTrainer.Models;

namespace TypingTrainer.Services;

// Все заезды хранятся одним JSON-массивом. Для учебного локального проекта
// этого достаточно; при большом объёме данных здесь можно подключить БД.
public sealed class StatisticsRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public StatisticsRepository(string dataDirectory)
    {
        _filePath = Path.Combine(dataDirectory, "statistics.json");
    }

    public async Task<IReadOnlyList<RaceResult>> GetAllAsync()
    {
        await _gate.WaitAsync();
        try
        {
            return await LoadUnsafeAsync();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task AddAsync(RaceResult result)
    {
        await _gate.WaitAsync();
        try
        {
            // Простой подход read-modify-write: читаем историю, добавляем
            // завершённый заезд и сохраняем весь список.
            List<RaceResult> results = await LoadUnsafeAsync();
            results.Add(result);
            await SaveUnsafeAsync(results);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task ClearAsync()
    {
        await _gate.WaitAsync();
        try
        {
            // [] — современный синтаксис C# для пустой коллекции.
            await SaveUnsafeAsync([]);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<RaceResult>> LoadUnsafeAsync()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        try
        {
            await using FileStream stream = File.OpenRead(_filePath);
            return await JsonSerializer.DeserializeAsync<List<RaceResult>>(stream, _jsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
        catch (IOException)
        {
            return [];
        }
    }

    private async Task SaveUnsafeAsync(IReadOnlyCollection<RaceResult> results)
    {
        string? directory = Path.GetDirectoryName(_filePath);
        Directory.CreateDirectory(directory!);

        // Запись через .tmp предотвращает повреждение основной истории.
        string temporaryPath = _filePath + ".tmp";
        await using (FileStream stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, results, _jsonOptions);
        }

        File.Move(temporaryPath, _filePath, true);
    }
}
