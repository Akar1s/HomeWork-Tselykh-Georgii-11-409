using MiniHttpServer.Core.Attributes;
using MiniHttpServer.Services;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class ForgetEndpoint
    {
        private readonly EmailService _emailService = new();

        [HttpGet]
        public string ShowForgetPage()
        {
            Console.WriteLine("ForgetEndpoint: Serving forget.html");
            return "Public/forget.html";
        }

        [HttpPost]
        public string HandleForgetForm(string email)
        {
            Console.WriteLine($"ForgetEndpoint: Обработка восстановления пароля для: {email}");

            if (string.IsNullOrWhiteSpace(email))
                return GetErrorPage("Email не может быть пустым");

            try
            {
                _emailService.SendPasswordReset(email);
                return GetSuccessPage();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в ForgetEndpoint: {ex.Message}");
                return GetErrorPage("Ошибка при отправке письма");
            }
        }

        private static string GetSuccessPage() => @"
<!DOCTYPE html>
<html>
<head>
    <title>Password Reset Sent</title>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1, shrink-to-fit=no'>
    <link rel='stylesheet' type='text/css' href='style.css'>
    <link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css'>
</head>
<body>
    <div class='container-fluid'>
        <div class='row'>
            <div class='col-lg-6 col-md-6 d-none d-md-block image-container'></div>
            <div class='col-lg-6 col-md-6 form-container'>
                <div class='col-lg-8 col-md-12 col-sm-9 col-xs-12 form-box text-center'>
                    <div class='logo mb-3'>
                        <img src='image/logo.png' width='150px'>
                    </div>
                    <div class='mb-4'>
                        <h4 class='mb-3'>✅ Письмо отправлено!</h4>
                        <p class='text-white'>Инструкции по восстановлению пароля отправлены на ваш email.</p>
                    </div>
                    <div class='mb-3'>
                        <a href='login.html' class='btn'>Вернуться к авторизации</a>
                    </div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>";

        private static string GetErrorPage(string error) => $@"
<!DOCTYPE html>
<html>
<head>
    <title>Error</title>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1, shrink-to-fit=no'>
    <link rel='stylesheet' type='text/css' href='style.css'>
    <link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css'>
</head>
<body>
    <div class='container-fluid'>
        <div class='row'>
            <div class='col-lg-6 col-md-6 d-none d-md-block image-container'></div>
            <div class='col-lg-6 col-md-6 form-container'>
                <div class='col-lg-8 col-md-12 col-sm-9 col-xs-12 form-box text-center'>
                    <div class='logo mb-3'>
                        <img src='image/logo.png' width='150px'>
                    </div>
                    <div class='mb-4'>
                        <h4 class='mb-3 text-danger'>Ошибка</h4>
                        <p class='text-white'>{error}</p>
                    </div>
                    <div class='mb-3'>
                        <a href='forget.html' class='btn'>Попробовать снова</a>
                    </div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>";
    }
}
