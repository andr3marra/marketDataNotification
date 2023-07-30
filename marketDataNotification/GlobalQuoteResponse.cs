using System.Text.Json.Serialization;

namespace marketDataNotification;
internal partial class Program {
    public class GlobalQuoteResponse {
        [JsonPropertyName("Global Quote")]
        public GlobalQuote GlobalQuote { get; set; }
    }
}