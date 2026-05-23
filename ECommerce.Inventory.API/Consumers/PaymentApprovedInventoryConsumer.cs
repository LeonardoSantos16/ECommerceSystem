using ECommerce.Inventory.API.Contracts;
using MassTransit;

namespace ECommerce.Inventory.API.Consumers
{
    public class PaymentApprovedInventoryConsumer : IConsumer<PaymentApprovedEvent>
    {
        private readonly ILogger<PaymentApprovedInventoryConsumer> _logger;

        public PaymentApprovedInventoryConsumer(ILogger<PaymentApprovedInventoryConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentApprovedEvent> context)
        {
            var message = context.Message;

            _logger.LogInformation("📦 [Inventory Service] Iniciando baixa de stock para o pedido {OrderId}", message.OrderId);
            _logger.LogInformation("✅ [Inventory Service] Stock atualizado com sucesso para o pedido {OrderId}!", message.OrderId);

            await Task.Delay(300);
        }
    }
}
