using System.Net;
using System.Text;

namespace MiniHttpServer.shared
{
    public class HttpServer
    {
        private HttpListener _listener = new();
        private SettingsModel _config;
        private CancellationTokenSource _cts = new();

        public HttpServer(SettingsModel config)
        {
            _config = config;
        }

        public void Start()
        {
            try
            {
                _listener.Prefixes.Add($"http://{_config.Domain}:{_config.Port}/");
                _listener.Start();
                Console.WriteLine($"Сервер запущен на http://{_config.Domain}:{_config.Port}/");

                // Запускаем обработку запросов в фоновой задаче
                _ = Task.Run(async () => await ProcessRequestsAsync());
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine($"Ошибка запуска сервера: порт {_config.Port} занят. {ex.Message}");
                Environment.Exit(1);
            }
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener.Stop();
            Console.WriteLine("Сервер остановил работу");
        }


        private async Task ProcessRequestsAsync()
        {
            var requests = new List<Task>();

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    // Ожидаем следующий запрос с поддержкой отмены
                    var context = await _listener.GetContextAsync()
                        .ConfigureAwait(false);

                    // Обрабатываем запрос в отдельной задаче
                    var requestTask = ProcessRequestAsync(context);
                    requests.Add(requestTask);

                    // Очищаем завершенные задачи
                    if (requests.Count > 10)
                    {
                        requests.RemoveAll(t => t.IsCompleted);
                    }
                }
                catch (HttpListenerException) when (_cts.Token.IsCancellationRequested)
                {
                    break;
                }
                catch (ObjectDisposedException) when (_cts.Token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении контекста: {ex.Message}");
                }
            }

            // Ожидаем завершения всех оставшихся запросов
            if (requests.Count > 0)
            {
                await Task.WhenAll(requests);
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            string responseText = string.Empty;
            int statusCode = 200;

            try
            {
                responseText = File.ReadAllText($"{_config.PublicDirectoryPath}/index.html");
            }
            catch (DirectoryNotFoundException dirEx)
            {
                Console.WriteLine("Директория не найдена: " + dirEx.Message);
                responseText = "<html><body>Directory not found</body></html>";
                statusCode = 404;
            }
            catch (FileNotFoundException filnfEx)
            {
                Console.WriteLine("Файл не найден: " + filnfEx.Message);
                responseText = "<html><body>File not found</body></html>";
                statusCode = 404;
            }
            catch (Exception ex)
            {
                Console.WriteLine("" + ex.Message);
                responseText = "<html><body>Internal server error</body></html>";
                statusCode = 500;
            }

            try
            {
                var response = context.Response;
                byte[] buffer = Encoding.UTF8.GetBytes(responseText);
                response.ContentLength64 = buffer.Length;
                response.ContentType = "text/html; charset=utf-8";
                response.StatusCode = statusCode;

                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, _cts.Token);
                await response.OutputStream.FlushAsync();

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {context.Request.HttpMethod} {context.Request.Url} -> {statusCode}");
            }
            catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested)
            {
                // Запрос был отменен при остановке сервера
                Console.WriteLine($"Запрос {context.Request.HttpMethod} {context.Request.Url} отменен из-за остановки сервера");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке ответа: {ex.Message}");
            }
            finally
            {
                context.Response.OutputStream?.Close();
            }
        }
    }
}