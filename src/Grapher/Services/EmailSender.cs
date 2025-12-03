using Microsoft.AspNetCore.Identity.UI.Services;

namespace Grapher.Services;

// Dummy email sender class
public class EmailSender : IEmailSender {
    public Task SendEmailAsync(string email, string subject, string htmlMessage) {
        Console.WriteLine($"\n--- EMAIL TO {email} ---\nSubject: {subject}\nMessage: {htmlMessage}\n-----------------------\n");
        return Task.CompletedTask;
    }
}

