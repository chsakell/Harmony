namespace Harmony.Notifications.Contracts
{
    public interface IEmailService
    {
        Task SendEmailAsync(string recipientAddress, string subject, string htmlContent);
    }
}
