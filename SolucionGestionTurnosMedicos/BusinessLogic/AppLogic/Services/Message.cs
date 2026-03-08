using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace BusinessLogic.AppLogic
{
    public class Message : IMessage
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<Message> _logger;

        public Message(IOptions<EmailSettings> options, ILogger<Message> logger)
        {
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
        }

        public void SendEmail(string subject, string body, string to)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Email destino vacío", nameof(to));

            ValidateSettings();

            using var message = new MailMessage();
            message.From = new MailAddress(_settings.Username);
            message.Subject = subject ?? string.Empty;
            message.To.Add(new MailAddress(to));
            message.Body = body ?? string.Empty;
            message.IsBodyHtml = true;

            using var smtpClient = BuildSmtpClient();
            smtpClient.Send(message);
        }

        public void SendEmail(string subject, string htmlBody, string to, IEnumerable<LinkedResource>? inlineResources)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Email destino vacío", nameof(to));

            ValidateSettings();

            using var message = new MailMessage();
            message.From = new MailAddress(_settings.Username);
            message.To.Add(new MailAddress(to));
            message.Subject = subject ?? string.Empty;
            message.IsBodyHtml = true;

            if (inlineResources != null && inlineResources.Any())
            {
                var htmlView = AlternateView.CreateAlternateViewFromString(
                    htmlBody ?? string.Empty,
                    null,
                    MediaTypeNames.Text.Html
                );

                foreach (var resource in inlineResources)
                {
                    if (resource != null)
                        htmlView.LinkedResources.Add(resource);
                }

                message.AlternateViews.Add(htmlView);
            }
            else
            {
                message.Body = htmlBody ?? string.Empty;
            }

            using var smtpClient = BuildSmtpClient();
            smtpClient.Send(message);
        }

        private SmtpClient BuildSmtpClient()
        {
            return new SmtpClient(_settings.Host)
            {
                Port = _settings.Port,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };
        }

        private void ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(_settings.Host))
                throw new InvalidOperationException("EmailSettings.Host no configurado.");

            if (string.IsNullOrWhiteSpace(_settings.Username))
                throw new InvalidOperationException("EmailSettings.Username no configurado.");

            if (string.IsNullOrWhiteSpace(_settings.Password))
                throw new InvalidOperationException("EmailSettings.Password no configurado.");
        }
    }
}
