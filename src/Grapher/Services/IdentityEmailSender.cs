using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Grapher.Services
{
    // Adapter so Identity UI can get an IEmailSender while we use our implementation internally
    public class IdentityEmailSender : Microsoft.AspNetCore.Identity.UI.Services.IEmailSender
    {
        private readonly IEmailSender _inner;

        public IdentityEmailSender(IEmailSender inner)
        {
            _inner = inner;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // delegate to your SmtpEmailSender (plain text body ok)
            return _inner.SendEmailAsync(email, subject, htmlMessage);
        }
    }
}
