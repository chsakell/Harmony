using Microsoft.Extensions.Options;
using Harmony.Application.Configurations;
using MimeKit;
using MailKit.Net.Smtp;
using Harmony.Notifications.Models;
using Harmony.Notifications.Contracts.Notifications.Email;

namespace Harmony.Notifications.Services.EmailProviders
{
    public class GmailEmailService : IEmailService
    {
        private readonly ILogger<GmailEmailService> _logger;
        private readonly GmailSettings settings;
        public GmailEmailService(IOptions<GmailSettings> options,
            ILogger<GmailEmailService> logger)
        {
            _logger = logger;
            settings = options.Value;
        }

        public async Task SendEmailAsync(string recipientAddress, string subject, string htmlMessage)
        {
            var message = new Message(new MailboxAddress("",
                                          recipientAddress),
                                          subject: subject,
                                          content: htmlMessage);

            await SendEmailAsync(message);
        }

        private async Task SendEmailAsync(Message message)
        {
            var mailMessage = CreateEmailMessage(message);

            await SendAsync(mailMessage);
        }

        private async Task SendEmailsAsync(List<Message> messages)
        {
            foreach (var message in messages)
            {
                await SendEmailAsync(message);
            }
        }

        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(settings.DisplayName, settings.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message.Content
            };

            return emailMessage;
        }

        private async Task SendAsync(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(settings.SmtpServer, settings.Port, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(settings.UserName, settings.Password);

                    await client.SendAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send email");
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }
        }
    }
}
