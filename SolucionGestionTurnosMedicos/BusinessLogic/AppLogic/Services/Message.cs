using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using Microsoft.Extensions.Options;

namespace BusinessLogic.AppLogic
{
    public class Message : IMessage
    {

        public void SendEmail(string subject, string body, string to)
        {
            // Se pasó el control de errores al Controlador.

            EmailSettings settings = new EmailSettings();
            var fromEmail = settings.Username;
            var password = settings.Password;

            var message = new MailMessage();
            message.From = new MailAddress(fromEmail!);
            message.Subject = subject;
            message.To.Add(new MailAddress(to));
            message.Body = body;
            message.IsBodyHtml = true;

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = settings.Port,
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            smtpClient.Send(message);
        }

        // Nuevo overload con recursos inline
        public static void SendEmail(string subject, string htmlBody, string to, IEnumerable<LinkedResource>? inlineResources)
        {
            EmailSettings settings = new();
            var fromEmail = settings.Username;
            var password = settings.Password;


            using var message = new MailMessage();
            message.From = new MailAddress(fromEmail!);
            message.To.Add(new MailAddress(to));
            message.Subject = subject;
            message.IsBodyHtml = true;

            if (inlineResources != null)
            {
                var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                foreach (var res in inlineResources)
                    htmlView.LinkedResources.Add(res);

                message.AlternateViews.Add(htmlView);
            }
            else
            {
                message.Body = htmlBody;
            }

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = settings.Port,
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            smtpClient.Send(message);
        }

    }
}
