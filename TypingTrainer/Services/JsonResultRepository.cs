using System.Text.Json;
using System.Text.Json.Serialization;

namespace TypingTrainer.Services;

public sealed class JsonResultRepository : IResultRepository
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public JsonResultRepository(string? filePath = null)
    {
        _filePath = filePath ?? GetDefaultFilePath();
    }

    public IReadOnlyList<SessionResult> Load()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        try
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<SessionResult>>(json, _serializerOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
        catch (IOException)
        {
            return [];
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
    }

    public void Add(SessionResult result)
    {
        List<SessionResult> results = Load().ToList();
        results.Add(result);

        string? directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string temporaryPath = _filePath + ".tmp";
        string json = JsonSerializer.Serialize(results, _serializerOptions);
        File.WriteAllText(temporaryPath, json);
        File.Move(temporaryPath, _filePath, true);
    }

    private static string GetDefaultFilePath()
    {
        string? configuredPath = Environment.GetEnvironmentVariable("TYPING_TRAINER_DATA_FILE");
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return configuredPath;
        }

        string localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localData, "TypingTrainer", "results.json");
    }
}
