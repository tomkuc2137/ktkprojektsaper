using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ktkprojektsaper.Core.Abstractions;
using ktkprojektsaper.Core.Models;
using ktkprojektsaper.Graphic.Controls;
using ktkprojektsaper.Graphic.Input;

namespace ktkprojektsaper.Graphic.Views;

public sealed class WpfGameView : IGameView
{
    private readonly MainWindow _window;
    private readonly WpfGameInput _input;

    private readonly TextBlock _hudDifficulty = new();
    private readonly TextBlock _hudMines = new();
    private readonly TextBlock _hudTime = new();
    private readonly TextBlock _hudStatus = new();
    private readonly TextBlock _message = new();
    private readonly UniformGrid _boardGrid = new();
    private readonly ScrollViewer _boardScroll = new();

    private CellControl[,]? _cells;
    private DockPanel? _root;
    private int _width;
    private int _height;

    public WpfGameView(MainWindow window, WpfGameInput input)
    {
        _window = window;
        _input = input;
    }

    public void ShowGame(GameHudData hud, IReadOnlyList<CellViewData> allCells)
    {
        using var done = new ManualResetEventSlim(false);
        _window.Dispatcher.BeginInvoke(() =>
        {
            _width = hud.Width;
            _height = hud.Height;
            BuildLayout(hud);
            _cells = new CellControl[hud.Width, hud.Height];

            var cellSize = ResolveCellSize(hud.Width, hud.Height);
            _boardGrid.Children.Clear();
            _boardGrid.Columns = hud.Width;
            _boardGrid.Rows = hud.Height;

            foreach (var data in allCells.OrderBy(c => c.Y).ThenBy(c => c.X))
            {
                var cell = new CellControl(data.X, data.Y, cellSize);
                cell.Apply(data, animate: false);
                cell.CellClicked += OnCellClicked;
                _cells[data.X, data.Y] = cell;
                _boardGrid.Children.Add(cell);
            }

            ApplyHud(hud);
            _message.Text = "LPM / Enter – odkryj   ·   PPM / F – flaga   ·   R – restart   ·   Esc – menu";
            _window.SetContent(_root!);
            _window.AttachGameKeys(OnGameKey);
            Keyboard.Focus(_root!);
            done.Set();
        }, DispatcherPriority.Normal);
        done.Wait();
    }

    public void UpdateCells(IReadOnlyList<CellViewData> changedCells)
    {
        _window.Dispatcher.BeginInvoke(() =>
        {
            if (_cells is null)
                return;

            foreach (var data in changedCells)
            {
                if (data.X < 0 || data.Y < 0 || data.X >= _width || data.Y >= _height)
                    continue;
                _cells[data.X, data.Y].Apply(data, animate: true);
            }
        }, DispatcherPriority.Render);
    }

    public void UpdateCursor(CellViewData previous, CellViewData current)
    {
        _window.Dispatcher.BeginInvoke(() =>
        {
            if (_cells is null)
                return;

            _cells[previous.X, previous.Y].Apply(previous, animate: false);
            _cells[current.X, current.Y].Apply(current, animate: false);
        }, DispatcherPriority.Render);
    }

    public void UpdateHud(GameHudData hud)
    {
        _window.Dispatcher.BeginInvoke(() => ApplyHud(hud), DispatcherPriority.Background);
    }

    public void ShowMessage(string message)
    {
        _window.Dispatcher.BeginInvoke(() => { _message.Text = message; }, DispatcherPriority.Normal);
    }

    public string? AskPlayerName()
    {
        string? name = null;
        using var done = new ManualResetEventSlim(false);

        _window.Dispatcher.BeginInvoke(() =>
        {
            var dialog = new Window
            {
                Title = "Wygrana!",
                Width = 360,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = _window,
                ResizeMode = ResizeMode.NoResize,
                Background = (Brush)_window.FindResource("AppBg")
            };

            var box = new TextBox { Margin = new Thickness(0, 8, 0, 12) };
            var ok = new Button { Content = "Zapisz", Style = (Style)_window.FindResource("MenuButton"), Width = 100 };
            var skip = new Button
            {
                Content = "Pomiń",
                Style = (Style)_window.FindResource("MenuButton"),
                Width = 100,
                Margin = new Thickness(8, 0, 0, 0)
            };

            ok.Click += (_, _) =>
            {
                name = box.Text;
                dialog.DialogResult = true;
            };
            skip.Click += (_, _) =>
            {
                name = null;
                dialog.DialogResult = false;
            };

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            buttons.Children.Add(ok);
            buttons.Children.Add(skip);

            var panel = new StackPanel { Margin = new Thickness(20) };
            panel.Children.Add(new TextBlock
            {
                Text = "Podaj imię do tabeli wyników:",
                Foreground = Brushes.WhiteSmoke
            });
            panel.Children.Add(box);
            panel.Children.Add(buttons);
            dialog.Content = panel;
            dialog.ShowDialog();
            done.Set();
        }, DispatcherPriority.Normal);

        done.Wait();
        return string.IsNullOrWhiteSpace(name) ? null : name.Trim();
    }

    private void BuildLayout(GameHudData hud)
    {
        _root = new DockPanel { LastChildFill = true, Background = (Brush)_window.FindResource("AppBg") };
        _root.Focusable = true;

        var top = new Border
        {
            Background = (Brush)_window.FindResource("PanelBg"),
            Padding = new Thickness(16, 10, 16, 10),
            Margin = new Thickness(0, 0, 0, 8)
        };
        DockPanel.SetDock(top, Dock.Top);

        StyleHudText(_hudDifficulty);
        StyleHudText(_hudMines);
        StyleHudText(_hudTime);
        StyleHudText(_hudStatus);
        _hudStatus.Foreground = (Brush)_window.FindResource("Accent");

        var hudRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
        hudRow.Children.Add(_hudDifficulty);
        hudRow.Children.Add(HudSep());
        hudRow.Children.Add(_hudMines);
        hudRow.Children.Add(HudSep());
        hudRow.Children.Add(_hudTime);
        hudRow.Children.Add(HudSep());
        hudRow.Children.Add(_hudStatus);
        top.Child = hudRow;
        _root.Children.Add(top);

        _message.Foreground = (Brush)_window.FindResource("TextMuted");
        _message.FontSize = 13;
        _message.Margin = new Thickness(16, 8, 16, 12);
        _message.TextWrapping = TextWrapping.Wrap;
        _message.HorizontalAlignment = HorizontalAlignment.Center;
        DockPanel.SetDock(_message, Dock.Bottom);
        _root.Children.Add(_message);

        _boardScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        _boardScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        _boardScroll.Content = new Border
        {
            Background = (Brush)_window.FindResource("PanelBg"),
            Padding = new Thickness(10),
            CornerRadius = new CornerRadius(8),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Child = _boardGrid
        };
        _root.Children.Add(_boardScroll);

        ApplyHud(hud);
    }

    private void ApplyHud(GameHudData hud)
    {
        _hudDifficulty.Text = hud.DifficultyName;
        _hudMines.Text = $"Miny: {hud.RemainingMines}";
        _hudTime.Text = $"Czas: {hud.Elapsed:mm\\:ss}";
        _hudStatus.Text = hud.Status switch
        {
            GameStatus.Ready => "Gotowy",
            GameStatus.Playing => "Gra",
            GameStatus.Won => "WYGRANA",
            GameStatus.Lost => "PRZEGRANA",
            _ => hud.Status.ToString()
        };
    }

    private void OnCellClicked(int x, int y, MouseButton button)
    {
        if (button == MouseButton.Left)
            _input.Enqueue(GameInputAction.Reveal, x, y);
        else if (button == MouseButton.Right)
            _input.Enqueue(GameInputAction.ToggleFlag, x, y);
    }

    private void OnGameKey(Key key)
    {
        switch (key)
        {
            case Key.Up or Key.W:
                _input.Enqueue(GameInputAction.MoveUp);
                break;
            case Key.Down or Key.S:
                _input.Enqueue(GameInputAction.MoveDown);
                break;
            case Key.Left or Key.A:
                _input.Enqueue(GameInputAction.MoveLeft);
                break;
            case Key.Right or Key.D:
                _input.Enqueue(GameInputAction.MoveRight);
                break;
            case Key.Enter or Key.Space:
                _input.Enqueue(GameInputAction.Reveal);
                break;
            case Key.F:
                _input.Enqueue(GameInputAction.ToggleFlag);
                break;
            case Key.R:
                _input.Enqueue(GameInputAction.Restart);
                break;
            case Key.Escape:
                _window.DetachGameKeys();
                _input.Enqueue(GameInputAction.QuitToMenu);
                break;
        }
    }

    private static double ResolveCellSize(int width, int height)
    {
        if (width >= 28 || height >= 16)
            return 22;
        if (width >= 16)
            return 28;
        return 34;
    }

    private static void StyleHudText(TextBlock block)
    {
        block.FontSize = 16;
        block.FontWeight = FontWeights.SemiBold;
        block.Foreground = Brushes.WhiteSmoke;
        block.Margin = new Thickness(8, 0, 8, 0);
        block.VerticalAlignment = VerticalAlignment.Center;
    }

    private static TextBlock HudSep() => new()
    {
        Text = "·",
        Foreground = new SolidColorBrush(Color.FromRgb(0xA8, 0xB5, 0xA8)),
        VerticalAlignment = VerticalAlignment.Center
    };
}
