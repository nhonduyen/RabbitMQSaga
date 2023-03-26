namespace Order.API.Infrastructure.Config
{
    public class RabbitMQSettings
    {
        public string Url { get; set; }
        public string OrderExchange { get; set; }
        public string CatalogExchange { get; set; }
        public string OrderResponseQueue { get; set; }
        public string OrderCreatedRoutingkey { get; set; }
        public string CatalogResponseQueue { get; set; }
        public string CatalogResponseRoutingkey { get; set; }
    }
}
