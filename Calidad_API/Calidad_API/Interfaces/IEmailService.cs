namespace Calidad_API.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string bodyHtml, CancellationToken ct = default);
        Task SendAsync(IEnumerable<string> to, string subject, string bodyHtml, CancellationToken ct = default);
    }
}
