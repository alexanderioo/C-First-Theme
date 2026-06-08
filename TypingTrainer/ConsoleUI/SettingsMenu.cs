using TypingTrainer.Models;
using TypingTrainer.Services;

namespace TypingTrainer.ConsoleUI;

public sealed class SettingsMenu
{
    private readonly SettingsRepository _repository;

    public SettingsMenu(SettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task ShowAsync()
    {
        // Настройки изменяются в локальной переменной record и после каждого
        // действия записываются в settings.json.
        UserSettings settings = await _repository.GetAsync();
        bool isOpen = true;

        while (isOpen)
        {
            ConsoleTheme.Apply(settings);
            ConsoleTheme.Heading("Настройки");
            Console.WriteLine($"1. Тема: {GetThemeName(settings.Theme)}");
            Console.WriteLine($"2. Живые показатели: {(settings.ShowLiveMetrics ? "включены" : "выключены")}");
            Console.WriteLine($"3. Ширина текста: {settings.TextWidth} символов");
            Console.WriteLine($"4. Обратный отсчёт: {settings.CountdownSeconds} сек.");
            Console.WriteLine("5. Как изменить размер шрифта Terminal");
            Console.WriteLine("0. Назад");
            Console.WriteLine();

            int choice = ConsoleInput.ReadMenuChoice("Ваш выбор: ", 0, 5);
            switch (choice)
            {
                case 1:
                    settings = settings with { Theme = SelectTheme(settings.Theme) };
                    await SaveAsync(settings);
                    break;
                case 2:
                    // Инвертируем логическое значение true/false.
                    settings = settings with { ShowLiveMetrics = !settings.ShowLiveMetrics };
                    await SaveAsync(settings);
                    break;
                case 3:
                    settings = settings with
                    {
                        TextWidth = ConsoleInput.ReadNumber(
                            "Новая ширина текста",
                            40,
                            120,
                            settings.TextWidth)
                    };
                    await SaveAsync(settings);
                    break;
                case 4:
                    settings = settings with
                    {
                        CountdownSeconds = ConsoleInput.ReadNumber(
                            "Секунд перед стартом",
                            0,
                            5,
                            settings.CountdownSeconds)
                    };
                    await SaveAsync(settings);
                    break;
                case 5:
                    ShowFontHelp();
                    break;
                case 0:
                    isOpen = false;
                    break;
            }
        }
    }

    private async Task SaveAsync(UserSettings settings)
    {
        await _repository.SaveAsync(settings);
        ConsoleTheme.Success("Настройки сохранены.");
        ConsoleInput.WaitForEnter();
    }

    private static AppTheme SelectTheme(AppTheme current)
    {
        Console.WriteLine();
        Console.WriteLine("1. Светлая");
        Console.WriteLine("2. Тёмная");
        ConsoleTheme.Muted($"Текущее значение: {GetThemeName(current)}");

        return ConsoleInput.ReadMenuChoice("Выберите тему: ", 1, 2) switch
        {
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => current
        };
    }

    private static void ShowFontHelp()
    {
        ConsoleTheme.Clear();
        ConsoleTheme.Heading("Размер шрифта Terminal");
        Console.WriteLine("Консольная программа не может сама менять системный");
        Console.WriteLine("размер шрифта окна Terminal. Это настройка приложения macOS.");
        Console.WriteLine();
        Console.WriteLine("На Mac используйте:");
        Console.WriteLine("  Cmd + Plus  — увеличить шрифт");
        Console.WriteLine("  Cmd + Minus — уменьшить шрифт");
        Console.WriteLine("  Cmd + 0     — вернуть обычный масштаб");
        ConsoleInput.WaitForEnter();
    }

    private static string GetThemeName(AppTheme theme) => theme switch
    {
        AppTheme.Dark => "Тёмная",
        _ => "Светлая"
    };
}
