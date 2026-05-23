using ECommerce.Checkout.API.DTOs;
using ECommerce.Payment.API.Consumers;
using MassTransit;
using Polly;
using Polly.CircuitBreaker;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var circuitBreakerPolicy = Policy.Handle<HttpRequestException>()
    .Or<TimeoutException>()
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 2,
        durationOfBreak: TimeSpan.FromSeconds(30),
        onBreak: (exception, timespan) =>
        {
            Console.WriteLine($"[Polly: Circuit Breaker] CIRCUITO ABERTO por {timespan.TotalSeconds}s devido a: {exception.Message}");
        },
        onReset: () =>
        {
            Console.WriteLine("[Polly: Circuit Breaker] CIRCUITO FECHADO. Operando normalmente.");
        },
        onHalfOpen: () =>
        {
            Console.WriteLine("[Polly: Circuit Breaker] CIRCUITO MEIO-ABERTO. Testando próxima requisição...");
        }
     );

builder.Services.AddSingleton<AsyncCircuitBreakerPolicy>(circuitBreakerPolicy);
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
        rider.AddProducer<PaymentApprovedEvent>("payment-approved-topic");

        rider.UsingKafka((context, k) =>
        {
            k.Host("localhost:9092");
            k.TopicEndpoint<OrderCreatedEvent>("order-created-topic", "payment-service-group", e =>
            {
                e.UseMessageRetry(r =>
                {
                    r.Exponential(3, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(2));
                });
                e.ConfigureConsumer<OrderCreatedConsumer>(context);

                e.AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest;
            });
        });
    });
});

var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

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
