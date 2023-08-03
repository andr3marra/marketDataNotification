using System.Text.Json.Serialization;

namespace marketDataNotificationService.Models {
    public class GlobalQuoteResponse {
        [JsonPropertyName("Global Quote")]
        public GlobalQuote GlobalQuote { get; set; }
    }
}
