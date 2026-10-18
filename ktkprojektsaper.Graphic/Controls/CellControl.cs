using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ktkprojektsaper.Core.Abstractions;
using ktkprojektsaper.Core.Models;

namespace ktkprojektsaper.Graphic.Controls;

public sealed class CellControl : Border
{
    private static readonly Brush HiddenBrush = new SolidColorBrush(Color.FromRgb(0x3A, 0x56, 0x42));
    private static readonly Brush HiddenHoverBrush = new SolidColorBrush(Color.FromRgb(0x4A, 0x6A, 0x52));
    private static readonly Brush RevealedBrush = new SolidColorBrush(Color.FromRgb(0xD8, 0xD2, 0xC0));
    private static readonly Brush CursorBrush = new SolidColorBrush(Color.FromRgb(0xC4, 0xA3, 0x5A));
    private static readonly Brush MineBrush = new SolidColorBrush(Color.FromRgb(0x8B, 0x1E, 0x1E));
    private static readonly Brush FlagBrush = new SolidColorBrush(Color.FromRgb(0xB8, 0x6B, 0x2E));

    private readonly TextBlock _label;
    private CellState _lastState = (CellState)(-1);
    private bool _lastCursor;

    public int CellX { get; }
    public int CellY { get; }

    public event Action<int, int, MouseButton>? CellClicked;

    public CellControl(int x, int y, double size)
    {
        CellX = x;
        CellY = y;
        Width = size;
        Height = size;
        Margin = new Thickness(1);
        CornerRadius = new CornerRadius(3);
        BorderThickness = new Thickness(1);
        BorderBrush = new SolidColorBrush(Color.FromRgb(0x2A, 0x3C, 0x30));
        Cursor = Cursors.Hand;
        SnapsToDevicePixels = true;

        _label = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeights.Bold,
            FontSize = Math.Max(11, size * 0.42),
            FontFamily = new FontFamily("Segoe UI")
        };
        Child = _label;

        MouseLeftButtonUp += (_, _) => CellClicked?.Invoke(CellX, CellY, MouseButton.Left);
        MouseRightButtonUp += (_, e) =>
        {
            e.Handled = true;
            CellClicked?.Invoke(CellX, CellY, MouseButton.Right);
        };
        MouseEnter += (_, _) =>
        {
            if (_lastState == CellState.Hidden && !_lastCursor)
                Background = HiddenHoverBrush;
        };
        MouseLeave += (_, _) =>
        {
            if (_lastState == CellState.Hidden && !_lastCursor)
                Background = HiddenBrush;
        };
    }

    public void Apply(CellViewData data, bool animate)
    {
        var stateChanged = data.State != _lastState;
        _lastState = data.State;
        _lastCursor = data.IsCursor;

        BorderBrush = data.IsCursor
            ? CursorBrush
            : new SolidColorBrush(Color.FromRgb(0x2A, 0x3C, 0x30));
        BorderThickness = new Thickness(data.IsCursor ? 2.5 : 1);

        switch (data.State)
        {
            case CellState.Hidden:
                Background = data.IsCursor ? CursorBrush : HiddenBrush;
                _label.Text = string.Empty;
                break;
            case CellState.Flagged:
                Background = FlagBrush;
                _label.Text = "🚩";
                _label.Foreground = Brushes.White;
                break;
            case CellState.Revealed when data.IsMine:
                Background = MineBrush;
                _label.Text = "💣";
                _label.Foreground = Brushes.White;
                break;
            case CellState.Revealed:
                Background = RevealedBrush;
                if (data.AdjacentMines == 0)
                {
                    _label.Text = string.Empty;
                }
                else
                {
                    _label.Text = data.AdjacentMines.ToString();
                    _label.Foreground = NumberBrush(data.AdjacentMines);
                }
                break;
        }

        if (animate && stateChanged && data.State == CellState.Revealed)
            PlayRevealAnimation();
    }

    private void PlayRevealAnimation()
    {
        var anim = new DoubleAnimation(0.25, 1, TimeSpan.FromMilliseconds(180))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        BeginAnimation(OpacityProperty, anim);

        var scale = new ScaleTransform(0.82, 0.82);
        RenderTransform = scale;
        RenderTransformOrigin = new Point(0.5, 0.5);
        var sx = new DoubleAnimation(0.82, 1, TimeSpan.FromMilliseconds(180))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, sx);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, sx);
    }

    private static Brush NumberBrush(int n) => n switch
    {
        1 => new SolidColorBrush(Color.FromRgb(0x1E, 0x4D, 0xA1)),
        2 => new SolidColorBrush(Color.FromRgb(0x1B, 0x7A, 0x3A)),
        3 => new SolidColorBrush(Color.FromRgb(0xB0, 0x2A, 0x2A)),
        4 => new SolidColorBrush(Color.FromRgb(0x4B, 0x00, 0x82)),
        5 => new SolidColorBrush(Color.FromRgb(0x8B, 0x00, 0x00)),
        6 => new SolidColorBrush(Color.FromRgb(0x00, 0x80, 0x80)),
        _ => Brushes.Black
    };
}
