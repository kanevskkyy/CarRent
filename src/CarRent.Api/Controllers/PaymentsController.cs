using CarRent.Application.DTOs.Payments;
using CarRent.Application.Interfaces.Services;
using CarRent.Application.Models.Payments;
using CarRent.Application.Payments.Commands.CreatePayment;
using CarRent.Application.Payments.Commands.ProcessWebhook;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

/// <summary>
/// PayPal payments for bookings.
/// </summary>
[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IUserClaimsService _userClaimsService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        ISender sender,
        IUserClaimsService userClaimsService,
        ILogger<PaymentsController> logger)
    {
        _sender = sender;
        _userClaimsService = userClaimsService;
        _logger = logger;
    }

    /// <summary>
    /// Create a PayPal order for a pending booking.
    /// </summary>
    /// <param name="bookingId">Booking ID (GUID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Order ID and the PayPal URL where the user approves the payment.</returns>
    [HttpPost("bookings/{bookingId:guid}/pay")]
    [Authorize]
    public async Task<ActionResult<CreatePaymentResponse>> Pay(Guid bookingId, CancellationToken cancellationToken)
    {
        Guid userId = _userClaimsService.UserId;

        _logger.LogInformation("Creating payment for Booking ID: {BookingId} by User ID: {UserId}", bookingId, userId);

        CreatePaymentResponse result = await _sender.Send(new CreatePaymentCommand(bookingId, userId), cancellationToken);

        return Ok(result);
    }
    
    /// <summary>
    /// PayPal webhook receiver (called by PayPal servers, not by users).
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        using StreamReader reader = new(Request.Body);
        string rawBody = await reader.ReadToEndAsync(cancellationToken);

        WebhookHeaders headers = new(
            Request.Headers["PAYPAL-AUTH-ALGO"].ToString(),
            Request.Headers["PAYPAL-CERT-URL"].ToString(),
            Request.Headers["PAYPAL-TRANSMISSION-ID"].ToString(),
            Request.Headers["PAYPAL-TRANSMISSION-SIG"].ToString(),
            Request.Headers["PAYPAL-TRANSMISSION-TIME"].ToString());

        await _sender.Send(new ProcessPayPalWebhookCommand(headers, rawBody), cancellationToken);

        return Ok();
    }
}