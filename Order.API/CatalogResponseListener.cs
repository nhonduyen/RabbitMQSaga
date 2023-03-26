using Newtonsoft.Json;
using Order.API.Infrastructure.Data;
using Plain.RabbitMQ;
using Shared.Models;

namespace Order.API
{
    public class CatalogResponseListener : IHostedService
    {
        private ISubscriber _subscriber;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CatalogResponseListener> _logger;
        public CatalogResponseListener(ISubscriber subscripber, IServiceScopeFactory scopeFactory, ILogger<CatalogResponseListener> logger)
        {
            this._subscriber = subscripber;
            this._scopeFactory = scopeFactory;
            _logger = logger;   
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Start listening");
            _subscriber.Subscribe(Subscribe);
            return Task.CompletedTask;
        }

        private bool Subscribe(string message, IDictionary<string, object> header)
        {
            var response = JsonConvert.DeserializeObject<CatalogResponse>(message);
            _logger.LogInformation($"Received message {message}");
            if (!response.IsSuccess)
            {
                _logger.LogInformation($"Response status {response.IsSuccess}");
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _orderingContext = scope.ServiceProvider.GetRequiredService<OrderingContext>();

                    // If transaction is not successful, Remove ordering item
                    var orderItem = _orderingContext.OrderItems.Where(o => o.ProductId == response.CatalogId && o.OrderId == response.OrderId).FirstOrDefault();
                    _orderingContext.OrderItems.Remove(orderItem);
                    _orderingContext.SaveChanges();
                }
            }
            return true;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
