using ktkprojektsaper.Core.Abstractions;
using ktkprojektsaper.Core.Models;
using ktkprojektsaper.Core.Services;

namespace ktkprojektsaper.Core.Controllers;

/// <summary>
/// Główny kontroler aplikacji (menu / nawigacja między ekranami).
/// </summary>
public sealed class AppController
{
    private readonly IMenuView _menuView;
    private readonly IGameView _gameView;
    private readonly IGameInput _gameInput;
    private readonly IScoreStore _scoreStore;

    public AppController(
        IMenuView menuView,
        IGameView gameView,
        IGameInput gameInput,
        IScoreStore scoreStore)
    {
        _menuView = menuView;
        _gameView = gameView;
        _gameInput = gameInput;
        _scoreStore = scoreStore;
    }

    public void Run()
    {
        while (true)
        {
            _gameInput.Flush();
            var choice = _menuView.ShowMenu("SAPER",
            [
                "Nowa gra",
                "Tabela wyników",
                "Sterowanie",
                "Wyjście"
            ]);

            switch (choice)
            {
                case 0:
                    StartNewGame();
                    break;
                case 1:
                    _menuView.ShowScores(_scoreStore.Load());
                    break;
                case 2:
                    _menuView.ShowInfo("Sterowanie",
                        "Strzałki  – ruch kursora\n" +
                        "Enter/Spacja – odkryj pole\n" +
                        "F        – flaga\n" +
                        "R        – nowa gra (ten sam poziom)\n" +
                        "Esc      – powrót do menu");
                    break;
                case 3:
                    return;
                case -1:
                    // Esc w menu głównym – pytamy ponownie zamiast od razu zamykać
                    // (unikamy zamknięcia przez śmieci w buforze po Clear).
                    continue;
            }
        }
    }

    private void StartNewGame()
    {
        var options = Difficulty.Presets.Select(d => $"{d.Name} ({d.Width}x{d.Height}, {d.MineCount} min)").ToList();
        options.Add("Własny poziom");
        options.Add("Wstecz");

        var choice = _menuView.ShowMenu("Poziom trudności", options);
        if (choice < 0 || choice == options.Count - 1)
            return;

        Difficulty difficulty;
        if (choice == options.Count - 2)
        {
            var custom = _menuView.ShowCustomDifficulty();
            if (custom is null)
                return;
            difficulty = custom;
        }
        else
        {
            difficulty = Difficulty.Presets[choice];
        }

        var game = new GameController(difficulty, _gameView, _gameInput, _scoreStore);
        game.Run();
    }
}
