using MiniHttpServer.Core.Abstracts;
using MiniHttpServer.Shared;
using System.Net;
using System.Text;

namespace MiniHttpServer.Core.Handlers
{
    internal class StaticFilesHandler : Handler
    {
        public override async void HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var isGetMethod = request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase);
            var absolutePath = request.Url.AbsolutePath;

            
            if (isGetMethod && (absolutePath == "/" || absolutePath == ""))
            {
                var response = context.Response;
                byte[]? buffer = GetResponseBytes.Invoke("index.html");

                response.ContentType = "text/html; charset=utf-8";

                if (buffer == null)
                {
                    response.StatusCode = 404;
                    buffer = Encoding.UTF8.GetBytes("<html><body>404 - Not Found</body></html>");
                }

                response.ContentLength64 = buffer.Length;
                using Stream output = response.OutputStream;
                await output.WriteAsync(buffer, 0, buffer.Length);
                await output.FlushAsync();

                Console.WriteLine($"Запрос обработан: / -> index.html");
                return;
            }

            var isStaticFile = absolutePath.Split('/').Any(x => x.Contains("."));


            if (isGetMethod && isStaticFile)
            {
                var response = context.Response;

                byte[]? buffer = null;

                string path = request.Url.AbsolutePath.Trim('/');

                /*
                   if (path == null || path == "/")
                    buffer = GetResponseBytes.Invoke($"Public/index.html");
                */
                buffer = GetResponseBytes.Invoke(path);

                response.ContentType = MiniHttpServer.Shared.ContentType.GetContentType(path.Trim('/'));

                if (buffer == null)
                {
                    response.StatusCode = 404;
                    string errorText = "<html><body>404 - Not Found</html></body>";
                    buffer = Encoding.UTF8.GetBytes(errorText);
                }

                response.ContentLength64 = buffer.Length;

                using Stream output = response.OutputStream;
                await output.WriteAsync(buffer, 0, buffer.Length);
                await output.FlushAsync();

                if (response.StatusCode == 200)
                    Console.WriteLine($"Запрос обработан: {request.Url.AbsolutePath} - Status: {response.StatusCode}");
                else
                    Console.WriteLine($"Ошибка запроса: {request.Url.AbsolutePath} - Status: {response.StatusCode}");

            }
            // передача запроса дальше по цепи при наличии в ней обработчиков
            else if (Successor != null)
            {
                Successor.HandleRequest(context);
            }
        }
    }
}
