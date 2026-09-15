using Calidad_API.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Calidad_API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public Task SendAsync(string to, string subject, string bodyHtml, CancellationToken ct = default)
            => SendAsync(new[] { to }, subject, bodyHtml, ct);

        public async Task SendAsync(IEnumerable<string> to, string subject, string bodyHtml, CancellationToken ct = default)
        {
            var section = _config.GetSection("Email");
            var host = section["SmtpHost"] ?? throw new InvalidOperationException("Email:SmtpHost no configurado");
            var port = int.Parse(section["SmtpPort"] ?? "587");
            var useSsl = bool.Parse(section["UseSsl"] ?? "true");
            var user = section["User"];
            var password = section["Password"];
            var fromName = section["FromName"] ?? "Modulo Calidad";
            var fromAddress = section["FromAddress"] ?? user;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            foreach (var addr in to.Where(x => !string.IsNullOrWhiteSpace(x)))
                message.To.Add(MailboxAddress.Parse(addr.Trim()));

            if (!message.To.Any())
            {
                _logger.LogWarning("Email no enviado: no hay destinatarios.");
                return;
            }

            message.Subject = subject;
            message.Body = new TextPart("html") { Text = bodyHtml };

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(host, port, useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, ct);
                if (!string.IsNullOrWhiteSpace(user))
                    await client.AuthenticateAsync(user, password, ct);

                await client.SendAsync(message, ct);
                await client.DisconnectAsync(true, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo: {Subject}", subject);
                // No relanzamos para que la generación del vale no falle por el correo
            }
        }
    }
}