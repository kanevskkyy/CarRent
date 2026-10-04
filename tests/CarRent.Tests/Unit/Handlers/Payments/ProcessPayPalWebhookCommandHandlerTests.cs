using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Application.Models.Payments;
using CarRent.Application.Payments.Commands.ProcessWebhook;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using CarRent.Tests.Common;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Handlers.Payments;

public class ProcessPayPalWebhookCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly Mock<IPaymentGateway> _paymentGatewayMock;
    private readonly Mock<IPaymentConfirmationService> _confirmationMock;
    private readonly Mock<ILogger<ProcessPayPalWebhookCommandHandler>> _loggerMock;
    private readonly ProcessPayPalWebhookCommandHandler _handler;

    public ProcessPayPalWebhookCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _unitOfWorkMock.Setup(u => u.Payments).Returns(_paymentRepoMock.Object);
        _paymentGatewayMock = new Mock<IPaymentGateway>();
        _confirmationMock = new Mock<IPaymentConfirmationService>();
        _loggerMock = new Mock<ILogger<ProcessPayPalWebhookCommandHandler>>();

        _handler = new ProcessPayPalWebhookCommandHandler(
            _unitOfWorkMock.Object,
            _paymentGatewayMock.Object,
            _confirmationMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenWebhookSignatureInvalid_ThrowsUnauthorizedException()
    {
        // Arrange
        WebhookHeaders headers = new("SHA256withRSA", "https://example.test/cert", "transmission-id", "signature", "2026-10-04T00:00:00Z");
        string rawBody = "{}";
        ProcessPayPalWebhookCommand command = new(headers, rawBody);

        _paymentGatewayMock.Setup(g => g.VerifyWebhookAsync(headers, rawBody, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid webhook signature.");
    }

    [Fact]
    public async Task Handle_WhenUnknownEventType_IgnoresEvent()
    {
        // Arrange
        WebhookHeaders headers = new("SHA256withRSA", "https://example.test/cert", "transmission-id", "signature", "2026-10-04T00:00:00Z");
        string rawBody = """{"event_type": "BILLING.SUBSCRIPTION.CREATED", "resource": {}}""";
        ProcessPayPalWebhookCommand command = new(headers, rawBody);

        _paymentGatewayMock.Setup(g => g.VerifyWebhookAsync(headers, rawBody, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        MediatR.Unit result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        _paymentRepoMock.Verify(p => p.GetByTransactionIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPaymentNotFound_LogsAndReturnsUnit()
    {
        // Arrange
        WebhookHeaders headers = new("SHA256withRSA", "https://example.test/cert", "transmission-id", "signature", "2026-10-04T00:00:00Z");
        string rawBody = """
        {
            "event_type": "CHECKOUT.ORDER.APPROVED",
            "resource": { "id": "unknown-order" }
        }
        """;
        ProcessPayPalWebhookCommand command = new(headers, rawBody);

        _paymentGatewayMock.Setup(g => g.VerifyWebhookAsync(headers, rawBody, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _paymentRepoMock.Setup(p => p.GetByTransactionIdAsync("unknown-order", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        // Act
        MediatR.Unit result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        _confirmationMock.Verify(c => c.ConfirmAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPaymentNotPending_ReturnsWithoutConfirming()
    {
        // Arrange
        WebhookHeaders headers = new("SHA256withRSA", "https://example.test/cert", "transmission-id", "signature", "2026-10-04T00:00:00Z");
        string rawBody = """
        {
            "event_type": "CHECKOUT.ORDER.APPROVED",
            "resource": { "id": "already-succeeded" }
        }
        """;
        ProcessPayPalWebhookCommand command = new(headers, rawBody);

        Payment payment = new() { TransactionId = "already-succeeded", Status = PaymentStatus.Succeeded };

        _paymentGatewayMock.Setup(g => g.VerifyWebhookAsync(headers, rawBody, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _paymentRepoMock.Setup(p => p.GetByTransactionIdAsync("already-succeeded", It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act
        MediatR.Unit result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        _confirmationMock.Verify(c => c.ConfirmAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenOrderApprovedAndBookingPending_ConfirmsPayment()
    {
        // Arrange
        WebhookHeaders headers = new("SHA256withRSA", "https://example.test/cert", "transmission-id", "signature", "2026-10-04T00:00:00Z");
        string rawBody = """
        {
            "event_type": "CHECKOUT.ORDER.APPROVED",
            "resource": { "id": "order-to-confirm" }
        }
        """;
        ProcessPayPalWebhookCommand command = new(headers, rawBody);

        Booking booking = TestHelper.CreateTestBooking(status: BookingStatus.Pending);
        Payment payment = new()
        {
            TransactionId = "order-to-confirm",
            Status = PaymentStatus.Pending,
            Booking = booking
        };

        _paymentGatewayMock.Setup(g => g.VerifyWebhookAsync(headers, rawBody, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _paymentRepoMock.Setup(p => p.GetByTransactionIdAsync("order-to-confirm", It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act
        MediatR.Unit result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        _confirmationMock.Verify(c => c.ConfirmAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCaptureCompletedAndBookingCancelled_RefundsOrder()
    {
        // Arrange
        WebhookHeaders headers = new("SHA256withRSA", "https://example.test/cert", "transmission-id", "signature", "2026-10-04T00:00:00Z");
        string rawBody = """
        {
            "event_type": "PAYMENT.CAPTURE.COMPLETED",
            "resource": {
                "supplementary_data": {
                    "related_ids": {
                        "order_id": "order-refund"
                    }
                }
            }
        }
        """;
        ProcessPayPalWebhookCommand command = new(headers, rawBody);

        Booking booking = TestHelper.CreateTestBooking(status: BookingStatus.Cancelled);
        Payment payment = new()
        {
            TransactionId = "order-refund",
            Status = PaymentStatus.Pending,
            Booking = booking
        };

        _paymentGatewayMock.Setup(g => g.VerifyWebhookAsync(headers, rawBody, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _paymentRepoMock.Setup(p => p.GetByTransactionIdAsync("order-refund", It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act
        MediatR.Unit result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        _paymentGatewayMock.Verify(g => g.RefundOrderAsync("order-refund", It.IsAny<CancellationToken>()), Times.Once);
        payment.Status.Should().Be(PaymentStatus.Refunded);
        _paymentRepoMock.Verify(p => p.Update(payment), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
