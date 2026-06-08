using System.Text.Json;
using TypingTrainer.Models;

namespace TypingTrainer.Services;

public sealed class StatisticsRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public StatisticsRepository(IWebHostEnvironment environment)
    {
        _filePath = Path.Combine(environment.ContentRootPath, "App_Data", "statistics.json");
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

        string temporaryPath = _filePath + ".tmp";
        await using (FileStream stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, results, _jsonOptions);
        }

        File.Move(temporaryPath, _filePath, true);
    }
}
