namespace TypingTrainer.Services;

public interface IResultRepository
{
    IReadOnlyList<SessionResult> Load();

    void Add(SessionResult result);
}
