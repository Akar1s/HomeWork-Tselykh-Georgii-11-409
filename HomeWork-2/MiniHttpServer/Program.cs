using MiniHttpServer.shared;
using System.Text.Json;

var settingsJson = string.Empty;
var cts = new CancellationTokenSource();

// Чтение настроек
try
{
    settingsJson = File.ReadAllText("settings.json");
}
catch (FileNotFoundException)
{
    Console.WriteLine("Файл settings.json не существует");
    Environment.Exit(1);
}

SettingsModel settings = null;

try
{
    settings = JsonSerializer.Deserialize<SettingsModel>(settingsJson);
}
catch (Exception ex)
{
    Console.WriteLine("Файл settings.json некорректен: " + ex.Message);
    Environment.Exit(1);
}

// Проверка существования директории
if (!Directory.Exists(settings.PublicDirectoryPath))
{
    Console.WriteLine($"Публичная директория не существует: {settings.PublicDirectoryPath}");
    Environment.Exit(1);
}

var httpServer = new HttpServer(settings);
httpServer.Start();

// Задача для обработки консольных команд
var consoleTask = Task.Run(() =>
{
    while (!cts.Token.IsCancellationRequested)
    {
        var input = Console.ReadLine();
        if (input?.Trim().ToLower() == "/stop")
        {
            Console.WriteLine("Остановка сервера...");
            cts.Cancel();
            break;
        }
        else if (!string.IsNullOrEmpty(input))
        {
            Console.WriteLine("Неизвестная команда. Используйте /stop для остановки сервера.");
        }
    }
});

try
{
    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (TaskCanceledException)
{

}


httpServer.Stop();