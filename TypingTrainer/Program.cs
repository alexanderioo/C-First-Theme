using TypingTrainer.Components;
using TypingTrainer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<DictionaryRepository>();
builder.Services.AddSingleton<SettingsRepository>();
builder.Services.AddSingleton<StatisticsRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
