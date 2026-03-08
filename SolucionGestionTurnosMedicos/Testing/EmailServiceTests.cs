using System.Net.Mail;
using BusinessLogic.AppLogic.Services;
using Xunit;
using Moq;
using Models.CustomModels;
using System.Collections.Generic;
using System.Net.Mime;
using Microsoft.Extensions.Logging;

namespace Testing
{
    public class EmailServiceTests
    {
        // Verificar que la llamada a SendShiftConfirmationEmail prepara contenido esperado
        [Fact]
        public void SendShiftConfirmationEmail_BuildsExpectedHtmlAndInvokesSend()
        {
            var mockSender = new Mock<IEmailSender>();
            string capturedSubject = null;
            string capturedBody = null;
            string capturedTo = null;
            mockSender.Setup(s => s.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<System.Net.Mail.LinkedResource>>()))
                .Callback<string, string, string, IEnumerable<System.Net.Mail.LinkedResource>>((sub, body, to, inline) =>
                {
                    capturedSubject = sub;
                    capturedBody = body;
                    capturedTo = to;
                });

            var mockLogger = new Mock<ILogger<EmailService>>();

            var service = new EmailService(mockSender.Object, mockLogger.Object);

            var turno = new Turno { Fecha = new DateTime(2025, 12, 31), Hora = new TimeSpan(9, 30, 0) };
            var paciente = new Paciente { Nombre = "Juan", Apellido = "Prueba", Email = "jp@test.com", Dni = "12345678", FechaNacimiento = new DateTime(1990, 1, 1) };
            var medico = new MedicoConEspecialidad { Nombre = "Dr", Apellido = "Test", Especialidad = "Medicina General" };

            // Act
            service.SendShiftConfirmationEmail(turno, paciente, medico);

            mockSender.Verify(s => s.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<System.Net.Mail.LinkedResource>>()), Times.Once);
            Xunit.Assert.Equal("jp@test.com", capturedTo);
            Xunit.Assert.Contains("Constancia de turno", capturedSubject);
            Xunit.Assert.Contains("31/12/2025", capturedBody);
            Xunit.Assert.Contains("09:30", capturedBody);
            Xunit.Assert.Contains("JUAN PRUEBA", capturedBody.ToUpperInvariant());
            Xunit.Assert.Contains("DR TEST", capturedBody.ToUpperInvariant());
            Xunit.Assert.Contains("Medicina General", capturedBody);
        }

        [Fact]
        public void SendPasswordResetEmail_BuildsExpectedHtmlAndInvokesSend()
        {
            var mockSender = new Mock<IEmailSender>();
            string capturedSubject = null;
            string capturedBody = null;
            string capturedTo = null;
            mockSender.Setup(s => s.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<System.Net.Mail.LinkedResource>>()))
                .Callback<string, string, string, IEnumerable<System.Net.Mail.LinkedResource>>((sub, body, to, inline) =>
                {
                    capturedSubject = sub;
                    capturedBody = body;
                    capturedTo = to;
                });

            var mockLogger = new Mock<ILogger<EmailService>>();
            var service = new EmailService(mockSender.Object, mockLogger.Object);

            var email = "usuario@example.com";
            var newPassword = "Abc123!@#";

            service.SendPasswordResetEmail(email, newPassword);

            Xunit.Assert.Equal(email, capturedTo);
            Xunit.Assert.Contains("Nueva Contraseña", capturedSubject);
            Xunit.Assert.Contains(newPassword, capturedBody);
        }
    }
}