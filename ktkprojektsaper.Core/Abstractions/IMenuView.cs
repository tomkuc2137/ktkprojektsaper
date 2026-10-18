using ktkprojektsaper.Core.Models;

namespace ktkprojektsaper.Core.Abstractions;

public interface IMenuView
{
    int ShowMenu(string title, IReadOnlyList<string> options, int selectedIndex = 0);
    Difficulty? ShowCustomDifficulty();
    void ShowScores(IReadOnlyList<ScoreEntry> scores);
    void ShowInfo(string title, string message);
}
