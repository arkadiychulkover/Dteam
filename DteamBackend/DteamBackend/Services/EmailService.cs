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
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<EmailSettings> settings, IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _settings = settings.Value;
            _configuration = configuration;
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
            var frontendUrl = _configuration["FrontendUrl"] ?? _configuration["FRONTEND_URL"] ?? "https://dteam-app.vercel.app";
            var subject = "Код відновлення пароля — DTEAM";
            var bodyHtml = $@"
                <div style=""font-family: Arial, sans-serif; background-color: #0a0c14; color: #f1f5f9; padding: 28px; border-radius: 12px; max-width: 500px; margin: 0 auto; border: 1px solid #1e293b;"">
                    <h2 style=""color: #22d3ee; margin-top: 0;"">Відновлення доступу DTEAM</h2>
                    <p>Вітаємо!</p>
                    <p>Ви надіслали запит на скидання пароля для вашого облікового запису DTEAM.</p>
                    <p>Ваш одноразовий код підтвердження:</p>
                    <div style=""background-color: #1e293b; color: #38bdf8; font-size: 24px; font-weight: bold; letter-spacing: 2px; padding: 16px; border-radius: 8px; text-align: center; margin: 20px 0; word-break: break-all; border: 1px solid #334155;"">
                        {resetCode}
                    </div>
                    <div style=""text-align: center; margin: 24px 0;"">
                        <a href=""{frontendUrl}"" style=""display: inline-block; padding: 12px 28px; background: linear-gradient(135deg, #06b6d4, #3b82f6); color: #ffffff; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 15px;"">Перейти до DTEAM</a>
                    </div>
                    <p style=""font-size: 13px; color: #94a3b8; margin-bottom: 0;"">Код дійсний протягом 15 хвилин. Якщо ви не надсилали запит на скидання пароля, просто проігноруйте цей лист.</p>
                </div>
            ";

            await SendEmailAsync(toEmail, subject, bodyHtml);
        }
    }
}
