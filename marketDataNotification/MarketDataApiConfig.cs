namespace marketDataNotification;

public class MarketDataApiConfig {
    public double PollingInterval { get; set; }
    public Uri BaseAddress { get; set; }
    public string ApiKey { get; set; }
}
