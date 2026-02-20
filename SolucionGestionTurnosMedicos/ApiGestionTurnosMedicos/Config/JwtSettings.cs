namespace ApiGestionTurnosMedicos.Config
{
    public class JwtSettings
    {
        public string Key { get; set; } = default!;
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public string ExpiresInMinutes { get; set; } = "60";
    }
}
