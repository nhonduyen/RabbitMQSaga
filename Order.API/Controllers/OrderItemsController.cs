using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Order.API.Infrastructure.Config;
using Order.API.Infrastructure.Data;
using Order.API.Models;
using Plain.RabbitMQ;
using Shared.Models;

namespace Order.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrderItemsController : ControllerBase
    {
        private readonly OrderingContext _context;
        private readonly IPublisher _publisher;
        private readonly IConfiguration _config;
        private readonly ILogger<OrderItemsController> _logger;

        public OrderItemsController(OrderingContext context, IPublisher publisher, IConfiguration config, ILogger<OrderItemsController> logger)
        {
            _context = context;
            _publisher = publisher;
            _config = config;
            _logger = logger;   
        }

        // GET: api/OrderItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderItems()
        {
            var orderItems = await _context.OrderItems.ToListAsync();
            return Ok(orderItems);
        }

        // GET: api/OrderItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderItem>> GetOrderItem(int id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);

            if (orderItem == null)
            {
                return NotFound();
            }

            return orderItem;
        }


        // POST: api/OrderItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task PostOrderItem(OrderItem orderItem)
        {
            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();

            // New inserted identity value
            var id = orderItem.Id;

            var rabbitMQSettings = _config.GetSection("RabbitMQ").Get<RabbitMQSettings>();
            
            _publisher.Publish(JsonConvert.SerializeObject(new OrderRequest
            {
                OrderId = orderItem.OrderId,
                CatalogId = orderItem.ProductId,
                Units = orderItem.Units,
                Name = orderItem.ProductName
            }),
            rabbitMQSettings.OrderCreatedRoutingkey, // Routing key
            null);
            _logger.LogInformation($"Send to message queue {rabbitMQSettings.OrderCreatedRoutingkey}");
        }
    }
}
