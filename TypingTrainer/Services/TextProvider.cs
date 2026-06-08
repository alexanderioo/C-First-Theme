namespace TypingTrainer.Services;

public sealed class TextProvider
{
    private readonly Dictionary<(TrainingLanguage Language, Difficulty Difficulty), string[]> _texts = new()
    {
        [(TrainingLanguage.Russian, Difficulty.Easy)] =
        [
            "Мама мыла раму а кот сидел у окна.",
            "Сегодня ясный день и светит солнце.",
            "Доброе слово помогает людям дружить."
        ],
        [(TrainingLanguage.Russian, Difficulty.Medium)] =
        [
            "Регулярная практика помогает печатать быстрее, точнее и увереннее.",
            "За окном шумел летний дождь, а в комнате тихо играла музыка.",
            "Клавиатурный тренажёр запоминает результаты каждой завершённой попытки."
        ],
        [(TrainingLanguage.Russian, Difficulty.Hard)] =
        [
            "В 2026 году цифровая грамотность включает не только скорость, но и точность: исправлять ошибки всегда дольше.",
            "Программист написал: «Хороший код должен быть понятен человеку», — а затем добавил ещё 3 коротких теста.",
            "Съешь ещё этих мягких французских булок, да выпей чаю; после этого повтори упражнение на 100% внимательно."
        ],
        [(TrainingLanguage.English, Difficulty.Easy)] =
        [
            "The sun is bright and the sky is blue.",
            "Practice every day to type with confidence.",
            "A small step can lead to a great result."
        ],
        [(TrainingLanguage.English, Difficulty.Medium)] =
        [
            "Regular practice improves typing speed, accuracy, and confidence.",
            "A quiet workspace helps you focus on every letter and punctuation mark.",
            "The typing trainer saves the result of every completed exercise."
        ],
        [(TrainingLanguage.English, Difficulty.Hard)] =
        [
            "In 2026, digital literacy requires both speed and precision: fixing mistakes always takes extra time.",
            "The developer wrote, \"Readable code matters,\" and then added 3 focused tests before lunch.",
            "Typing at 100% accuracy isn't easy; however, patient practice makes difficult tasks manageable."
        ]
    };

    public string GetRandom(TrainingLanguage language, Difficulty difficulty)
    {
        if (!_texts.TryGetValue((language, difficulty), out string[]? texts))
        {
            throw new ArgumentException("Для выбранных параметров нет учебных текстов.");
        }

        return texts[Random.Shared.Next(texts.Length)];
    }
}
