# Как устроен проект

## 1. Точка входа

Изучение начинается с `TypingTrainer/Program.cs`.

Там настраивается UTF-8, определяется папка JSON-файлов и создаётся
`ConsoleApplication`.

## 2. Главное меню

`ConsoleApplication` содержит основной цикл программы:

```csharp
while (isRunning)
{
    // вывести меню
    // прочитать пункт
    // вызвать нужный раздел
}
```

Каждый крупный раздел вынесен в отдельный класс из папки `ConsoleUI`.

## 3. Словари

`DictionaryMenu` показывает список и собирает данные пользователя.

`DictionaryRepository` отвечает за JSON. Такое разделение важно:

- меню отвечает за общение с человеком;
- репозиторий отвечает за файл;
- `TypingDictionary` хранит данные.

## 4. Тренировка

Главный файл — `ConsoleUI/TypingSession.cs`.

Метод `Console.ReadKey(intercept: true)` получает каждую клавишу отдельно.
Введённые символы хранятся в `List<char>`.

Новый символ сравнивается с символом исходного текста на той же позиции:

```csharp
char expectedCharacter = targetText[typedCharacters.Count];
```

При несовпадении увеличивается счётчик ошибок. `Backspace` удаляет символ,
но не уменьшает количество уже совершённых ошибок.

## 5. Результат

Формулы находятся в `Models/RaceResult.cs`.

`TypingSession` передаёт туда длительность, количество символов, нажатий
и ошибок, а получает готовый неизменяемый объект результата.

## 6. Статистика

`StatisticsRepository` сохраняет каждый `RaceResult`.

`StatisticsSummary.From` применяет LINQ:

- `Sum` считает сумму;
- `Max` находит рекорд;
- `Average` вычисляет среднее;
- `GroupBy` объединяет заезды по словарям.

## 7. Что изучать по порядку

1. `Program.cs`
2. `ConsoleUI/ConsoleApplication.cs`
3. `ConsoleUI/DictionaryMenu.cs`
4. `ConsoleUI/TypingSession.cs`
5. `Models/RaceResult.cs`
6. `Services/StatisticsRepository.cs`
7. `ConsoleUI/StatisticsMenu.cs`
