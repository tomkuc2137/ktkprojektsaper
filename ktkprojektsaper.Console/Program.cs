using ktkprojektsaper.Console.Input;
using ktkprojektsaper.Console.Views;
using ktkprojektsaper.Core.Controllers;
using ktkprojektsaper.Core.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.CursorVisible = false;
Console.Title = "Saper";

if (OperatingSystem.IsWindows())
{
    try
    {
        // Rider / małe okno: bez większego bufora plansza wychodzi poza zakres kursora.
        if (Console.BufferWidth < 100 || Console.BufferHeight < 40)
            Console.SetBufferSize(Math.Max(Console.BufferWidth, 100), Math.Max(Console.BufferHeight, 40));
    }
    catch
    {
        // Nie każda konsola pozwala na zmianę rozmiaru.
    }
}

try
{
    var scoreStore = new FileScoreStore();
    var menuView = new ConsoleMenuView();
    var gameView = new ConsoleGameView();
    var input = new ConsoleGameInput();

    var app = new AppController(menuView, gameView, input, scoreStore);
    app.Run();
}
catch (Exception ex)
{
    Console.ResetColor();
    Console.CursorVisible = true;
    Console.WriteLine();
    Console.WriteLine("Błąd: " + ex.Message);
    Console.WriteLine(ex.StackTrace);
    Console.WriteLine();
    Console.WriteLine("Naciśnij Enter, aby zamknąć...");
    try { Console.ReadLine(); } catch { /* ignore */ }
}
finally
{
    Console.ResetColor();
    Console.CursorVisible = true;
}
