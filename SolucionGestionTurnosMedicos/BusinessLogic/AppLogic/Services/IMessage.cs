namespace BusinessLogic.AppLogic
{
    public interface IMessage
    {
        void SendEmail(string subject, string body, string to);

    }
}
