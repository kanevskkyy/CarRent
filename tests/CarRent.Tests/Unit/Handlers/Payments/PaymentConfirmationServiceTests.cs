using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Application.Services;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using CarRent.Tests.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Handlers.Payments;

public class PaymentConfirmationServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPaymentGateway> _paymentGatewayMock;
    private readonly PaymentConfirmationService _service;

    public PaymentConfirmationServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _paymentGatewayMock = new Mock<IPaymentGateway>();
        _service = new PaymentConfirmationService(_unitOfWorkMock.Object, _paymentGatewayMock.Object);
    }

    [Fact]
    public async Task ConfirmAsync_WhenPaymentAlreadySucceeded_DoesNothing()
    {
        // Arrange
        Payment payment = new()
        {
            Status = PaymentStatus.Succeeded,
            TransactionId = "txn-1"
        };

        // Act
        await _service.ConfirmAsync(payment, CancellationToken.None);

        // Assert
        _paymentGatewayMock.Verify(g => g.CaptureOrderAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmAsync_WhenBookingNotPending_ThrowsValidationException()
    {
        // Arrange
        Booking booking = TestHelper.CreateTestBooking(status: BookingStatus.Cancelled);
        Payment payment = new()
        {
            Status = PaymentStatus.Pending,
            TransactionId = "txn-2",
            Booking = booking
        };

        // Act
        Func<Task> act = async () => await _service.ConfirmAsync(payment, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Booking is no longer pending*");
        _paymentGatewayMock.Verify(g => g.CaptureOrderAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmAsync_WhenBookingPending_CapturesOrderAndConfirmsBooking()
    {
        // Arrange
        Booking booking = TestHelper.CreateTestBooking(status: BookingStatus.Pending);
        Payment payment = new()
        {
            Status = PaymentStatus.Pending,
            TransactionId = "txn-success",
            Booking = booking
        };

        // Act
        await _service.ConfirmAsync(payment, CancellationToken.None);

        // Assert
        _paymentGatewayMock.Verify(g => g.CaptureOrderAsync("txn-success", It.IsAny<CancellationToken>()), Times.Once);
        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.PaidAt.Should().NotBeNull();
        booking.Status.Should().Be(BookingStatus.Confirmed);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
