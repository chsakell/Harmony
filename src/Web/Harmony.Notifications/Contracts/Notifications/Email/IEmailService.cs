namespace Harmony.Notifications.Contracts.Notifications.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string recipientAddress, string subject, string htmlContent);
    }
}
