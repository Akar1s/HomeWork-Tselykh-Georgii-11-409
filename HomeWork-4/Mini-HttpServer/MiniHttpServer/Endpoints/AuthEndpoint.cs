using MiniHttpServer.Core.Attributes;
using MiniHttpServer.Services;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class AuthEndpoint
    {
        private readonly EmailService _emailService = new();

        [HttpGet]
        public string LoginPage()
        {
            Console.WriteLine(" AuthEndpoint: Serving login.html");
            return "Public/login.html";
        }

        [HttpPost]
        public string Login(string email, string password)
        {
            Console.WriteLine($" AuthEndpoint: Обработка логина для {email}");
            _emailService.SendAuthNotification(email, password);
            return "<html><body><h2>Авторизация успешна!</h2><p>Письмо с уведомлением отправлено администратору.</p></body></html>";
        }
    }
}
