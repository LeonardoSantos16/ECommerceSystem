using ECommerce.Checkout.API.DTOs;
using ECommerce.Payment.API.DTOs;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
namespace ECommerce.Payment.API.Controllers
{
    [ApiController]
    [Route("api/payments/webhooks")]
    public class WebhookController : ControllerBase
    {
        private readonly ITopicProducer<PaymentApprovedEvent> _paymentApprovedProducer;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            ITopicProducer<PaymentApprovedEvent> paymentApprovedProducer,
            ILogger<WebhookController> logger)
        {
            _paymentApprovedProducer = paymentApprovedProducer;
            _logger = logger;
        }

        [HttpPost("stripe")]
        public async Task<IActionResult> ReceiveStripeWebhook([FromBody] StripeWebhookPayload payload)
        {
            _logger.LogInformation("==================================================");
            _logger.LogInformation("📥 Webhook recebido do Stripe! Evento: {EventType}", payload.EventType);
            _logger.LogInformation("==================================================");

            if (payload.EventType == "payment_intent.succeeded" && payload.Data.Status == "succeeded")
            {
                _logger.LogInformation("✅ Pagamento confirmado via Webhook para o Pedido: {OrderId}", payload.Data.OrderId);

                await _paymentApprovedProducer.Produce(new PaymentApprovedEvent(
                    payload.Data.OrderId,
                    payload.Data.CustomerId,
                    DateTime.UtcNow));

                return Ok(new {Message = "Webhook recebido e processado com sucesso" });
            }

            _logger.LogWarning("⚠️ Evento ignorado ou não tratado: {EventType}", payload.EventType);
            return BadRequest("Evento não suportado");
        }
    }
}
