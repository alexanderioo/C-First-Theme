using System.Text.Json;
using System.Text.Json.Serialization;
using TypingTrainer.Models;

namespace TypingTrainer.Services;

// Хранит пользовательское оформление и сообщает интерфейсу об изменениях.
public sealed class SettingsRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public SettingsRepository(string dataDirectory)
    {
        _filePath = Path.Combine(dataDirectory, "settings.json");
    }

    public async Task<UserSettings> GetAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (!File.Exists(_filePath))
            {
                return UserSettings.Default;
            }

            await using FileStream stream = File.OpenRead(_filePath);
            return await JsonSerializer.DeserializeAsync<UserSettings>(stream, _jsonOptions)
                ?? UserSettings.Default;
        }
        catch (JsonException)
        {
            return UserSettings.Default;
        }
        catch (IOException)
        {
            return UserSettings.Default;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(UserSettings settings)
    {
        // Ограничиваем значения, даже если JSON был изменён вручную.
        UserSettings normalized = settings with
        {
            TextWidth = Math.Clamp(settings.TextWidth, 40, 120),
            CountdownSeconds = Math.Clamp(settings.CountdownSeconds, 0, 5)
        };

        await _gate.WaitAsync();
        try
        {
            string? directory = Path.GetDirectoryName(_filePath);
            Directory.CreateDirectory(directory!);

            // Временный файл защищает текущие настройки от частичной записи.
            string temporaryPath = _filePath + ".tmp";
            await using (FileStream stream = File.Create(temporaryPath))
            {
                await JsonSerializer.SerializeAsync(stream, normalized, _jsonOptions);
            }

            File.Move(temporaryPath, _filePath, true);
        }
        finally
        {
            _gate.Release();
        }
    }
}
