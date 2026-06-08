namespace TypingTrainer.Models;

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
    public static UserSettings Default { get; } = new();

    public int FontSize { get; init; } = 28;

    public TrainerFontFamily FontFamily { get; init; } = TrainerFontFamily.Mono;

    public LineSpacing LineSpacing { get; init; } = LineSpacing.Comfortable;

    public AppTheme Theme { get; init; } = AppTheme.Light;

    public bool ShowLiveMetrics { get; init; } = true;
}
