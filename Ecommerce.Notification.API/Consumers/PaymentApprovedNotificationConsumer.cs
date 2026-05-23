using Ecommerce.Notification.API.Contracts;
using MassTransit;

namespace Ecommerce.Notification.API.Consumers
{
    public class PaymentApprovedNotificationConsumer : IConsumer<PaymentApprovedEvent>
    {
        private readonly ILogger<PaymentApprovedNotificationConsumer> _logger;

        public PaymentApprovedNotificationConsumer(ILogger<PaymentApprovedNotificationConsumer> logger)
        {
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<PaymentApprovedEvent> context)
        {
            var message = context.Message;

            _logger.LogInformation("==================================================");
            _logger.LogInformation("📬 [Notification Service] Enviando e-mail para o cliente {CustomerId}", message.CustomerId);
            _logger.LogInformation("📢 Olá! O seu pedido {OrderId} foi pago com sucesso em {Date}!", message.OrderId, message.ApprovedAt);
            _logger.LogInformation("==================================================");

            await Task.Delay(500);
        }
    }
}
