using Microsoft.AspNetCore.Identity.UI.Services;

namespace Grapher.Services;

// This implements Microsoft's interface but uses YOUR sender internally
public class IdentityEmailSender : Microsoft.AspNetCore.Identity.UI.Services.IEmailSender
{
    private readonly Grapher.Services.IEmailSender _realSender;

    public IdentityEmailSender(Grapher.Services.IEmailSender realSender)
    {
        _realSender = realSender;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Delegate the work to your custom SMTP sender
        return _realSender.SendEmailAsync(email, subject, htmlMessage);
    }
}
