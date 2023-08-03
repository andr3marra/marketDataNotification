using Microsoft.AspNetCore.Mvc;

namespace marketDataNotificationService.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class SubscriptionController : ControllerBase {
        private readonly ILogger<SubscriptionController> _logger;
        private readonly SubscriptionRepository _subscriptionRepository;

        public SubscriptionController(ILogger<SubscriptionController> logger, SubscriptionRepository subscriptionRepository) {
            _logger = logger;
            _subscriptionRepository = subscriptionRepository;
        }

        [HttpGet("GetAll")]
        public IEnumerable<Subscription> GetAll() {
            return _subscriptionRepository.Subscriptions;
        }

        [HttpPost(Name = "Create")]
        public ActionResult<Guid> Post(Subscription subscription) {
            if (_subscriptionRepository.TryAddSubscription(subscription, out var subscriptionId)) {
                return Ok(subscriptionId);
            }
            return BadRequest();
        }

        [HttpGet(Name = "GetById")]
        public ActionResult<Subscription> Get(Guid subscriptionId) {
            if (_subscriptionRepository.SubscriptionById.TryGetValue(subscriptionId, out var subscription)) {
                return Ok(subscription);
            }
            return NotFound();
        }

        [HttpDelete(Name = "Delete")]
        public IActionResult Delete(Guid subscriptionId) {
            if (_subscriptionRepository.RemoveSubscription(subscriptionId)) {
                return Ok();
            }
            return NotFound();
        }
    }
}