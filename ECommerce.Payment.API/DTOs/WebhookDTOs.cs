namespace ECommerce.Payment.API.DTOs
{
    public record StripeWebhookPayload
    (
        string EventType,
        StripeData Data
    );

    public record StripeData(
        string OrderId,
        string CustomerId,
        decimal Amount,
        string Status);
}
