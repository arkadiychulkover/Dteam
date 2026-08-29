using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace DteamBackend.Services
{
    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "DTEAM Gaming";
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string bodyHtml);
        Task SendPasswordResetEmailAsync(string toEmail, string resetCode);
    }

    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_settings.Host))
                {
                    _logger.LogWarning("[SMTP] SMTP Host не настроен. Пропуск отправки письма на {Email}", toEmail);
                    return;
                }

                using var message = new MailMessage();
                message.From = new MailAddress(_settings.FromEmail, _settings.FromName);
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = bodyHtml;
                message.IsBodyHtml = true;

                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    EnableSsl = _settings.EnableSsl,
                    Credentials = !string.IsNullOrWhiteSpace(_settings.Username)
                        ? new NetworkCredential(_settings.Username, _settings.Password)
                        : null
                };

                await client.SendMailAsync(message);
                _logger.LogInformation("[SMTP] Письмо успешно отправлено на {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SMTP] Ошибка при отправке письма на {Email}", toEmail);
            }
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetCode)
        {
            var subject = "Код восстановления пароля — DTEAM";
            var bodyHtml = $@"
                <div style=""font-family: Arial, sans-serif; background-color: #0a0c14; color: #f1f5f9; padding: 24px; border-radius: 12px;"">
                    <h2 style=""color: #22d3ee;"">Восстановление доступа DTEAM</h2>
                    <p>Здравствуйте!</p>
                    <p>Вы запросили сброс пароля для вашей учетной записи DTEAM.</p>
                    <p>Ваш одноразовый код подтверждения:</p>
                    <div style=""background-color: #1e293b; color: #38bdf8; font-size: 20px; font-weight: bold; letter-spacing: 1px; padding: 16px; border-radius: 8px; text-align: center; margin: 20px 0; word-break: break-all;"">
                        {resetCode}
                    </div>
                    <p style=""font-size: 13px; color: #94a3b8;"">Код действителен в течение 15 минут. Если вы не запрашивали сброс пароля, просто проигнорируйте это письмо.</p>
                </div>
            ";

            await SendEmailAsync(toEmail, subject, bodyHtml);
        }
    }
}

