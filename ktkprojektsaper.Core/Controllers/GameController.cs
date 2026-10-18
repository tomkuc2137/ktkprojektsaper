using ktkprojektsaper.Core.Abstractions;
using ktkprojektsaper.Core.Models;
using ktkprojektsaper.Core.Services;

namespace ktkprojektsaper.Core.Controllers;

/// <summary>
/// Kontroler partii gry – łączy model z widokiem, bez zależności od konsoli.
/// </summary>
public sealed class GameController
{
    private readonly IGameView _view;
    private readonly IGameInput _input;
    private readonly IScoreStore _scoreStore;
    private GameModel _game;

    public GameController(Difficulty difficulty, IGameView view, IGameInput input, IScoreStore scoreStore)
    {
        _view = view;
        _input = input;
        _scoreStore = scoreStore;
        _game = new GameModel(difficulty);
    }

    /// <summary>
    /// Pętla gry. Zwraca true, jeśli użytkownik chce wrócić do menu.
    /// </summary>
    public bool Run()
    {
        DrawFullBoard();
        // Enter z menu / śmieci po Clear nie mogą od razu zamknąć partii.
        _input.Flush();

        while (true)
        {
            var action = _input.ReadGameAction(TimeSpan.FromMilliseconds(250));
            switch (action)
            {
                case GameInputAction.None:
                    if (_game.Status == GameStatus.Playing)
                        _view.UpdateHud(BuildHud());
                    break;
                case GameInputAction.MoveUp:
                    MoveCursor(0, -1);
                    break;
                case GameInputAction.MoveDown:
                    MoveCursor(0, 1);
                    break;
                case GameInputAction.MoveLeft:
                    MoveCursor(-1, 0);
                    break;
                case GameInputAction.MoveRight:
                    MoveCursor(1, 0);
                    break;
                case GameInputAction.Reveal:
                    ApplyPointerTargetIfAny();
                    HandleReveal();
                    break;
                case GameInputAction.ToggleFlag:
                    ApplyPointerTargetIfAny();
                    HandleFlag();
                    break;
                case GameInputAction.Restart:
                    _game = new GameModel(_game.Difficulty);
                    DrawFullBoard();
                    break;
                case GameInputAction.QuitToMenu:
                    return true;
            }
        }
    }

    private void MoveCursor(int dx, int dy)
    {
        if (_game.Status is GameStatus.Won or GameStatus.Lost)
            return;

        var prevX = _game.CursorX;
        var prevY = _game.CursorY;
        _game.MoveCursor(dx, dy);

        if (prevX == _game.CursorX && prevY == _game.CursorY)
            return;

        var previous = ToCellView(prevX, prevY, isCursor: false);
        var current = ToCellView(_game.CursorX, _game.CursorY, isCursor: true);
        _view.UpdateCursor(previous, current);
    }

    private void ApplyPointerTargetIfAny()
    {
        if (!_input.TryConsumeCellTarget(out var x, out var y))
            return;

        if (_game.Status is GameStatus.Won or GameStatus.Lost)
            return;

        var prevX = _game.CursorX;
        var prevY = _game.CursorY;
        _game.SetCursor(x, y);

        if (prevX == _game.CursorX && prevY == _game.CursorY)
            return;

        _view.UpdateCursor(
            ToCellView(prevX, prevY, isCursor: false),
            ToCellView(_game.CursorX, _game.CursorY, isCursor: true));
    }

    private void HandleReveal()
    {
        if (_game.Status is GameStatus.Won or GameStatus.Lost)
            return;

        var changed = _game.RevealAtCursor();
        if (changed.Count == 0)
            return;

        RefreshChanged(changed);
        _view.UpdateHud(BuildHud());

        if (_game.Status == GameStatus.Won)
            OnWon();
        else if (_game.Status == GameStatus.Lost)
            _view.ShowMessage("Boom! Wpadłeś na minę. R – nowa gra, Esc – menu.");
    }

    private void HandleFlag()
    {
        if (_game.Status is GameStatus.Won or GameStatus.Lost)
            return;

        var changed = _game.ToggleFlagAtCursor();
        if (changed.Count == 0)
            return;

        RefreshChanged(changed);
        _view.UpdateHud(BuildHud());
    }

    private void OnWon()
    {
        _view.ShowMessage($"Wygrana! Czas: {_game.Elapsed:mm\\:ss}");
        var name = _view.AskPlayerName();
        if (!string.IsNullOrWhiteSpace(name))
        {
            _scoreStore.Add(new ScoreEntry
            {
                PlayerName = name.Trim(),
                DifficultyName = _game.Difficulty.Name,
                Duration = _game.Elapsed,
                PlayedAt = DateTime.Now
            });
            _view.ShowMessage("Wynik zapisany. R – nowa gra, Esc – menu.");
        }
        else
        {
            _view.ShowMessage("R – nowa gra, Esc – menu.");
        }
    }

    private void RefreshChanged(IReadOnlyList<(int X, int Y)> positions)
    {
        var cells = positions
            .Distinct()
            .Select(p => ToCellView(p.X, p.Y, p.X == _game.CursorX && p.Y == _game.CursorY))
            .ToList();

        _view.UpdateCells(cells);
    }

    private void DrawFullBoard()
    {
        var cells = new List<CellViewData>(_game.Board.Width * _game.Board.Height);
        for (var y = 0; y < _game.Board.Height; y++)
        for (var x = 0; x < _game.Board.Width; x++)
            cells.Add(ToCellView(x, y, x == _game.CursorX && y == _game.CursorY));

        _view.ShowGame(BuildHud(), cells);
    }

    private GameHudData BuildHud() => new()
    {
        DifficultyName = _game.Difficulty.Name,
        RemainingMines = _game.RemainingMines,
        Elapsed = _game.Elapsed,
        Status = _game.Status,
        Width = _game.Board.Width,
        Height = _game.Board.Height
    };

    private CellViewData ToCellView(int x, int y, bool isCursor)
    {
        var cell = _game.Board[x, y];
        return new CellViewData(x, y, cell.State, cell.IsMine, cell.AdjacentMines, isCursor);
    }
}
