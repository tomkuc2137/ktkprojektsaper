namespace ktkprojektsaper.Core.Models;

public sealed class Cell
{
    public bool IsMine { get; set; }
    public int AdjacentMines { get; set; }
    public CellState State { get; set; } = CellState.Hidden;

    public bool IsHidden => State == CellState.Hidden;
    public bool IsRevealed => State == CellState.Revealed;
    public bool IsFlagged => State == CellState.Flagged;
}
