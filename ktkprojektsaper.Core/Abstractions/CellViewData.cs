using ktkprojektsaper.Core.Models;

namespace ktkprojektsaper.Core.Abstractions;

/// <summary>
/// Dane komórki niezależne od technologii UI (konsola / GUI).
/// </summary>
public readonly record struct CellViewData(
    int X,
    int Y,
    CellState State,
    bool IsMine,
    int AdjacentMines,
    bool IsCursor);
