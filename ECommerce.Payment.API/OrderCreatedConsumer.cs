using MassTransit;
using StackExchange.Redis;
using static ECommerce.Checkout.API.DTOs.Contracts;

namespace ECommerce.Payment.API.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IDatabase _redis;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(IConnectionMultiplexer redis, ILogger<OrderCreatedConsumer> logger)
    {
        _redis = redis.GetDatabase();
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var orderId = context.Message.OrderId;
        var lockKey = $"payment_processed:{orderId}";
        bool isNewOrder = await _redis.StringSetAsync(lockKey, "processed", TimeSpan.FromDays(1), When.NotExists);

        if (!isNewOrder)
        {
            _logger.LogWarning($"O pedido {orderId} já foi processado anteriormente e context {context.Message}");
            return;
        }

        try
        {
            _logger.LogInformation("Iniciando pagamento do pedido {OrderId} no valor de {Value:C}", orderId, context.Message.TotalAmount);

            await Task.Delay(500);

            _logger.LogInformation($"Pagamento confirmado com sucesso para o pedido {orderId}");
        } catch (Exception ex)
        {
            await _redis.KeyDeleteAsync(lockKey);
            _logger.LogError(ex, "Erro ao processar pagamento do pedido {OrderId}. Chave de idempotência removida.", orderId);
            throw;
        }
    }
}