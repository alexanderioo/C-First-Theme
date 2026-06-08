using System.Text.Json;
using System.Text.Json.Serialization;
using TypingTrainer.Models;

namespace TypingTrainer.Services;

public sealed class SettingsRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public SettingsRepository(IWebHostEnvironment environment)
    {
        _filePath = Path.Combine(environment.ContentRootPath, "App_Data", "settings.json");
    }

    public event Action<UserSettings>? Changed;

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
        UserSettings normalized = settings with
        {
            FontSize = Math.Clamp(settings.FontSize, 18, 44)
        };

        await _gate.WaitAsync();
        try
        {
            string? directory = Path.GetDirectoryName(_filePath);
            Directory.CreateDirectory(directory!);

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

        Changed?.Invoke(normalized);
    }
}
