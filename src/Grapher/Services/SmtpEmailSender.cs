using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Grapher.Services
{
    public class SmtpOptions
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 25;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool EnableSsl { get; set; } = false;
        public string From { get; set; } = "no-reply@example.com";
    }

    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _options;

        public SmtpEmailSender(IOptions<SmtpOptions> options)
        {
            _options = options.Value;
        }

        public Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MailMessage(_options.From, to, subject, body);
            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl
            };

            if (!string.IsNullOrEmpty(_options.Username))
            {
                client.Credentials = new NetworkCredential(_options.Username, _options.Password);
            }

            return client.SendMailAsync(message);
        }
    }
}
