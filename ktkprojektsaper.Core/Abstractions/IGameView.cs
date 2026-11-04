using ktkprojektsaper.Core.Models;

namespace ktkprojektsaper.Core.Abstractions;

public interface IGameView
{
    void ShowGame(GameHudData hud, IReadOnlyList<CellViewData> allCells);
    void UpdateCells(IReadOnlyList<CellViewData> changedCells);
    void UpdateCursor(CellViewData previous, CellViewData current);
    void UpdateHud(GameHudData hud);
    void ShowMessage(string message);
    string? AskPlayerName();
}
