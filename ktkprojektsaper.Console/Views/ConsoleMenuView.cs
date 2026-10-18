using ktkprojektsaper.Core.Abstractions;
using ktkprojektsaper.Core.Models;

namespace ktkprojektsaper.Console.Views;

public sealed class ConsoleMenuView : IMenuView
{
    public int ShowMenu(string title, IReadOnlyList<string> options, int selectedIndex = 0)
    {
        if (options.Count == 0)
            return -1;

        selectedIndex = Math.Clamp(selectedIndex, 0, options.Count - 1);
        Draw(title, options, selectedIndex);
        FlushInput();

        while (true)
        {
            var key = System.Console.ReadKey(intercept: true);
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                    selectedIndex = (selectedIndex - 1 + options.Count) % options.Count;
                    Draw(title, options, selectedIndex);
                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                    selectedIndex = (selectedIndex + 1) % options.Count;
                    Draw(title, options, selectedIndex);
                    break;
                case ConsoleKey.Enter:
                case ConsoleKey.Spacebar:
                    return selectedIndex;
                case ConsoleKey.Escape:
                    return -1;
            }
        }
    }

    public Difficulty? ShowCustomDifficulty()
    {
        System.Console.Clear();
        System.Console.CursorVisible = true;

        try
        {
            System.Console.WriteLine("Własny poziom (Esc anuluje)\n");

            if (!TryReadInt("Szerokość (5-40): ", 5, 40, out var width))
                return null;
            if (!TryReadInt("Wysokość (5-20): ", 5, 20, out var height))
                return null;

            var maxMines = width * height - 9;
            if (!TryReadInt($"Liczba min (1-{maxMines}): ", 1, maxMines, out var mines))
                return null;

            return new Difficulty("Własny", width, height, mines);
        }
        catch (ArgumentException ex)
        {
            ShowInfo("Błąd", ex.Message);
            return null;
        }
        finally
        {
            System.Console.CursorVisible = false;
        }
    }

    public void ShowScores(IReadOnlyList<ScoreEntry> scores)
    {
        System.Console.Clear();
        System.Console.WriteLine("=== Tabela wyników ===\n");

        if (scores.Count == 0)
        {
            System.Console.WriteLine("Brak zapisanych wyników.");
        }
        else
        {
            System.Console.WriteLine($"{"#",-3} {"Gracz",-16} {"Poziom",-12} {"Czas",-8} Data");
            System.Console.WriteLine(new string('-', 60));
            for (var i = 0; i < scores.Count; i++)
            {
                var s = scores[i];
                System.Console.WriteLine(
                    $"{i + 1,-3} {Truncate(s.PlayerName, 16),-16} {Truncate(s.DifficultyName, 12),-12} " +
                    $"{s.Duration:mm\\:ss}  {s.PlayedAt:yyyy-MM-dd HH:mm}");
            }
        }

        System.Console.WriteLine("\nNaciśnij dowolny klawisz...");
        System.Console.ReadKey(intercept: true);
    }

    public void ShowInfo(string title, string message)
    {
        System.Console.Clear();
        System.Console.WriteLine($"=== {title} ===\n");
        System.Console.WriteLine(message);
        System.Console.WriteLine("\nNaciśnij dowolny klawisz...");
        System.Console.ReadKey(intercept: true);
    }

    private static void Draw(string title, IReadOnlyList<string> options, int selectedIndex)
    {
        System.Console.CursorVisible = false;
        System.Console.Clear();
        System.Console.WriteLine($"=== {title} ===\n");
        System.Console.WriteLine("Strzałki / Enter – wybór, Esc – wstecz\n");

        for (var i = 0; i < options.Count; i++)
        {
            if (i == selectedIndex)
            {
                System.Console.ForegroundColor = ConsoleColor.Black;
                System.Console.BackgroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine($" > {options[i]} ");
                System.Console.ResetColor();
            }
            else
            {
                System.Console.WriteLine($"   {options[i]}");
            }
        }
    }

    private static bool TryReadInt(string prompt, int min, int max, out int value)
    {
        value = 0;
        while (true)
        {
            System.Console.Write(prompt);
            var line = ReadLineOrCancel();
            if (line is null)
                return false;

            if (int.TryParse(line, out value) && value >= min && value <= max)
                return true;

            System.Console.WriteLine($"Podaj liczbę z zakresu {min}-{max}.");
        }
    }

    private static string? ReadLineOrCancel()
    {
        var buffer = new System.Text.StringBuilder();
        while (true)
        {
            var key = System.Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Escape)
                return null;
            if (key.Key == ConsoleKey.Enter)
            {
                System.Console.WriteLine();
                return buffer.ToString();
            }
            if (key.Key == ConsoleKey.Backspace && buffer.Length > 0)
            {
                buffer.Length--;
                System.Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                buffer.Append(key.KeyChar);
                System.Console.Write(key.KeyChar);
            }
        }
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..(max - 1)] + "…";

    private static void FlushInput()
    {
        try
        {
            while (System.Console.KeyAvailable)
                System.Console.ReadKey(intercept: true);
        }
        catch (InvalidOperationException)
        {
        }
    }
}
