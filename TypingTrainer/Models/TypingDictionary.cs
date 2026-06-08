namespace TypingTrainer.Models;

public sealed record TypingDictionary
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Language { get; init; }

    public DictionaryKind Kind { get; init; }

    public required string Difficulty { get; init; }

    public bool IsBuiltIn { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public List<string> Entries { get; init; } = [];

    public int CharacterCount => Entries.Sum(entry => entry.Length);
}
