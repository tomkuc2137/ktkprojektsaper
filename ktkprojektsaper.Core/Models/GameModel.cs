namespace ktkprojektsaper.Core.Models;

public sealed class GameModel
{
    private readonly Random _random;
    private bool _minesPlaced;
    private DateTime? _startedAt;
    private DateTime? _finishedAt;

    public Board Board { get; private set; }
    public Difficulty Difficulty { get; private set; }
    public GameStatus Status { get; private set; } = GameStatus.Ready;
    public int CursorX { get; private set; }
    public int CursorY { get; private set; }

    public int RemainingMines => Difficulty.MineCount - Board.CountFlags();

    public TimeSpan Elapsed
    {
        get
        {
            if (_startedAt is null)
                return TimeSpan.Zero;
            var end = _finishedAt ?? DateTime.UtcNow;
            return end - _startedAt.Value;
        }
    }

    public GameModel(Difficulty difficulty, Random? random = null)
    {
        Difficulty = difficulty;
        Board = new Board(difficulty.Width, difficulty.Height, difficulty.MineCount);
        _random = random ?? Random.Shared;
        CursorX = difficulty.Width / 2;
        CursorY = difficulty.Height / 2;
    }

    public void MoveCursor(int dx, int dy)
    {
        if (Status is GameStatus.Won or GameStatus.Lost)
            return;

        CursorX = Math.Clamp(CursorX + dx, 0, Board.Width - 1);
        CursorY = Math.Clamp(CursorY + dy, 0, Board.Height - 1);
    }

    public void SetCursor(int x, int y)
    {
        if (!Board.InBounds(x, y))
            return;

        CursorX = x;
        CursorY = y;
    }

    /// <summary>
    /// Odkrywa pole. Zwraca listę zmienionych współrzędnych (do częściowego odświeżenia widoku).
    /// </summary>
    public IReadOnlyList<(int X, int Y)> RevealAtCursor()
    {
        return Reveal(CursorX, CursorY);
    }

    public IReadOnlyList<(int X, int Y)> Reveal(int x, int y)
    {
        if (Status is GameStatus.Won or GameStatus.Lost)
            return [];

        if (!Board.InBounds(x, y))
            return [];

        var cell = Board[x, y];
        if (cell.IsFlagged || cell.IsRevealed)
            return [];

        if (!_minesPlaced)
        {
            Board.PlaceMines(x, y, _random);
            _minesPlaced = true;
            _startedAt = DateTime.UtcNow;
            Status = GameStatus.Playing;
        }

        var changed = new List<(int X, int Y)>();
        FloodReveal(x, y, changed);

        if (Board[x, y].IsMine && Board[x, y].IsRevealed)
        {
            RevealAllMines(changed);
            Status = GameStatus.Lost;
            _finishedAt = DateTime.UtcNow;
            return changed;
        }

        if (Board.CountHiddenSafeCells() == 0)
        {
            Status = GameStatus.Won;
            _finishedAt = DateTime.UtcNow;
            FlagRemainingMines(changed);
        }

        return changed;
    }

    /// <summary>
    /// Przełącza flagę. Zwraca zmienione pole lub pustą listę.
    /// </summary>
    public IReadOnlyList<(int X, int Y)> ToggleFlagAtCursor()
    {
        if (Status is GameStatus.Won or GameStatus.Lost)
            return [];

        var cell = Board[CursorX, CursorY];
        if (cell.IsRevealed)
            return [];

        cell.State = cell.IsFlagged ? CellState.Hidden : CellState.Flagged;
        return [(CursorX, CursorY)];
    }

    private void FloodReveal(int startX, int startY, List<(int X, int Y)> changed)
    {
        var stack = new Stack<(int X, int Y)>();
        stack.Push((startX, startY));

        while (stack.Count > 0)
        {
            var (x, y) = stack.Pop();
            if (!Board.InBounds(x, y))
                continue;

            var cell = Board[x, y];
            if (cell.IsRevealed || cell.IsFlagged)
                continue;

            cell.State = CellState.Revealed;
            changed.Add((x, y));

            if (cell.IsMine || cell.AdjacentMines > 0)
                continue;

            foreach (var n in Board.Neighbors(x, y))
                stack.Push(n);
        }
    }

    private void RevealAllMines(List<(int X, int Y)> changed)
    {
        for (var x = 0; x < Board.Width; x++)
        for (var y = 0; y < Board.Height; y++)
        {
            var cell = Board[x, y];
            if (cell.IsMine && !cell.IsRevealed)
            {
                cell.State = CellState.Revealed;
                changed.Add((x, y));
            }
        }
    }

    private void FlagRemainingMines(List<(int X, int Y)> changed)
    {
        for (var x = 0; x < Board.Width; x++)
        for (var y = 0; y < Board.Height; y++)
        {
            var cell = Board[x, y];
            if (cell.IsMine && !cell.IsFlagged)
            {
                cell.State = CellState.Flagged;
                changed.Add((x, y));
            }
        }
    }
}
