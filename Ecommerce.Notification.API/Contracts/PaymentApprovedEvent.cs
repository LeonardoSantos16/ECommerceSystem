namespace Ecommerce.Notification.API.Contracts
{
    public record PaymentApprovedEvent(
         string OrderId,
         string CustomerId,
         DateTime ApprovedAt
    );
}