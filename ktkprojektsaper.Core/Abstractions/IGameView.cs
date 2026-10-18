using ktkprojektsaper.Core.Models;

namespace ktkprojektsaper.Core.Abstractions;

/// <summary>
/// Widok gry – implementacja w konsoli lub później w GUI.
/// Częściowe odświeżanie: UpdateCells / UpdateCursor zamiast pełnego redraw.
/// </summary>
public interface IGameView
{
    void ShowGame(GameHudData hud, IReadOnlyList<CellViewData> allCells);
    void UpdateCells(IReadOnlyList<CellViewData> changedCells);
    void UpdateCursor(CellViewData previous, CellViewData current);
    void UpdateHud(GameHudData hud);
    void ShowMessage(string message);
    string? AskPlayerName();
}
