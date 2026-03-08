namespace BusinessLogic.AppLogic
{
    public class EmailSettings
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
    }
}
