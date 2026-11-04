using ktkprojektsaper.Core.Models;

namespace ktkprojektsaper.Core.Abstractions;

public readonly record struct CellViewData(
    int X,
    int Y,
    CellState State,
    bool IsMine,
    int AdjacentMines,
    bool IsCursor);
