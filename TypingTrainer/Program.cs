using System.Text;
using TypingTrainer;
using TypingTrainer.Services;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
Console.Title = "Тренажёр набора текста";

var repository = new JsonResultRepository();
var application = new TypingTrainerApplication(
    new TextProvider(),
    repository,
    new TypingSession());

application.Run();
