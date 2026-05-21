using ECommerce.Payment.API.Consumers;
using MassTransit;
using static ECommerce.Checkout.API.DTOs.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });

    x.AddRider(rider =>
    {
        rider.AddConsumer<OrderCreatedConsumer>();

        rider.UsingKafka((context, k) =>
        {
            k.Host("localhost:9092");

            k.TopicEndpoint<OrderCreatedEvent>("order-created-topic", "payment-service-group", e =>
            {
                e.ConfigureConsumer<OrderCreatedConsumer>(context);

                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
            });
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
