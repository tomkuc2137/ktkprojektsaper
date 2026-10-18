namespace ktkprojektsaper.Core.Models;

public sealed class Board
{
    private readonly Cell[,] _cells;

    public int Width { get; }
    public int Height { get; }
    public int MineCount { get; }

    public Board(int width, int height, int mineCount)
    {
        Width = width;
        Height = height;
        MineCount = mineCount;
        _cells = new Cell[width, height];

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
            _cells[x, y] = new Cell();
    }

    public Cell this[int x, int y] => _cells[x, y];

    public bool InBounds(int x, int y) =>
        x >= 0 && y >= 0 && x < Width && y < Height;

    public IEnumerable<(int X, int Y)> Neighbors(int x, int y)
    {
        for (var dx = -1; dx <= 1; dx++)
        for (var dy = -1; dy <= 1; dy++)
        {
            if (dx == 0 && dy == 0)
                continue;

            var nx = x + dx;
            var ny = y + dy;
            if (InBounds(nx, ny))
                yield return (nx, ny);
        }
    }

    public void PlaceMines(int safeX, int safeY, Random random)
    {
        var forbidden = new HashSet<(int, int)> { (safeX, safeY) };
        foreach (var n in Neighbors(safeX, safeY))
            forbidden.Add(n);

        var placed = 0;
        while (placed < MineCount)
        {
            var x = random.Next(Width);
            var y = random.Next(Height);
            if (forbidden.Contains((x, y)) || _cells[x, y].IsMine)
                continue;

            _cells[x, y].IsMine = true;
            placed++;
        }

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            if (_cells[x, y].IsMine)
                continue;

            _cells[x, y].AdjacentMines = Neighbors(x, y).Count(n => _cells[n.X, n.Y].IsMine);
        }
    }

    public int CountFlags()
    {
        var count = 0;
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
            if (_cells[x, y].IsFlagged)
                count++;
        return count;
    }

    public int CountHiddenSafeCells()
    {
        var count = 0;
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            var cell = _cells[x, y];
            if (!cell.IsMine && !cell.IsRevealed)
                count++;
        }
        return count;
    }
}
