namespace ECommerce.Checkout.API.DTOs
{
    public class Contracts
    {
        public record Order(string Id, string CustomerId, decimal TotalAmount, string Status = "Pending");

        public record OrderCreatedEvent(
            string OrderId,
            string CustomerId,
            decimal TotalAmount,
            DateTime CreatedAt
        );

        public record PaymentApprovedEvent(string OrderId, string CustomerId, DateTime ApprovedAt);
    }
}
