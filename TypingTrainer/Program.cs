using TypingTrainer.Components;
using TypingTrainer.Services;

// WebApplicationBuilder собирает конфигурацию приложения, логирование
// и контейнер зависимостей, из которого Blazor получает нужные сервисы.
var builder = WebApplication.CreateBuilder(args);

// Interactive Server означает, что Razor-компоненты выполняют C# на сервере,
// а браузер получает обновления интерфейса через постоянное соединение.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

// Singleton создаёт один экземпляр репозитория на всё время работы приложения.
// Это удобно здесь: все страницы работают с одними JSON-файлами и блокировками.
builder.Services.AddSingleton<DictionaryRepository>();
builder.Services.AddSingleton<SettingsRepository>();
builder.Services.AddSingleton<StatisticsRepository>();

var app = builder.Build();

// В рабочей среде показываем пользователю страницу ошибки вместо технического
// исключения и просим браузер в дальнейшем использовать защищённое соединение.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Antiforgery защищает формы от поддельных запросов с других сайтов.
app.UseAntiforgery();

// Публикуем CSS, favicon и служебные файлы Blazor из wwwroot.
app.MapStaticAssets();

// App.razor становится корневым компонентом всего интерфейса.
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Запускаем веб-сервер и ожидаем запросы, пока пользователь не нажмёт Ctrl+C.
app.Run();
