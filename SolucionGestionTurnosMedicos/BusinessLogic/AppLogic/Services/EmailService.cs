using DataAccess.Data;
using Microsoft.Extensions.Logging;
using Models.CustomModels;
using System.Net.Mail;
using System.Net.Mime;

namespace BusinessLogic.AppLogic.Services
{
    public interface IEmailSender
    {
        void SendEmail(string subject, string htmlBody, string to, IEnumerable<LinkedResource>? inlineResources = null);
    }

    public class SmtpEmailSender : IEmailSender
    {
        private readonly IMessage _message;

        public SmtpEmailSender(IMessage message)
        {
            _message = message;
        }

        public void SendEmail(string subject, string htmlBody, string to, IEnumerable<LinkedResource>? inlineResources = null)
        {
            _message.SendEmail(subject, htmlBody, to, inlineResources);
        }
    }

    public class EmailService
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IEmailSender emailSender, ILogger<EmailService> logger)
        {
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _logger = logger;
        }

        public void SendShiftConfirmationEmail(Turno turno, Paciente paciente, MedicoConEspecialidad medico)
        {
            if (turno == null) throw new ArgumentNullException(nameof(turno));
            if (paciente == null) throw new ArgumentNullException(nameof(paciente));
            if (medico == null) throw new ArgumentNullException(nameof(medico));

            // Si no hay email, no cortar el flujo de negocio
            if (string.IsNullOrWhiteSpace(paciente.Email))
            {
                _logger.LogWarning("No se envió email de turno: paciente {PacienteId} sin email.", paciente.Id);
                return;
            }

            string logoPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets", "Images", "logo192.png"
            );

            string horaTurno = turno.Hora.ToString(@"hh\:mm");

            string emailBody = $@"
            <html lang='es'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Confirmación de Turno</title>
            </head>
            <body style='font-family:Arial, sans-serif; background:#fff; padding:20px;'>
                <div style='max-width:600px;margin:auto;background:#e8e8e8;padding:20px;border-radius:10px;'>
                    <div style='text-align:center;background:#004c3c;color:#fff;border-radius:10px;padding:12px;'>
                        <img src='cid:cm-logo' width='96' height='96' /><br />
                        <span style='font-size:1.5rem;'>Chrono<b>Med</b></span>
                    </div>
                    <h2 style='color:#0C3883;'>Asignación de turno para: {paciente.NombreCompletoPaciente().ToUpper()}</h2>
                    <p>Estimado/a <b>{paciente.NombreCompletoPaciente().ToUpper()}</b>:</p>
                    <p>Recordamos tu turno con estos datos:</p>
                    <p><b>Paciente:</b> {paciente.NombreCompletoPaciente()}</p>
                    <p><b>DNI:</b> {paciente.Dni}</p>
                    <p><b>Especialidad:</b> {medico.Especialidad}</p>
                    <p><b>Médico:</b> {medico.NombreCompletoMedico(medico.Nombre, medico.Apellido).ToUpper()}</p>
                    <p><b>Fecha y hora:</b> {turno.Fecha:dd/MM/yyyy} - {horaTurno}</p>
                </div>
            </body>
            </html>";

            try
            {
                LinkedResource? logo = null;

                if (File.Exists(logoPath))
                {
                    logo = new LinkedResource(logoPath)
                    {
                        ContentId = "cm-logo",
                        TransferEncoding = System.Net.Mime.TransferEncoding.Base64,
                        ContentType = new ContentType("image/png")
                    };
                }

                _emailSender.SendEmail(
                    "Constancia de turno",
                    emailBody,
                    paciente.Email,
                    logo != null ? new[] { logo } : null
                );
            }
            catch (Exception ex)
            {
                // Loguea pero no rompe
                _logger.LogWarning(ex, "Falló el envío de email de turno para paciente {PacienteId}", paciente.Id);
            }
        }

        public void SendPasswordResetEmail(string email, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email));
            if (newPassword == null)
                throw new ArgumentNullException(nameof(newPassword));

            string logoPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets", "Images", "logo192.png"
            );

            string htmlBody = $@"
            <html>
            <head><meta charset='UTF-8'></head>
            <body style='font-family:Arial, sans-serif; background:#fff; padding:20px;'>
                <div style='max-width:600px;margin:auto;background:#e8e8e8;padding:20px;border-radius:10px;'>
                    <div style='text-align:center;background:#004c3c;color:#fff;border-radius:10px;padding:12px;'>
                        <img src='cid:cm-logo' width='96' height='96' /><br />
                        <span style='font-size:1.5rem;'>Chrono<b>Med</b></span>
                    </div>
                    <h2 style='color:#0C3883;'>Nueva Contraseña</h2>
                    <p>Tu contraseña fue restablecida.</p>
                    <p><b>Nueva contraseña:</b> {newPassword}</p>
                </div>
            </body>
            </html>";

            try
            {
                LinkedResource? logo = null;

                if (File.Exists(logoPath))
                {
                    logo = new LinkedResource(logoPath)
                    {
                        ContentId = "cm-logo",
                        TransferEncoding = System.Net.Mime.TransferEncoding.Base64,
                        ContentType = new ContentType("image/png")
                    };
                }

                _emailSender.SendEmail(
                    "Nueva Contraseña",
                    htmlBody,
                    email,
                    logo != null ? new[] { logo } : null
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falló el envío de email de reset para {Email}", email);
                throw;
            }
        }
    }
}
