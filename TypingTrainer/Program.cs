using System.Text;
using TypingTrainer.ConsoleUI;
using TypingTrainer.Services;

// UTF-8 нужен, чтобы русский текст правильно читался и отображался в Terminal.
Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
Console.Title = "Клавиши — тренажёр печати";

string dataDirectory = DataDirectoryProvider.GetDataDirectory();

var application = new ConsoleApplication(
    new DictionaryRepository(dataDirectory),
    new SettingsRepository(dataDirectory),
    new StatisticsRepository(dataDirectory));

await application.RunAsync();
