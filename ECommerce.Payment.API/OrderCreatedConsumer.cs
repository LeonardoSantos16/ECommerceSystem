using ECommerce.Checkout.API.DTOs;
using MassTransit;
using Polly.CircuitBreaker;
using StackExchange.Redis;

namespace ECommerce.Payment.API.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IDatabase _redis;
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly ITopicProducer<PaymentApprovedEvent> _paymentApprovedProducer;
    private readonly AsyncCircuitBreakerPolicy _circuitBreaker;

    public OrderCreatedConsumer(IConnectionMultiplexer redis, ILogger<OrderCreatedConsumer> logger, ITopicProducer<PaymentApprovedEvent> paymentApprovedProducer, AsyncCircuitBreakerPolicy circuitBreaker)
    {
        _redis = redis.GetDatabase();
        _logger = logger;
        _paymentApprovedProducer = paymentApprovedProducer;
        _circuitBreaker = circuitBreaker;
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
            await _circuitBreaker.ExecuteAsync(async () =>
            {
                _logger.LogInformation("Iniciando pagamento do pedido {OrderId} no valor de {Value:C}", orderId, context.Message.TotalAmount);

                await Task.Delay(500);

                _logger.LogInformation($"Pagamento confirmado com sucesso para o pedido {orderId}");

            });
            await _paymentApprovedProducer.Produce(new PaymentApprovedEvent(
                context.Message.OrderId,
                context.Message.CustomerId,
                DateTime.UtcNow));
        }
        catch (BrokenCircuitException)
        {
            await _redis.KeyDeleteAsync(lockKey);
            _logger.LogError("Pagamento rejeitado imediatamente: O circuito externo está ABERTO (Stripe fora do ar). ID: {OrderId}", orderId);
            throw;
        }
        catch (Exception ex)
        {
            await _redis.KeyDeleteAsync(lockKey);
            _logger.LogError(ex, "Erro ao processar pagamento do pedido {OrderId}. Chave de idempotência removida.", orderId);
            throw;
        }
    }
}