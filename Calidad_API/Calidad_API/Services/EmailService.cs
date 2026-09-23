using Azure.Identity;
using Calidad_API.Interfaces;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

namespace Calidad_API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private readonly GraphServiceClient _graph;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;

            var tenantId = config["Email:TenantId"]
                ?? throw new InvalidOperationException("Email:TenantId no configurado");
            var clientId = config["Email:ClientId"]
                ?? throw new InvalidOperationException("Email:ClientId no configurado");
            var clientSecret = config["Email:ClientSecret"]
                ?? throw new InvalidOperationException("Email:ClientSecret no configurado");

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            _graph = new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });
        }

        public Task SendAsync(string to, string subject, string bodyHtml, CancellationToken ct = default)
            => SendAsync(new[] { to }, subject, bodyHtml, ct);

        public async Task SendAsync(
            IEnumerable<string> to,
            string subject,
            string bodyHtml,
            CancellationToken ct = default)
        {
            var from = _config["Email:FromAddress"]
                ?? throw new InvalidOperationException("Email:FromAddress no configurado");

            var recipients = to
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (recipients.Count == 0)
            {
                _logger.LogWarning("Email no enviado: no hay destinatarios.");
                return;
            }

            var message = new Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = bodyHtml
                },
                ToRecipients = recipients.Select(addr => new Recipient
                {
                    EmailAddress = new EmailAddress { Address = addr }
                }).ToList()
            };

            var body = new SendMailPostRequestBody
            {
                Message = message,
                SaveToSentItems = true
            };

            try
            {
                // Envía como el buzón FromAddress
                await _graph.Users[from]
                    .SendMail
                    .PostAsync(body, cancellationToken: ct);

                _logger.LogInformation("Correo enviado por Graph. Asunto: {Subject}", subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo por Graph: {Subject}", subject);
                // No relanzar: el vale no debe fallar por el correo
            }
        }
    }
}