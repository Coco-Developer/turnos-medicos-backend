using System.Net.Mail;
using System.Net.Mime;

namespace BusinessLogic.AppLogic
{
    public interface IMessage
    {
        void SendEmail(string subject, string body, string to);
        void SendEmail(string subject, string htmlBody, string to, IEnumerable<LinkedResource>? inlineResources);
    }
}
