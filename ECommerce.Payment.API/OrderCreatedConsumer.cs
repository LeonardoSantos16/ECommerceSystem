using MassTransit;
using static ECommerce.Checkout.API.DTOs.Contracts;

namespace ECommerce.Payment.API.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;

        Console.WriteLine($"[Payment Service] Processando pagamento para o Pedido: {message.OrderId}");
        Console.WriteLine($"[Payment Service] Valor: {message.TotalAmount:C} | Cliente: {message.CustomerId}");

        await Task.Delay(1000);

        Console.WriteLine($"[Payment Service] Pagamento concluído com sucesso para o Pedido: {message.OrderId}");
    }
}