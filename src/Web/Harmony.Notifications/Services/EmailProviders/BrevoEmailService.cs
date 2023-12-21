using Microsoft.Extensions.Options;
using Harmony.Application.Configurations;
using Microsoft.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Harmony.Notifications.Contracts.Notifications.Email;

namespace Harmony.Notifications.Services.EmailProviders
{
    public class BrevoEmailService : IEmailService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BrevoEmailService> _logger;
        private readonly BrevoSettings settings;
        public BrevoEmailService(IOptions<BrevoSettings> options,
            IHttpClientFactory httpClientFactory,
            ILogger<BrevoEmailService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            settings = options.Value;
        }

        public async Task SendEmailAsync(string recipientAddress, string subject, string htmlContent)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var brevoData = new BrevoBody()
            {
                Sender = new BrevoSender()
                {
                    Email = settings.SenderEmail,
                    Name = settings.SenderName
                },
                Subject = subject,
                To = new List<BrevoRecipient>
                {
                    new BrevoRecipient()
                    {
                        Email = recipientAddress
                    }
                },
                HtmlContent = htmlContent
            };

            var json = JsonSerializer.Serialize(brevoData);

            var httpRequestMessage = new HttpRequestMessage(
             HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
            {
                Headers =
                {
                    { HeaderNames.Accept, "application/json" },
                    { "api-key", settings.ApiKey }
                },
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            if(!httpResponseMessage.IsSuccessStatusCode)
            {
                var response = await httpResponseMessage.Content.ReadAsStringAsync();
                _logger.LogError($"Error sending email with Brevo Status {httpResponseMessage.StatusCode} Message: {response}");
            }
        }
    }

    public class BrevoBody
    {
        [JsonPropertyName("sender")]
        public BrevoSender Sender { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("htmlContent")]
        public string HtmlContent { get; set; }

        [JsonPropertyName("to")]
        public List<BrevoRecipient> To { get; set; }
    }

    public class BrevoSender
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }
    }

    public class BrevoRecipient
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
