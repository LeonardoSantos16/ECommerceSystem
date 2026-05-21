using MassTransit;
using MongoDB.Driver;
using static ECommerce.Checkout.API.DTOs.Contracts;

var builder = WebApplication.CreateBuilder(args);

var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb");
var mongoClient = new MongoClient(mongoConnectionString);
var database = mongoClient.GetDatabase("checkoutdb");


builder.Services.AddSingleton(database);

builder.Services.AddOpenApi();

builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });

    x.AddRider(rider =>
    {
        rider.AddProducer<OrderCreatedEvent>("order-created-topic");

        rider.UsingKafka((context, k) =>
        {
            k.Host("localhost:9092"); 
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/api/checkout/orders", async (
    Order order,
    IMongoDatabase db,
    ITopicProducer<OrderCreatedEvent> producer) =>
{
    var collection = db.GetCollection<Order>("orders");

    await collection.InsertOneAsync(order);

    await producer.Produce(new OrderCreatedEvent(
        order.Id,
        order.CustomerId,
        order.TotalAmount,
        DateTime.UtcNow
    ));

    return Results.Accepted($"/api/checkout/orders/{order.Id}", order);
});

app.Run();

