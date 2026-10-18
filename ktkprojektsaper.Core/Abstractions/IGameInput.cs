namespace ktkprojektsaper.Core.Abstractions;

/// <summary>
/// Wejście użytkownika niezależne od konsoli / GUI.
/// </summary>
public interface IGameInput
{
    /// <summary>
    /// Odczyt akcji. Gdy podano timeout i nie ma wejścia, zwraca <see cref="GameInputAction.None"/>.
    /// </summary>
    GameInputAction ReadGameAction(TimeSpan? timeout = null);

    /// <summary>
    /// Usuwa zalegające klawisze (np. Enter z menu albo sekwencje po Clear).
    /// </summary>
    void Flush();
}
