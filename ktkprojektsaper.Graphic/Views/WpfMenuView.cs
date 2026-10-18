using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ktkprojektsaper.Core.Abstractions;
using ktkprojektsaper.Core.Models;

namespace ktkprojektsaper.Graphic.Views;

public sealed class WpfMenuView : IMenuView
{
    private readonly MainWindow _window;

    public WpfMenuView(MainWindow window)
    {
        _window = window;
    }

    public int ShowMenu(string title, IReadOnlyList<string> options, int selectedIndex = 0)
    {
        var result = -1;
        using var done = new ManualResetEventSlim(false);
        var start = Math.Clamp(selectedIndex, 0, Math.Max(0, options.Count - 1));

        _window.Dispatcher.BeginInvoke(() =>
        {
            var panel = BuildMenu(title, options, start, index =>
            {
                result = index;
                done.Set();
            });
            _window.SetContent(panel);
        }, DispatcherPriority.Normal);

        done.Wait();
        return result;
    }

    public Difficulty? ShowCustomDifficulty()
    {
        Difficulty? result = null;
        using var done = new ManualResetEventSlim(false);

        _window.Dispatcher.BeginInvoke(() =>
        {
            var dialog = new Window
            {
                Title = "Własny poziom",
                Width = 360,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = _window,
                ResizeMode = ResizeMode.NoResize,
                Background = (Brush)_window.FindResource("AppBg")
            };

            var widthBox = new TextBox { Text = "12", Margin = new Thickness(0, 0, 0, 8) };
            var heightBox = new TextBox { Text = "12", Margin = new Thickness(0, 0, 0, 8) };
            var minesBox = new TextBox { Text = "20", Margin = new Thickness(0, 0, 0, 12) };

            var ok = new Button { Content = "OK", Style = (Style)_window.FindResource("MenuButton"), Width = 100 };
            var cancel = new Button
            {
                Content = "Anuluj",
                Style = (Style)_window.FindResource("MenuButton"),
                Width = 100,
                Margin = new Thickness(8, 0, 0, 0)
            };

            ok.Click += (_, _) =>
            {
                if (!int.TryParse(widthBox.Text, out var w) ||
                    !int.TryParse(heightBox.Text, out var h) ||
                    !int.TryParse(minesBox.Text, out var m))
                {
                    MessageBox.Show(dialog, "Podaj poprawne liczby całkowite.", "Błąd");
                    return;
                }

                try
                {
                    result = new Difficulty("Własny", w, h, m);
                    dialog.DialogResult = true;
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show(dialog, ex.Message, "Błąd");
                }
            };
            cancel.Click += (_, _) => dialog.DialogResult = false;

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            buttons.Children.Add(ok);
            buttons.Children.Add(cancel);

            var form = new StackPanel { Margin = new Thickness(20) };
            form.Children.Add(Label("Szerokość (5–40)"));
            form.Children.Add(widthBox);
            form.Children.Add(Label("Wysokość (5–20)"));
            form.Children.Add(heightBox);
            form.Children.Add(Label("Liczba min"));
            form.Children.Add(minesBox);
            form.Children.Add(buttons);
            dialog.Content = form;

            dialog.ShowDialog();
            done.Set();
        }, DispatcherPriority.Normal);

        done.Wait();
        return result;
    }

    public void ShowScores(IReadOnlyList<ScoreEntry> scores)
    {
        using var done = new ManualResetEventSlim(false);
        _window.Dispatcher.BeginInvoke(() =>
        {
            var text = scores.Count == 0
                ? "Brak zapisanych wyników."
                : string.Join('\n', scores.Select((s, i) =>
                    $"{i + 1}. {s.PlayerName} – {s.DifficultyName} – {s.Duration:mm\\:ss} ({s.PlayedAt:yyyy-MM-dd})"));

            MessageBox.Show(_window, text, "Tabela wyników", MessageBoxButton.OK, MessageBoxImage.Information);
            done.Set();
        }, DispatcherPriority.Normal);
        done.Wait();
    }

    public void ShowInfo(string title, string message)
    {
        using var done = new ManualResetEventSlim(false);
        _window.Dispatcher.BeginInvoke(() =>
        {
            MessageBox.Show(_window, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            done.Set();
        }, DispatcherPriority.Normal);
        done.Wait();
    }

    private FrameworkElement BuildMenu(
        string title,
        IReadOnlyList<string> options,
        int selected,
        Action<int> onComplete)
    {
        var root = new DockPanel { LastChildFill = true, Margin = new Thickness(24) };

        var header = new TextBlock
        {
            Text = title,
            FontSize = 36,
            FontWeight = FontWeights.Bold,
            Foreground = (Brush)_window.FindResource("Accent"),
            Margin = new Thickness(0, 12, 0, 8),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var hint = new TextBlock
        {
            Text = "Strzałki / Enter – wybór   ·   Esc – wstecz   ·   kliknięcie myszą",
            Foreground = (Brush)_window.FindResource("TextMuted"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };
        DockPanel.SetDock(hint, Dock.Top);
        root.Children.Add(hint);

        var list = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Width = 420
        };

        var buttons = new List<Button>();
        for (var i = 0; i < options.Count; i++)
        {
            var index = i;
            var btn = new Button
            {
                Content = options[i],
                Style = (Style)_window.FindResource("MenuButton"),
                Tag = index
            };
            btn.Click += (_, _) => onComplete(index);
            buttons.Add(btn);
            list.Children.Add(btn);
        }

        void Highlight(int index)
        {
            selected = (index + options.Count) % options.Count;
            for (var i = 0; i < buttons.Count; i++)
            {
                buttons[i].Background = i == selected
                    ? new SolidColorBrush(Color.FromRgb(0xC4, 0xA3, 0x5A))
                    : new SolidColorBrush(Color.FromRgb(0x2F, 0x46, 0x36));
                buttons[i].Foreground = i == selected
                    ? new SolidColorBrush(Color.FromRgb(0x1B, 0x2A, 0x1F))
                    : new SolidColorBrush(Color.FromRgb(0xF2, 0xF0, 0xE6));
            }
        }

        Highlight(selected);
        root.Children.Add(list);
        root.Focusable = true;
        root.Loaded += (_, _) => Keyboard.Focus(root);
        root.PreviewKeyDown += (_, e) =>
        {
            if (e.Key is Key.Up or Key.W)
            {
                Highlight(selected - 1);
                e.Handled = true;
            }
            else if (e.Key is Key.Down or Key.S)
            {
                Highlight(selected + 1);
                e.Handled = true;
            }
            else if (e.Key is Key.Enter or Key.Space)
            {
                onComplete(selected);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                onComplete(-1);
                e.Handled = true;
            }
        };

        return root;
    }

    private static TextBlock Label(string text) => new()
    {
        Text = text,
        Foreground = Brushes.WhiteSmoke,
        Margin = new Thickness(0, 0, 0, 4)
    };
}
