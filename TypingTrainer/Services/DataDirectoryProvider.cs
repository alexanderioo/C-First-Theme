namespace TypingTrainer.Services;

public static class DataDirectoryProvider
{
    public static string GetDataDirectory()
    {
        // Переменная окружения удобна для тестов и переносимого запуска.
        string? configuredDirectory =
            Environment.GetEnvironmentVariable("TYPING_TRAINER_DATA_DIR");

        if (!string.IsNullOrWhiteSpace(configuredDirectory))
        {
            Directory.CreateDirectory(configuredDirectory);
            return configuredDirectory;
        }

        // На macOS это ~/Library/Application Support/TypingTrainer.
        // Пользовательские JSON-файлы не смешиваются с исходным кодом и Git.
        string localData =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string dataDirectory = Path.Combine(localData, "TypingTrainer");
        Directory.CreateDirectory(dataDirectory);
        return dataDirectory;
    }
}
