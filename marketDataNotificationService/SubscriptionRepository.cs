using System.Diagnostics.CodeAnalysis;

namespace marketDataNotificationService {
    public class SubscriptionRepository {
        public IEnumerable<Subscription> Subscriptions => subscribersByTicker.SelectMany(x => x.Value);
        public IReadOnlyDictionary<string, List<Subscription>> SubscribersByTicker => subscribersByTicker;
        public IReadOnlyDictionary<Guid, Subscription> SubscriptionById => subscriptionById;

        private Dictionary<string, List<Subscription>> subscribersByTicker = new Dictionary<string, List<Subscription>>();
        private Dictionary<Guid, Subscription> subscriptionById = new Dictionary<Guid, Subscription>();

        public bool TryAddSubscription(Subscription subscription, [MaybeNullWhen(false)] out Guid? subscriptionId) {
            if (subscribersByTicker.TryGetValue(subscription.Ticker, out var subscriptions)) {
                subscriptions.Add(subscription);
                subscriptionId = Guid.NewGuid();
                return subscriptionById.TryAdd(subscriptionId.Value, subscription);
            }
            else {
                if (subscribersByTicker.TryAdd(subscription.Ticker, new List<Subscription>() { subscription })) {
                    subscriptionId = Guid.NewGuid();
                    return subscriptionById.TryAdd(subscriptionId.Value, subscription);
                }
            }
            subscriptionId = null;
            return false;
        }

        public bool GetSubscription(Guid subscriptionId, [NotNullWhen(true)] out Subscription? subscription) {
            return subscriptionById.TryGetValue(subscriptionId, out subscription);
        }

        public bool RemoveSubscription(Guid subscriptionId) {
            if (subscriptionById.Remove(subscriptionId, out var subscription) && subscribersByTicker.TryGetValue(subscription.Ticker, out var subscriptions)) {
                if(subscriptions.Count == 1) {
                    return subscribersByTicker.Remove(subscription.Ticker);
                }
                else {
                    return subscriptions.Remove(subscription);
                }
            }
            return false;
        }
    }

    public record Subscription(string Subscriber, string TriggerName, decimal? LowerBound, decimal? UpperBound, string Ticker);
}
