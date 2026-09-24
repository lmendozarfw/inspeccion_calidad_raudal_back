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

        public Task SendAsync(
    IEnumerable<string> to,
    string subject,
    string bodyHtml,
    CancellationToken ct = default)
    => SendAsync(to, subject, bodyHtml, null, null, ct);

        public async Task SendAsync(
            IEnumerable<string> to,
            string subject,
            string bodyHtml,
            string? attachmentPath,
            string? attachmentFileName = null,
            CancellationToken ct = default)
        {
            var host = _config["Email:SmtpHost"];
            var port = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var user = _config["Email:User"];
            var password = _config["Email:Password"];
            var fromName = _config["Email:FromName"] ?? "Módulo Calidad";
            var fromAddress = _config["Email:FromAddress"] ?? user;

            if (string.IsNullOrWhiteSpace(host))
            {
                _logger.LogWarning("Email:SmtpHost vacío. No se envía correo.");
                return;
            }

            var recipients = to
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (recipients.Count == 0)
            {
                _logger.LogWarning("Email sin destinatarios.");
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            foreach (var addr in recipients)
                message.To.Add(MailboxAddress.Parse(addr));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = bodyHtml
            };

            if (!string.IsNullOrWhiteSpace(attachmentPath) && File.Exists(attachmentPath))
            {
                var name = string.IsNullOrWhiteSpace(attachmentFileName)
                    ? Path.GetFileName(attachmentPath)
                    : attachmentFileName;

                builder.Attachments.Add(name, await File.ReadAllBytesAsync(attachmentPath, ct));
            }

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();

            client.ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
            {
                if (errors == System.Net.Security.SslPolicyErrors.None)
                    return true;

                if (errors == System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch
                    && certificate?.Subject?.Contains("cloudfilter", StringComparison.OrdinalIgnoreCase) == true)
                    return true;

                return false;
            };

            try
            {
                var secure = port == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;

                await client.ConnectAsync(host, port, secure, ct);

                if (!string.IsNullOrWhiteSpace(user))
                    await client.AuthenticateAsync(user, password, ct);

                await client.SendAsync(message, ct);
                await client.DisconnectAsync(true, ct);

                _logger.LogInformation("Correo SMTP enviado. Asunto: {Subject}", subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo SMTP: {Subject}", subject);
            }
        }
    }
}