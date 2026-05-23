namespace ECommerce.Inventory.API.Contracts
{
    public record PaymentApprovedEvent(
         string OrderId,
         string CustomerId,
         DateTime ApprovedAt
    );
}