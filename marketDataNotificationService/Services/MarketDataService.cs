using marketDataNotificationService.Models;
using MediatR;
using Microsoft.Extensions.Options;

namespace marketDataNotificationService.Services {
    public class MarketDataService : BackgroundService {
        private readonly ILogger<MarketDataService> _logger;
        private readonly IOptionsMonitor<MarketDataApiConfig> _marketDataApiConfig;
        private readonly SubscriptionRepository _subscriptionRepository;
        private readonly IMediator _mediator;
        private PeriodicTimer periodicTimer;

        public MarketDataService(ILogger<MarketDataService> logger, IOptionsMonitor<MarketDataApiConfig> marketDataApiConfig, SubscriptionRepository subscriptionRepository, IMediator mediator) {
            _logger = logger;
            _marketDataApiConfig = marketDataApiConfig;
            _subscriptionRepository = subscriptionRepository;
            _mediator = mediator;
            periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(_marketDataApiConfig.CurrentValue.PollingInterval));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            do {
                foreach (var item in _subscriptionRepository.SubscribersByTicker) {
                    try {
                        var httpClient = new HttpClient() { BaseAddress = _marketDataApiConfig.CurrentValue.BaseAddress };
                        var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"query?function=GLOBAL_QUOTE&symbol={item.Key}&apikey={_marketDataApiConfig.CurrentValue.ApiKey}");
                        var httpResponseMessage = await httpClient.SendAsync(httpRequest);

                        if (httpResponseMessage.IsSuccessStatusCode) {
                            var result = await httpResponseMessage.Content.ReadFromJsonAsync<GlobalQuoteResponse>();
                            if (result == null || result.GlobalQuote == null) {
                                _logger.LogError("Unable to desserialize {response}", await httpResponseMessage.Content.ReadAsStringAsync());
                                continue;
                            }
                            await _mediator.Publish(result.GlobalQuote);
                        }
                        else {
                            _logger.LogError("Market Data Api returned: {statusCode} with {content}", httpResponseMessage.StatusCode, await httpResponseMessage.Content.ReadAsStringAsync());
                        }
                    }
                    catch (Exception ex) {
                        _logger.LogError(ex, "Error polling MarketData for {symbol}", item.Key);
                    }
                }
            } while (await periodicTimer.WaitForNextTickAsync());
        }
    }
}
