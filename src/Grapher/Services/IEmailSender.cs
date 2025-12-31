using System.Threading.Tasks;

namespace Grapher.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
