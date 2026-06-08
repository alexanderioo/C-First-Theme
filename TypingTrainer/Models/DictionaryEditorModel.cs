using System.ComponentModel.DataAnnotations;

namespace TypingTrainer.Models;

public sealed class DictionaryEditorModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Введите название словаря.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "Название должно содержать от 2 до 60 символов.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Добавьте короткое описание.")]
    [StringLength(180, MinimumLength = 5, ErrorMessage = "Описание должно содержать от 5 до 180 символов.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите язык.")]
    public string Language { get; set; } = "Русский";

    public DictionaryKind Kind { get; set; } = DictionaryKind.Texts;

    [Required(ErrorMessage = "Укажите сложность.")]
    public string Difficulty { get; set; } = "Средняя";

    [Required(ErrorMessage = "Добавьте хотя бы один текст.")]
    public string EntriesText { get; set; } = string.Empty;

    public bool IsBuiltIn { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TypingDictionary ToDictionary()
    {
        List<string> entries = EntriesText
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(entry => entry.Length >= 2)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return new TypingDictionary
        {
            Id = Id ?? Guid.NewGuid(),
            Name = Name.Trim(),
            Description = Description.Trim(),
            Language = Language.Trim(),
            Kind = Kind,
            Difficulty = Difficulty.Trim(),
            IsBuiltIn = IsBuiltIn,
            CreatedAt = CreatedAt,
            Entries = entries
        };
    }

    public static DictionaryEditorModel From(TypingDictionary dictionary)
    {
        return new DictionaryEditorModel
        {
            Id = dictionary.Id,
            Name = dictionary.Name,
            Description = dictionary.Description,
            Language = dictionary.Language,
            Kind = dictionary.Kind,
            Difficulty = dictionary.Difficulty,
            EntriesText = string.Join(Environment.NewLine, dictionary.Entries),
            IsBuiltIn = dictionary.IsBuiltIn,
            CreatedAt = dictionary.CreatedAt
        };
    }
}
