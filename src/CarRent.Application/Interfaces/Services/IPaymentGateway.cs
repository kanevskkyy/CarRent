using CarRent.Application.Models.Payments;

namespace CarRent.Application.Interfaces.Services;

public interface IPaymentGateway
{
    Task<bool> VerifyWebhookAsync(WebhookHeaders headers, string rawBody, CancellationToken cancellationToken);
    Task<PaymentOrder> CreateOrderAsync(
        decimal amount, string description, string referenceId, CancellationToken cancellationToken);
    Task CaptureOrderAsync(string orderId, CancellationToken cancellationToken);
    Task RefundOrderAsync(string orderId, CancellationToken cancellationToken);
}