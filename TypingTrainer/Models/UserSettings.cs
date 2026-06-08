namespace TypingTrainer.Models;

// enum ограничивает набор допустимых вариантов и не даёт сохранить
// произвольную строку вместо известной темы, шрифта или интервала.
public enum AppTheme
{
    Light,
    Dark
}

public sealed record UserSettings
{
    // Этот экземпляр используется, пока пользователь ничего не настраивал
    // или если файл настроек отсутствует/повреждён.
    public static UserSettings Default { get; } = new();

    public AppTheme Theme { get; init; } = AppTheme.Light;

    public bool ShowLiveMetrics { get; init; } = true;

    public int TextWidth { get; init; } = 80;

    public int CountdownSeconds { get; init; } = 3;
}
