namespace TypingTrainer.Models;

// enum ограничивает набор допустимых вариантов и не даёт сохранить
// произвольную строку вместо известной темы, шрифта или интервала.
public enum AppTheme
{
    Light,
    Dark,
    Contrast
}

public enum TrainerFontFamily
{
    Mono,
    Sans,
    Serif
}

public enum LineSpacing
{
    Compact,
    Comfortable,
    Airy
}

public sealed record UserSettings
{
    // Этот экземпляр используется, пока пользователь ничего не настраивал
    // или если файл настроек отсутствует/повреждён.
    public static UserSettings Default { get; } = new();

    public int FontSize { get; init; } = 28;

    public TrainerFontFamily FontFamily { get; init; } = TrainerFontFamily.Mono;

    public LineSpacing LineSpacing { get; init; } = LineSpacing.Comfortable;

    public AppTheme Theme { get; init; } = AppTheme.Light;

    public bool ShowLiveMetrics { get; init; } = true;
}
