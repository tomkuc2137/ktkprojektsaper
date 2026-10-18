using ktkprojektsaper.Core.Abstractions;
using ktkprojektsaper.Core.Models;

namespace ktkprojektsaper.Console.Views;

/// <summary>
/// Widok konsolowy z częściowym odświeżaniem – rysuje tylko zmienione komórki / HUD.
/// </summary>
public sealed class ConsoleGameView : IGameView
{
    private const int CellWidth = 2;
    private const int BoardOriginX = 1;
    private const int BoardOriginY = 3;
    private const int HudY = 0;
    private const int MessageYOffset = 2;

    private int _boardHeight;

    public void ShowGame(GameHudData hud, IReadOnlyList<CellViewData> allCells)
    {
        System.Console.CursorVisible = false;
        _boardHeight = hud.Height;

        var neededWidth = Math.Max(hud.Width * CellWidth + 4, 80);
        var neededHeight = BoardOriginY + hud.Height + MessageYOffset + 4;
        EnsureConsoleSize(neededWidth, neededHeight);

        try
        {
            System.Console.Clear();
        }
        catch (IOException)
        {
        }

        WriteHud(hud);

        if (TrySetCursor(0, BoardOriginY - 1))
            WriteClamped(new string('─', hud.Width * CellWidth + 2));

        foreach (var cell in allCells)
            DrawCell(cell);

        if (TrySetCursor(0, BoardOriginY + hud.Height))
            WriteClamped(new string('─', hud.Width * CellWidth + 2));

        WriteHelpLine(hud.Height);
        WriteMessage(hud.Height, "Gotowy. Enter – odkryj, F – flaga.");
    }

    public void UpdateCells(IReadOnlyList<CellViewData> changedCells)
    {
        foreach (var cell in changedCells)
            DrawCell(cell);
    }

    public void UpdateCursor(CellViewData previous, CellViewData current)
    {
        DrawCell(previous);
        DrawCell(current);
    }

    public void UpdateHud(GameHudData hud)
    {
        WriteHud(hud);
    }

    public void ShowMessage(string message)
    {
        WriteMessage(_boardHeight, message);
    }

    public string? AskPlayerName()
    {
        var y = BoardOriginY + _boardHeight + MessageYOffset + 1;
        EnsureConsoleSize(80, y + 2);

        if (!TrySetCursor(0, y))
            return null;

        ClearLine();
        System.Console.CursorVisible = true;
        WriteClamped("Podaj imię (Enter = pomiń): ");
        var name = System.Console.ReadLine();
        System.Console.CursorVisible = false;

        if (TrySetCursor(0, y))
            ClearLine();

        return string.IsNullOrWhiteSpace(name) ? null : name.Trim();
    }

    private void DrawCell(CellViewData cell)
    {
        var left = BoardOriginX + cell.X * CellWidth;
        var top = BoardOriginY + cell.Y;
        if (!TrySetCursor(left, top))
            return;

        var (text, fg, bg) = ResolveStyle(cell);
        System.Console.ForegroundColor = fg;
        System.Console.BackgroundColor = bg;
        WriteClamped(text.PadRight(CellWidth));
        System.Console.ResetColor();
    }

    private static (string Text, ConsoleColor Fg, ConsoleColor Bg) ResolveStyle(CellViewData cell)
    {
        var bg = cell.IsCursor ? ConsoleColor.DarkCyan : ConsoleColor.Black;

        if (cell.State == CellState.Flagged)
            return ("F ", ConsoleColor.Yellow, bg);

        if (cell.State == CellState.Hidden)
            return (cell.IsCursor ? "■ " : "□ ", ConsoleColor.DarkGray, bg);

        if (cell.IsMine)
            return ("* ", ConsoleColor.Red, ConsoleColor.DarkRed);

        if (cell.AdjacentMines == 0)
            return ("  ", ConsoleColor.Gray, bg);

        var color = cell.AdjacentMines switch
        {
            1 => ConsoleColor.Blue,
            2 => ConsoleColor.Green,
            3 => ConsoleColor.Red,
            4 => ConsoleColor.DarkBlue,
            5 => ConsoleColor.DarkRed,
            6 => ConsoleColor.Cyan,
            7 => ConsoleColor.DarkMagenta,
            _ => ConsoleColor.Gray
        };

        return ($"{cell.AdjacentMines} ", color, bg);
    }

    private void WriteHud(GameHudData hud)
    {
        if (!TrySetCursor(0, HudY))
            return;

        ClearLine();
        System.Console.ForegroundColor = ConsoleColor.White;
        WriteClamped(
            $" SAPER  |  {hud.DifficultyName}  |  Miny: {hud.RemainingMines,3}  |  Czas: {hud.Elapsed:mm\\:ss}  |  {StatusLabel(hud.Status)}");
        System.Console.ResetColor();
    }

    private void WriteHelpLine(int boardHeight)
    {
        var y = BoardOriginY + boardHeight + 1;
        if (!TrySetCursor(0, y))
            return;
        ClearLine();
        System.Console.ForegroundColor = ConsoleColor.DarkGray;
        WriteClamped(" Strzałki: ruch  |  Enter: odkryj  |  F: flaga  |  R: restart  |  Esc: menu");
        System.Console.ResetColor();
    }

    private void WriteMessage(int boardHeight, string message)
    {
        var y = BoardOriginY + boardHeight + MessageYOffset;
        if (!TrySetCursor(0, y))
            return;
        ClearLine();
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        WriteClamped(" " + message);
        System.Console.ResetColor();
    }

    private static string StatusLabel(GameStatus status) => status switch
    {
        GameStatus.Ready => "Gotowy",
        GameStatus.Playing => "Gra",
        GameStatus.Won => "WYGRANA",
        GameStatus.Lost => "PRZEGRANA",
        _ => status.ToString()
    };

    private static void EnsureConsoleSize(int minWidth, int minHeight)
    {
        if (!OperatingSystem.IsWindows())
            return;

        try
        {
            var width = Math.Max(minWidth, System.Console.WindowWidth);
            var height = Math.Max(minHeight, System.Console.WindowHeight);

            if (System.Console.BufferWidth < width || System.Console.BufferHeight < height)
            {
                System.Console.SetBufferSize(
                    Math.Max(System.Console.BufferWidth, width),
                    Math.Max(System.Console.BufferHeight, height));
            }

            if (System.Console.WindowWidth < Math.Min(width, System.Console.LargestWindowWidth) ||
                System.Console.WindowHeight < Math.Min(height, System.Console.LargestWindowHeight))
            {
                var winW = Math.Min(Math.Max(System.Console.WindowWidth, width), System.Console.LargestWindowWidth);
                var winH = Math.Min(Math.Max(System.Console.WindowHeight, height), System.Console.LargestWindowHeight);
                winW = Math.Min(winW, System.Console.BufferWidth);
                winH = Math.Min(winH, System.Console.BufferHeight);
                System.Console.SetWindowSize(Math.Max(1, winW), Math.Max(1, winH));
            }
        }
        catch (IOException)
        {
            // Konsola IDE może nie pozwalać na zmianę rozmiaru.
        }
        catch (ArgumentOutOfRangeException)
        {
        }
        catch (PlatformNotSupportedException)
        {
        }
    }

    private static void ClearLine()
    {
        try
        {
            var max = Math.Max(1, System.Console.BufferWidth - System.Console.CursorLeft - 1);
            var width = Math.Min(Math.Max(1, System.Console.WindowWidth - 1), max);
            var left = System.Console.CursorLeft;
            var top = System.Console.CursorTop;
            System.Console.Write(new string(' ', width));
            if (top >= 0 && top < System.Console.BufferHeight && left >= 0 && left < System.Console.BufferWidth)
                System.Console.SetCursorPosition(left, top);
        }
        catch (IOException)
        {
        }
        catch (ArgumentOutOfRangeException)
        {
        }
    }

    private static void WriteClamped(string text)
    {
        try
        {
            var space = System.Console.BufferWidth - System.Console.CursorLeft;
            if (space <= 0)
                return;
            if (text.Length > space)
                text = text[..space];
            System.Console.Write(text);
        }
        catch (IOException)
        {
        }
        catch (ArgumentOutOfRangeException)
        {
        }
    }

    private static bool TrySetCursor(int left, int top)
    {
        try
        {
            if (left < 0 || top < 0)
                return false;
            if (left >= System.Console.BufferWidth || top >= System.Console.BufferHeight)
                return false;

            System.Console.SetCursorPosition(left, top);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
}
