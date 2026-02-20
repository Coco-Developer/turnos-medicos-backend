using System.Net.Mime;
using System.Net.Mail;
using ApiGestionTurnosMedicos.CustomModels;
using DataAccess.Data;

namespace BusinessLogic.AppLogic.Services
{
    /// <summary>
    /// Interfaz para abstracción del envío de emails (facilita mockear en tests).
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Envía un email en HTML. `inlineResources` puede ser null.
        /// </summary>
        void SendEmail(string subject, string htmlBody, string to, IEnumerable<LinkedResource> inlineResources = null);
    }

    /// <summary>
    /// Implementación que delega en la clase estática `Message` existente.
    /// Mantiene la compatibilidad con la lógica del proyecto actual.
    /// </summary>
    public class SmtpEmailSender : IEmailSender
    {
        public void SendEmail(string subject, string htmlBody, string to, IEnumerable<LinkedResource> inlineResources = null)
        {
            // Si `Message.SendEmail` acepta array de LinkedResource, convertir.
            Message.SendEmail(subject, htmlBody, to, inlineResources?.ToArray());
        }
    }

    /// <summary>
    /// Servicio de emails refactorizado para inyección de `IEmailSender`.
    /// Pasar un `IEmailSender` (o un Mock en tests) al construir la instancia.
    /// </summary>
    public class EmailService
    {
        private readonly IEmailSender _emailSender;

        public EmailService(IEmailSender emailSender)
        {
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        }

        public void SendShiftConfirmationEmail(Turno turno, Paciente paciente, MedicoConEspecialidad medico)
        {
            if (turno == null) throw new ArgumentNullException(nameof(turno));
            if (paciente == null) throw new ArgumentNullException(nameof(paciente));
            if (medico == null) throw new ArgumentNullException(nameof(medico));

            // Ruta del logo (se copia al output por configuración de VS)
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
                <style>
                    @import url('https://fonts.googleapis.com/css2?family=Ubuntu:ital,wght@0,300;0,400;0,500;0,700;1,300;1,400;1,500;1,700&display=swap');
                    @import url('https://fonts.googleapis.com/css2?family=Rajdhani:wght@300;400;500;600;700&display=swap');

                    body {{
                        font-family: ""Ubuntu"", -apple-system, BlinkMacSystemFont, ""Bitstream Vera Sans"", ""DejaVu Sans"", Tahoma, 'Segoe UI', 'Roboto', 'Droid Sans', 'Helvetica Neue', sans-serif !important;
                        background-color: #fff;
                        padding: 20px;
                    }}
                    .app-logo {{
                        color: #fff;
                        background-color: #004c3c;
                        border-radius: 10px;
                        font-family: ""Rajdhani"", sans-serif !important;
                        
                        font-weight: 300 !important;
                        text-align:center;
                    }}
                    .app-logo span {{
                        font-size: 2rem;
                    }}
                    .app-logo b {{
                        font-weight: 700  !important;
                    }}
                    .container {{
                        margin-left: auto;
                        margin-right: auto;
                        background: #e8e8e8;
                        padding: 20px;
                        border-radius: 10px;
                        max-width: 600px;
                        margin: auto;
                        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                    }}
                    .title {{
                        text-align: center;
                        font-size: 24px;
                        font-weight: bold;
                        color: #0C3883;
                    }}
                    .highlight {{
                        font-weight: bold;
                    }}
                </style>
            </head>

            <body>
                <div class='container'>
                    <div class='app-logo'>
                        <img src='cid:cm-logo' width='128' height='128'><br><span>Chrono<b>Med</b></span>
                    </div>
                    <div class='title'>Asignación de turno para consulta de: {paciente.NombreCompletoPaciente().ToUpper()}</div>
                    </div>
                    <p>Estimado/a usuario <span class='highlight'>{paciente.NombreCompletoPaciente().ToUpper()}</span>:</p>
                    <p>Te recordamos que has reservado un turno para una consulta con los siguientes datos:</p>
                    <p class='section-title'>Datos del paciente:</p>
                    <p>Nombre completo: <span class='highlight'>{paciente.NombreCompletoPaciente()}</span></p>
                    <p>Dni: <span class='highlight'>{paciente.Dni}</span></p>
                    <p>Edad: <span class='highlight'>{paciente.EdadDelPaciente(paciente.FechaNacimiento)} años</span></p>
                    <p class='section-title'>Constancia del Turno:</p>
                    <p>Especialidad: <span class='highlight'>{medico.Especialidad}</span></p>
                    <p>Médico: <span class='highlight'>{medico.NombreCompletoMedico(medico.Nombre,
                            medico.Apellido).ToUpper()}</span></p>
                    <p>Turno para el día: <span class='highlight'>{turno.Fecha:dd/MM/yyyy} - Hora: {horaTurno}</span></p>
                    <p style='margin-top:2rem;font-size:80%'>Si <span class='highlight'>no vas a asistir</span> a tu consulta presencial o por Telemedicina, es <span
                            class='highlight'>importante</span> que la canceles o reprogrames para que otro paciente pueda tomarla.<br>
                       Podés cancelarlo desde la aplicación móvil.
                    </p>
                </div>
            </body>

            </html>";

            // Añadir el logo incrustado
            var logo = new LinkedResource(logoPath)
            {
                ContentId = "cm-logo", // ¡IMPORTANTE! Debe coincidir con cid:cm-logo en HTML
                TransferEncoding = System.Net.Mime.TransferEncoding.Base64,
                ContentType = new ContentType("image/png")
            };

            _emailSender.SendEmail("Constancia de turno", emailBody, paciente.Email, new[] { logo });
        }

        /// <summary>
        /// Envía un email con la nueva contraseña al destinatario especificado.
        /// </summary>
        /// <param name="email">El email del usuario. DEBE estar en la tabla Usuario.</param>
        /// <param name="newPassword">La nueva contraseña</param>
        public void SendPasswordResetEmail(string email, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException(nameof(email));
            if (newPassword == null) throw new ArgumentNullException(nameof(newPassword));

            // Ruta del logo (se copia al output por configuración de VS)
            string logoPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets", "Images", "logo192.png"
            );

            string htmlBody = $@"
            <html>
            <head>
                <meta charset='UTF-8'>
                <style>
                    @import url('https://fonts.googleapis.com/css2?family=Ubuntu:ital,wght@0,300;0,400;0,500;0,700;1,300;1,400;1,500;1,700&display=swap');
                    @import url('https://fonts.googleapis.com/css2?family=Rajdhani:wght@300;400;500;600;700&display=swap');

                    body {{
                        font-family: ""Ubuntu"", -apple-system, BlinkMacSystemFont, ""Bitstream Vera Sans"", ""DejaVu Sans"", Tahoma, 'Segoe UI', 'Roboto', 'Droid Sans', 'Helvetica Neue', sans-serif !important;
                        background-color: #fff;
                        padding: 20px;
                    }}
                    .app-logo {{
                        color: #fff;
                        background-color: #004c3c;
                        border-radius: 10px;
                        font-family: ""Rajdhani"", sans-serif !important;
                        
                        font-weight: 300 !important;
                        text-align:center;
                    }}
                    .app-logo span {{
                        font-size: 2rem;
                    }}
                    .app-logo b {{
                        font-weight: 700  !important;
                    }}
                    .container {{
                        margin-left: auto;
                        margin-right: auto;
                        background: #e8e8e8;
                        padding: 20px;
                        border-radius: 10px;
                        max-width: 600px;
                        margin: auto;
                        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                    }}
                    .title {{
                        text-align: center;
                        font-size: 24px;
                        font-weight: bold;
                        color: #0C3883;
                    }}
                    .highlight {{
                        font-weight: bold;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='app-logo'>
                        <img src='cid:cm-logo' width='128' height='128'><br><span>Chrono<b>Med</b></span>
                    </div>
                    <h1 class='title'>Nueva Contraseña</h1>
                    <p>Estimado usuario:</p>
                    <p>Tu contraseña ha sido restablecida exitosamente.</p>
                    <p>Tu nueva contraseña es: <strong class='highlight'>{newPassword}</strong></p>
                    <p>Por favor, inicia sesión con esta nueva contraseña.</p>
                </div>
            </body>
            </html>";

            // Añadir el logo incrustado
            var logo = new LinkedResource(logoPath)
            {
                ContentId = "cm-logo", // ¡IMPORTANTE! Debe coincidir con cid:cm-logo en HTML
                TransferEncoding = System.Net.Mime.TransferEncoding.Base64,
                ContentType = new ContentType("image/png")
            };

            _emailSender.SendEmail("Nueva Contraseña", htmlBody, email, new[] { logo });
        }
    }
}
