using ktkprojektsaper.Core.Abstractions;

namespace ktkprojektsaper.Console.Input;

public sealed class ConsoleGameInput : IGameInput
{
    public void Flush()
    {
        try
        {
            while (System.Console.KeyAvailable)
                System.Console.ReadKey(intercept: true);
        }
        catch (InvalidOperationException)
        {
            // Brak konsoli interaktywnej – nic do spłukania.
        }
    }

    public GameInputAction ReadGameAction(TimeSpan? timeout = null)
    {
        var deadline = timeout is null
            ? DateTime.MaxValue
            : DateTime.UtcNow + timeout.Value;

        while (DateTime.UtcNow < deadline)
        {
            bool available;
            try
            {
                available = System.Console.KeyAvailable;
            }
            catch (InvalidOperationException)
            {
                Thread.Sleep(40);
                continue;
            }

            if (!available)
            {
                Thread.Sleep(40);
                continue;
            }

            ConsoleKeyInfo key;
            try
            {
                key = System.Console.ReadKey(intercept: true);
            }
            catch (InvalidOperationException)
            {
                return GameInputAction.None;
            }

            return Map(key);
        }

        return GameInputAction.None;
    }

    private static GameInputAction Map(ConsoleKeyInfo key) => key.Key switch
    {
        ConsoleKey.UpArrow or ConsoleKey.W => GameInputAction.MoveUp,
        ConsoleKey.DownArrow or ConsoleKey.S => GameInputAction.MoveDown,
        ConsoleKey.LeftArrow or ConsoleKey.A => GameInputAction.MoveLeft,
        ConsoleKey.RightArrow or ConsoleKey.D => GameInputAction.MoveRight,
        ConsoleKey.Enter or ConsoleKey.Spacebar => GameInputAction.Reveal,
        ConsoleKey.F => GameInputAction.ToggleFlag,
        ConsoleKey.R => GameInputAction.Restart,
        ConsoleKey.Escape => GameInputAction.QuitToMenu,
        _ => GameInputAction.None
    };
}
