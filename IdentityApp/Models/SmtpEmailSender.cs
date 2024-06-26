using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace IdentityApp.Models
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly string _host;
        private readonly int _port;
        private readonly bool _enableSSL;
        private readonly string _username;
        private readonly string _password;

        public SmtpEmailSender(string host, int port, bool enableSSL, string username, string password)
        {
            _host = host;
            _port = port;
            _enableSSL = enableSSL;
            _username = username;
            _password = password;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            if (!IsValidEmail(email))
            {
                throw new FormatException("The specified string is not in the form required for an e-mail address.");
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_username ?? throw new InvalidOperationException("Username cannot be null")),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            using (var client = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_username, _password),
                EnableSsl = _enableSSL
            })
            {
                await client.SendMailAsync(mailMessage);
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                email = email.Trim();
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
