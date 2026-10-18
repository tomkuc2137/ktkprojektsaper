using System.Collections.Concurrent;
using ktkprojektsaper.Core.Abstractions;

namespace ktkprojektsaper.Graphic.Input;

public sealed class WpfGameInput : IGameInput
{
    private readonly ConcurrentQueue<(GameInputAction Action, int? X, int? Y)> _queue = new();
    private readonly object _targetLock = new();
    private int? _targetX;
    private int? _targetY;

    public void Enqueue(GameInputAction action, int? x = null, int? y = null)
    {
        if ((action is GameInputAction.Reveal or GameInputAction.ToggleFlag) &&
            x is not null && y is not null)
        {
            lock (_targetLock)
            {
                _targetX = x;
                _targetY = y;
            }
        }

        _queue.Enqueue((action, x, y));
    }

    public void Flush()
    {
        while (_queue.TryDequeue(out _))
        {
        }

        lock (_targetLock)
        {
            _targetX = null;
            _targetY = null;
        }
    }

    public bool TryConsumeCellTarget(out int x, out int y)
    {
        lock (_targetLock)
        {
            if (_targetX is null || _targetY is null)
            {
                x = 0;
                y = 0;
                return false;
            }

            x = _targetX.Value;
            y = _targetY.Value;
            _targetX = null;
            _targetY = null;
            return true;
        }
    }

    public GameInputAction ReadGameAction(TimeSpan? timeout = null)
    {
        var deadline = timeout is null
            ? DateTime.MaxValue
            : DateTime.UtcNow + timeout.Value;

        while (DateTime.UtcNow < deadline)
        {
            if (_queue.TryDequeue(out var item))
                return item.Action;

            Thread.Sleep(20);
        }

        return GameInputAction.None;
    }
}
