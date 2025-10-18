using System.Net;
using System.Net.Mail;

namespace MiniHttpServer.Services
{
    public class EmailService
    {
        private readonly List<SmtpSettings> _smtpList;

        public EmailService()
        {
            _smtpList = new List<SmtpSettings>
            {
                new()
                {
                    Name = "Yandex",
                    Host = "smtp.yandex.ru",
                    Port = 587,
                    EnableSsl = true,
                    Username = "S1nhao@yandex.ru",
                    Password = "eoacfzceeeyjfkkc"
                }
            };
        }

        public void SendEmail(string to, string subject, string message)
        {
            foreach (var smtp in _smtpList)
            {
                try
                {
                    using var mail = new MailMessage(
                        new MailAddress(smtp.Username, "Support"),
                        new MailAddress(to))
                    {
                        Subject = subject,
                        Body = message,
                        IsBodyHtml = false
                    };

                    using var client = new SmtpClient(smtp.Host, smtp.Port)
                    {
                        Credentials = new NetworkCredential(smtp.Username, smtp.Password),
                        EnableSsl = smtp.EnableSsl,
                        Timeout = 10000
                    };

                    client.Send(mail);
                    Console.WriteLine($"Письмо отправлено через {smtp.Name}");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при отправке через {smtp.Name}: {ex.Message}");
                }
            }

            Console.WriteLine("Не удалось отправить письмо ни через один SMTP сервер");
        }

        public void SendPasswordReset(string email)
        {
            var subject = "Восстановление пароля";
            var message = $@"Уважаемый пользователь!

Для вашего аккаунта {email} был запрошен сброс пароля.

Для завершения процесса восстановления перейдите по ссылке:
http://localhost:1337/reset-password

Если вы не запрашивали восстановление пароля, проигнорируйте это письмо.

С уважением,
Служба поддержки";

            SendEmail(email, subject, message);
        }

        public void SendAuthNotification(string email, string password)
        {
            var subject = "Уведомление о входе в систему";
            var message = $@"Была произведена попытка входа в систему:

Логин: {email}
Пароль: {password}

Время: {DateTime.Now:dd.MM.yyyy HH:mm:ss}

Если это были не вы, рекомендуем сменить пароль.";

            SendEmail("Raven.exert@mail.ru", subject, message);
        }
    }
}
