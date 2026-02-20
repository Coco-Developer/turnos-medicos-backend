using System.Net.Mail;
using BusinessLogic.AppLogic.Services;
using ApiGestionTurnosMedicos.CustomModels;

namespace Testing
{
    public class EmailServiceTests
    {
        // Verificar que la llamada a SendShiftConfirmationEmail prepara contenido esperado
        [Fact]
        public void SendShiftConfirmationEmail_BuildsExpectedHtmlAndInvokesSend()
        {
            // Arrange
            var mockSender = new Mock<IEmailSender>();
            string capturedSubject = null;
            string capturedBody = null;
            string capturedTo = null;
            mockSender.Setup(s => s.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<LinkedResource>>()))
                .Callback<string, string, string, IEnumerable<LinkedResource>>((sub, body, to, inline) =>
                {
                    capturedSubject = sub;
                    capturedBody = body;
                    capturedTo = to;
                });

            var service = new EmailService(mockSender.Object);

            var turno = new Turno { Fecha = new DateTime(2025, 12, 31), Hora = new TimeSpan(9, 30, 0) };
            var paciente = new Paciente { Nombre = "Juan", Apellido = "Prueba", Email = "jp@test.com", Dni = "12345678", FechaNacimiento = new DateTime(1990, 1, 1) };
            var medico = new MedicoConEspecialidad { Nombre = "Dr", Apellido = "Test", Especialidad = "Medicina General" };

            // Act
            service.SendShiftConfirmationEmail(turno, paciente, medico);

            // Assert
            mockSender.Verify(s => s.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<LinkedResource>>()), Times.Once);
            Assert.Equal("jp@test.com", capturedTo);
            Assert.Contains("Constancia de turno", capturedSubject);
            Assert.Contains("31/12/2025", capturedBody);
            Assert.Contains("09:30", capturedBody);
            Assert.Contains("JUAN PRUEBA", capturedBody.ToUpperInvariant());
            Assert.Contains("DR TEST", capturedBody.ToUpperInvariant());
            Assert.Contains("Medicina General", capturedBody);
        }

        // Verificar que SendPasswordResetEmail incluye la nueva contraseña y título
        [Fact]
        public void SendPasswordResetEmail_BuildsExpectedHtmlAndInvokesSend()
        {
            // Arrange
            var mockSender = new Mock<IEmailSender>();
            string capturedSubject = null;
            string capturedBody = null;
            string capturedTo = null;
            mockSender.Setup(s => s.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<LinkedResource>>()))
                .Callback<string, string, string, IEnumerable<LinkedResource>>((sub, body, to, inline) =>
                {
                    capturedSubject = sub;
                    capturedBody = body;
                    capturedTo = to;
                });

            var service = new EmailService(mockSender.Object);

            var email = "usuario@example.com";
            var newPassword = "Abc123!@#";

            // Act
            service.SendPasswordResetEmail(email, newPassword);
            //
            // Assert
            Assert.Equal(email, capturedTo);
            Assert.Contains("Nueva Contraseña", capturedSubject);
            Assert.Contains(newPassword, capturedBody);
        }
    }
}