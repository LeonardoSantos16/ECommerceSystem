namespace ECommerce.Checkout.API.DTOs
{
    public class Contracts
    {
        public record Order(string Id, string CustomerId, decimal TotalAmount);

        public record OrderCreatedEvent(
            string OrderId,
            string CustomerId,
            decimal TotalAmount,
            DateTime CreatedAt
        );
    }
}
