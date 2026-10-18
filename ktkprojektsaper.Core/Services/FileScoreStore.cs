using System.Globalization;
using System.Text;
using ktkprojektsaper.Core.Models;

namespace ktkprojektsaper.Core.Services;

public sealed class FileScoreStore : IScoreStore
{
    private readonly string _filePath;
    private const int MaxEntries = 20;

    public FileScoreStore(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ktkprojektsaper",
            "scores.csv");
    }

    public IReadOnlyList<ScoreEntry> Load()
    {
        if (!File.Exists(_filePath))
            return [];

        var result = new List<ScoreEntry>();
        foreach (var line in File.ReadAllLines(_filePath, Encoding.UTF8))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(';');
            if (parts.Length < 4)
                continue;

            if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
                continue;
            if (!DateTime.TryParse(parts[3], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var playedAt))
                continue;

            result.Add(new ScoreEntry
            {
                PlayerName = parts[0],
                DifficultyName = parts[1],
                Duration = TimeSpan.FromSeconds(seconds),
                PlayedAt = playedAt
            });
        }

        return result
            .OrderBy(s => s.Duration)
            .ThenByDescending(s => s.PlayedAt)
            .Take(MaxEntries)
            .ToList();
    }

    public void Add(ScoreEntry entry)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var existing = Load().ToList();
        existing.Add(entry);

        var top = existing
            .OrderBy(s => s.Duration)
            .ThenByDescending(s => s.PlayedAt)
            .Take(MaxEntries)
            .ToList();

        var lines = top.Select(s => string.Join(';',
            Escape(s.PlayerName),
            Escape(s.DifficultyName),
            s.Duration.TotalSeconds.ToString("F1", CultureInfo.InvariantCulture),
            s.PlayedAt.ToString("o", CultureInfo.InvariantCulture)));

        File.WriteAllLines(_filePath, lines, Encoding.UTF8);
    }

    private static string Escape(string value) =>
        value.Replace(';', ',').Replace('\n', ' ').Trim();
}
