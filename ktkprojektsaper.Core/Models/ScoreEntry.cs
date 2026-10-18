namespace ktkprojektsaper.Core.Models;

public sealed class ScoreEntry
{
    public string PlayerName { get; init; } = string.Empty;
    public string DifficultyName { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
    public DateTime PlayedAt { get; init; }
}
