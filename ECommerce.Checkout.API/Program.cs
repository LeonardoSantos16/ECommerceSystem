using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb");
var mongoClient = new MongoClient(mongoConnectionString);
var database = mongoClient.GetDatabase("checkoutdb");


builder.Services.AddSingleton(database);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/api/checkout/orders", async (Order order, IMongoDatabase db) =>
{
    var collection = db.GetCollection<Order>("orders");
    await collection.InsertOneAsync(order);
    return Results.Accepted($"/api/checkout/orders/{order.Id}", order);
});

app.Run();

public record Order(string Id, string CustomerId, decimal TotalAmount);