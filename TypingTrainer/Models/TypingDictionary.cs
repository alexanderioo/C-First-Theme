namespace TypingTrainer.Models;

// record хорошо подходит для модели данных: у него есть сравнение по значениям,
// а свойства init можно задать при создании, но нельзя случайно поменять позже.
public sealed record TypingDictionary
{
    // Guid — уникальный идентификатор, который используется прямо в URL заезда.
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Language { get; init; }

    public DictionaryKind Kind { get; init; }

    public required string Difficulty { get; init; }

    public bool IsBuiltIn { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public List<string> Entries { get; init; } = [];

    // Вычисляемое свойство не хранится отдельно: сумма считается из заданий.
    public int CharacterCount => Entries.Sum(entry => entry.Length);
}
