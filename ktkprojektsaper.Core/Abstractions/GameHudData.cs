using ktkprojektsaper.Core.Models;

namespace ktkprojektsaper.Core.Abstractions;

public sealed class GameHudData
{
    public required string DifficultyName { get; init; }
    public required int RemainingMines { get; init; }
    public required TimeSpan Elapsed { get; init; }
    public required GameStatus Status { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
}
