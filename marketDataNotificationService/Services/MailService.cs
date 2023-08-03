using marketDataNotificationService.Models;
using MediatR;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace marketDataNotificationService {
    public class MailService : INotificationHandler<GlobalQuote> {
        private readonly ILogger<MailService> _logger;
        private readonly IOptionsMonitor<EmailConfig> _emailConfig;
        private readonly SubscriptionRepository _subscriptionRepository;

        public MailService(ILogger<MailService> logger, IOptionsMonitor<EmailConfig> emailConfig, SubscriptionRepository subscriptionRepository) {
            _logger = logger;
            _emailConfig = emailConfig;
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task Handle(GlobalQuote notification, CancellationToken cancellationToken) {
            if (_subscriptionRepository.SubscribersByTicker.TryGetValue(notification.Symbol, out var subscribers)) {
                foreach (var item in subscribers) {
                    if (notification.Price > item.UpperBound) {
                        await SendMail($"Upper bound reached for {notification.Symbol}", $"The upper limit of {item.UpperBound} for {notification.Symbol} was reached. Current price is {notification.Price}. Asset sale is recomended", item.Subscriber);
                    }
                    else if (notification.Price < item.LowerBound) {
                        await SendMail($"Lower bound reached for {notification.Symbol}", $"The lower limit of {item.LowerBound} for {notification.Symbol} was reached. Current price is {notification.Price}. Asset purchase is recomended", item.Subscriber);
                    }
                    else {
                        _logger.LogInformation("Last Price was {price}, no bound reached", notification.Price);
                    }
                }
            }
        }

        async Task SendMail(string subject, string message, string recipient) {
            try {
                MailMessage mail = new MailMessage() {
                    From = new MailAddress(_emailConfig.CurrentValue.UsernameEmail)
                };

                mail.To.Add(new MailAddress(recipient));
                _emailConfig.CurrentValue.CcEmail.ForEach(item => mail.CC.Add(new MailAddress(item)));

                mail.Subject = subject;
                mail.Body = message;
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.High;

                using (SmtpClient smtp = new SmtpClient(_emailConfig.CurrentValue.Domain, _emailConfig.CurrentValue.Port)) {
                    smtp.Credentials = new NetworkCredential(_emailConfig.CurrentValue.UsernameEmail, _emailConfig.CurrentValue.UsernamePassword);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mail);
                }
                _logger.LogInformation("Email with Subject \"{subject}\" and message \"{message}\" for {destination} sent", subject, message, recipient);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error sending mail with {subject}", subject);
            }
        }
    }
}
