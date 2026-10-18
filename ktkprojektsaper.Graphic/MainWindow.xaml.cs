using System.Windows;
using System.Windows.Input;
using ktkprojektsaper.Core.Controllers;
using ktkprojektsaper.Core.Services;
using ktkprojektsaper.Graphic.Input;
using ktkprojektsaper.Graphic.Views;

namespace ktkprojektsaper.Graphic;

public partial class MainWindow : Window
{
    private Action<Key>? _gameKeyHandler;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        PreviewKeyDown += OnPreviewKeyDown;
    }

    public void SetContent(UIElement content)
    {
        BootLabel.Visibility = Visibility.Collapsed;
        ContentHost.Child = content;
    }

    public void AttachGameKeys(Action<Key> handler) => _gameKeyHandler = handler;

    public void DetachGameKeys() => _gameKeyHandler = null;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var input = new WpfGameInput();
        var menuView = new WpfMenuView(this);
        var gameView = new WpfGameView(this, input);
        var scores = new FileScoreStore();
        var app = new AppController(menuView, gameView, input, scores);

        Task.Factory.StartNew(() =>
        {
            try
            {
                app.Run();
            }
            finally
            {
                Dispatcher.BeginInvoke(() => Close());
            }
        }, TaskCreationOptions.LongRunning);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_gameKeyHandler is null)
            return;

        _gameKeyHandler(e.Key);
        if (e.Key is Key.Up or Key.Down or Key.Left or Key.Right or Key.Space)
            e.Handled = true;
    }
}
