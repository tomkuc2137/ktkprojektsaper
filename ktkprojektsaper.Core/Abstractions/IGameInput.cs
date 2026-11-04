namespace ktkprojektsaper.Core.Abstractions;

public interface IGameInput
{
    GameInputAction ReadGameAction(TimeSpan? timeout = null);
    void Flush();
    bool TryConsumeCellTarget(out int x, out int y);
}
