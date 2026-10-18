namespace ktkprojektsaper.Core.Models;

public sealed class Difficulty
{
    public string Name { get; }
    public int Width { get; }
    public int Height { get; }
    public int MineCount { get; }

    public Difficulty(string name, int width, int height, int mineCount)
    {
        if (width < 5 || height < 5)
            throw new ArgumentException("Minimalny rozmiar planszy to 5x5.");
        if (mineCount < 1 || mineCount >= width * height)
            throw new ArgumentException("Liczba min musi być większa od 0 i mniejsza niż liczba pól.");

        Name = name;
        Width = width;
        Height = height;
        MineCount = mineCount;
    }

    public static Difficulty Beginner { get; } = new("Łatwy", 9, 9, 10);
    public static Difficulty Intermediate { get; } = new("Średni", 16, 16, 40);
    public static Difficulty Expert { get; } = new("Trudny", 30, 16, 99);

    public static IReadOnlyList<Difficulty> Presets { get; } =
    [
        Beginner,
        Intermediate,
        Expert
    ];
}
