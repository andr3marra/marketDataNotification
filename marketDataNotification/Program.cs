using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using System.Net.Http.Json;

namespace marketDataNotification;
internal partial class Program {
    private static EmailConfig emailConfig;
    private static MarketDataApiConfig marketDataApiConfig;
    private static ILogger<Program> logger;
    static async Task Main(string[] args) {
        if (args.Length != 3) {
            throw new ArgumentException($"Invalid argument count. Expected: 3 Actual: {args.Length}");
        }

        if (!decimal.TryParse(args[1], out var lowerBound)) {
            throw new ArgumentException($"Invalid argument {args[1]}", "LowerBound");
        }

        if (!decimal.TryParse(args[2], out var upperBound)) {
            throw new ArgumentException($"Invalid argument {args[2]}", "UpperBound");
        }

        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        emailConfig = new EmailConfig();
        config.GetSection(nameof(EmailConfig)).Bind(emailConfig);
        marketDataApiConfig = new MarketDataApiConfig();
        config.GetSection(nameof(MarketDataApiConfig)).Bind(marketDataApiConfig);

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

        logger = loggerFactory.CreateLogger<Program>();

        logger.LogInformation("Starting MarketDataNotification for {symbol} with lower bound: {} and upper bound: {}", args[0], lowerBound, upperBound);

        await MarketDataMonitor(args[0], lowerBound, upperBound);
    }

    static async Task MarketDataMonitor(string symbol, decimal lowerBound, decimal upperBound) {
        var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(marketDataApiConfig.PollingInterval));
        do {
            try {
                var httpClient = new HttpClient() { BaseAddress = marketDataApiConfig.BaseAddress };
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={marketDataApiConfig.ApiKey}");
                var httpResponseMessage = await httpClient.SendAsync(httpRequest);

                if (httpResponseMessage.IsSuccessStatusCode) {
                    var result = await httpResponseMessage.Content.ReadFromJsonAsync<GlobalQuoteResponse>();

                    if (result.GlobalQuote.Price > upperBound) {
                        await SendMail($"Upper bound reached for {symbol}", $"The upper limit of {upperBound} for {symbol} was reached. Current price is {result.GlobalQuote.Price}. Asset sale is recomended");
                    }
                    else if (result.GlobalQuote.Price < lowerBound) {
                        await SendMail($"Lower bound reached for {symbol}", $"The lower limit of {lowerBound} for {symbol} was reached. Current price is {result.GlobalQuote.Price}. Asset purchase is recomended");
                    }
                    else {
                        logger.LogInformation("Last Price was {price}, no bound reached", result.GlobalQuote.Price);
                    }
                }
                else {
                    logger.LogError("Market Data Api returned: {statusCode} with {content}", httpResponseMessage.StatusCode, await httpResponseMessage.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex) {
                logger.LogError(ex, "Error polling MarketData for {symbol}", symbol);
            }
        } while (await periodicTimer.WaitForNextTickAsync());
    }

    static async Task SendMail(string subject, string message) {
        try {
            MailMessage mail = new MailMessage() {
                From = new MailAddress(emailConfig.UsernameEmail)
            };

            emailConfig.ToEmail.ForEach(item => mail.To.Add(new MailAddress(item)));
            emailConfig.CcEmail.ForEach(item => mail.CC.Add(new MailAddress(item)));

            mail.Subject = subject;
            mail.Body = message;
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.High;

            using (SmtpClient smtp = new SmtpClient(emailConfig.Domain, emailConfig.Port)) {
                smtp.Credentials = new NetworkCredential(emailConfig.UsernameEmail, emailConfig.UsernamePassword);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(mail);
            }
            logger.LogInformation("Email with Subject \"{subject}\" and message \"{message}\" for {destination} sent", subject, message, emailConfig.ToEmail);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error sending mail with {subject}", subject);
        }
    }
}