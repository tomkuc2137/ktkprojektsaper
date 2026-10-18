using ktkprojektsaper.Core.Models;

namespace ktkprojektsaper.Core.Services;

public interface IScoreStore
{
    IReadOnlyList<ScoreEntry> Load();
    void Add(ScoreEntry entry);
}
