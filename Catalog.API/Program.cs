using Catalog.API;
using Catalog.API.Infrastructure.Config;
using Catalog.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Plain.RabbitMQ;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;
var rabbitMQSettings = configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CatalogContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("CatalogConnection"), providerOptions => providerOptions.CommandTimeout(120));
});

builder.Services.AddSingleton<IConnectionProvider>(new ConnectionProvider(rabbitMQSettings.Url));
builder.Services.AddSingleton<IPublisher>(p => new Publisher(p.GetService<IConnectionProvider>(),
                rabbitMQSettings.CatalogExchange, // exchange name
                ExchangeType.Topic));

builder.Services.AddSingleton<ISubscriber>(s => new Subscriber(s.GetService<IConnectionProvider>(),
    rabbitMQSettings.OrderExchange, // Exchange name
    rabbitMQSettings.OrderResponseQueue, //queue name
    rabbitMQSettings.OrderCreatedRoutingkey, // routing key
    ExchangeType.Topic));

builder.Services.AddHostedService<OrderCreatedListener>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
