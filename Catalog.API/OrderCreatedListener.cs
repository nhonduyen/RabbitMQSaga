using Catalog.API.Infrastructure.Config;
using Catalog.API.Infrastructure.Data;
using Catalog.API.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Plain.RabbitMQ;
using Shared.Models;

namespace Catalog.API
{
    public class OrderCreatedListener : IHostedService
    {
        private readonly ISubscriber _subscribe;
        private readonly IPublisher _publisher;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<OrderCreatedListener> _logger;
        public OrderCreatedListener(ISubscriber subscriber, IPublisher publisher, IServiceScopeFactory scopeFactory, IConfiguration config, ILogger<OrderCreatedListener> logger)
        {
            _subscribe = subscriber;
            _publisher = publisher;
            _scopeFactory = scopeFactory;
            _config = config;
            _logger = logger;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Start listening");
            _subscribe.Subscribe(Subscribe);
            return Task.CompletedTask;
        }

        private bool Subscribe(string message, IDictionary<string, object> header)
        {
            var response = JsonConvert.DeserializeObject<OrderRequest>(message);

            _logger.LogInformation($"Received message {message}");

            using (var scope = _scopeFactory.CreateScope())
            {
                var rabbitMQSettings = _config.GetSection("RabbitMQ").Get<RabbitMQSettings>();
                var _context = scope.ServiceProvider.GetRequiredService<CatalogContext>();
                try
                {
                    CatalogItem catalogItem = _context.CatalogItems.Find(response.CatalogId);

                    if (catalogItem == null || catalogItem.AvailableStock < response.Units)
                        throw new Exception();

                    catalogItem.AvailableStock = catalogItem.AvailableStock - response.Units;
                    _context.Entry(catalogItem).State = EntityState.Modified;
                    _context.SaveChanges();

                    _publisher.Publish(JsonConvert.SerializeObject(
                            new CatalogResponse { OrderId = response.OrderId, CatalogId = response.CatalogId, IsSuccess = true }
                        ), rabbitMQSettings.CatalogResponseRoutingkey, null);
                    _logger.LogInformation($"Send to message queue {rabbitMQSettings.CatalogResponseRoutingkey}");
                }
                catch (Exception)
                {
                    _publisher.Publish(JsonConvert.SerializeObject(
                    new CatalogResponse { OrderId = response.OrderId, CatalogId = response.CatalogId, IsSuccess = false }
                ), rabbitMQSettings.CatalogResponseRoutingkey, null);
                    _logger.LogInformation($"Error: Send to message queue {rabbitMQSettings.CatalogResponseRoutingkey}");
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
