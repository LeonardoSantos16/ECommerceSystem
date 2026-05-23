using MassTransit;
using MongoDB.Driver;
using static ECommerce.Checkout.API.DTOs.Contracts;

namespace ECommerce.Checkout.API.Consurmers
{
    public class PaymentApprovedConsumer : IConsumer<PaymentApprovedEvent>
    {
        private readonly IMongoDatabase _db;
        private readonly ILogger<PaymentApprovedConsumer> _logger;

        public PaymentApprovedConsumer(IMongoDatabase db, ILogger<PaymentApprovedConsumer> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentApprovedEvent> context)
        {
            var orderId = context.Message.OrderId;
            var collection = _db.GetCollection<Order>("orders");
            var filter = Builders<Order>.Filter.Eq(o => o.Id, orderId);
            var update = Builders<Order>.Update.Set(o => o.Status, "Approved");

            var result = await collection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0)
            {
                _logger.LogInformation("Pedido {OrderId} atualizado para o status: APPROVED.", orderId);
            }
            else
            {
                _logger.LogWarning("Pedido {OrderId} não foi encontrado para atualização de status.", orderId);
            }
        }

    }
}
