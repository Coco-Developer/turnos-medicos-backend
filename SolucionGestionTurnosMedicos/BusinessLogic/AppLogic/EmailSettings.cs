namespace BusinessLogic.AppLogic
{
    public class EmailSettings
    {
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public int Port { get; set; } = 587; // se puede parametrizar también si quieres
    }
}