using System.Text.Json;
using CarRent.Application.Abstractions;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CarRent.Application.Payments.Commands.ProcessWebhook;

public class ProcessPayPalWebhookCommandHandler : ICommandHandler<ProcessPayPalWebhookCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IPaymentConfirmationService _confirmation;
    private readonly ILogger<ProcessPayPalWebhookCommandHandler> _logger;

    public ProcessPayPalWebhookCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentGateway paymentGateway,
        IPaymentConfirmationService confirmation,
        ILogger<ProcessPayPalWebhookCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _paymentGateway = paymentGateway;
        _confirmation = confirmation;
        _logger = logger;
    }

    public async Task<Unit> Handle(ProcessPayPalWebhookCommand command, CancellationToken cancellationToken)
    {
        bool isValid = await _paymentGateway.VerifyWebhookAsync(command.Headers, command.RawBody, cancellationToken);
        if (!isValid)
            throw new UnauthorizedException("Invalid webhook signature.");

        using JsonDocument doc = JsonDocument.Parse(command.RawBody);
        JsonElement root = doc.RootElement;

        string? eventType = root.GetProperty("event_type").GetString();
        JsonElement resource = root.GetProperty("resource");

        string? orderId = eventType switch
        {
            "CHECKOUT.ORDER.APPROVED" => resource.GetProperty("id").GetString(),
            "PAYMENT.CAPTURE.COMPLETED" => resource
                .GetProperty("supplementary_data")
                .GetProperty("related_ids")
                .GetProperty("order_id")
                .GetString(),
            _ => null
        };

        if (orderId is null)
        {
            _logger.LogInformation("Ignoring PayPal webhook event {EventType}.", eventType);
            return Unit.Value;
        }

        Payment? payment = await _unitOfWork.Payments.GetByTransactionIdAsync(orderId, cancellationToken);
        if (payment is null)
        {
            _logger.LogWarning("Webhook {EventType}: payment for order {OrderId} not found.", eventType, orderId);
            return Unit.Value;
        }

        if (payment.Status != PaymentStatus.Pending)
            return Unit.Value;

        try
        {
            if (payment.Booking.Status == BookingStatus.Pending)
            {
                await _confirmation.ConfirmAsync(payment, cancellationToken);
                _logger.LogInformation("Webhook {EventType}: order {OrderId} confirmed.", eventType, orderId);
            }
            else if (eventType == "PAYMENT.CAPTURE.COMPLETED")
            {
                await _paymentGateway.RefundOrderAsync(orderId, cancellationToken);
                payment.Status = PaymentStatus.Refunded;
                _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogWarning("Webhook: order {OrderId} captured for a non-pending booking, refunded.", orderId);
            }
            else
            {
                _logger.LogWarning(
                    "Webhook {EventType}: booking for order {OrderId} is {Status}, capture skipped.",
                    eventType, orderId, payment.Booking.Status);
            }
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Webhook {EventType} for order {OrderId} not processed: {Message}",
                eventType, orderId, ex.Message);
        }

        return Unit.Value;
    }
}