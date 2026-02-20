namespace ApiGestionTurnosMedicos.Services
{
    public record SendEmailRequest(
        
    string Subject,
    string Body,
    string To
        
    );
}