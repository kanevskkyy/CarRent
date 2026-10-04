using CarRent.Application.Bookings.Commands.CancelBooking;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using CarRent.Tests.Common;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Handlers.Bookings;

public class CancelBookingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBookingRepository> _bookingRepoMock;
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly Mock<IPaymentGateway> _paymentGatewayMock;
    private readonly CancelBookingCommandHandler _handler;

    public CancelBookingCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _bookingRepoMock = new Mock<IBookingRepository>();
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _paymentGatewayMock = new Mock<IPaymentGateway>();

        _unitOfWorkMock.Setup(u => u.Bookings).Returns(_bookingRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Payments).Returns(_paymentRepoMock.Object);

        _handler = new CancelBookingCommandHandler(_unitOfWorkMock.Object, _paymentGatewayMock.Object);
    }

    [Fact]
    public async Task Handle_WhenBookingNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid bookingId = Guid.NewGuid();
        CancelBookingCommand command = new(bookingId, Guid.NewGuid(), false);

        _bookingRepoMock.Setup(b => b.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Booking with id {bookingId} not found.");
    }

    [Fact]
    public async Task Handle_WhenNotOwnerAndNotAdmin_ThrowsValidationException()
    {
        // Arrange
        Guid ownerId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        Booking booking = TestHelper.CreateTestBooking(userId: ownerId);
        CancelBookingCommand command = new(booking.Id, otherUserId, false);

        _bookingRepoMock.Setup(b => b.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("You do not have permission to cancel this booking.");
    }

    [Fact]
    public async Task Handle_WhenAlreadyCancelled_ThrowsValidationException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Booking booking = TestHelper.CreateTestBooking(userId: userId, status: BookingStatus.Cancelled);
        CancelBookingCommand command = new(booking.Id, userId, false);

        _bookingRepoMock.Setup(b => b.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Booking is already cancelled.");
    }

    [Fact]
    public async Task Handle_WhenPaymentSucceeded_RefundsPaymentAndCancelsBooking()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Booking booking = TestHelper.CreateTestBooking(userId: userId, status: BookingStatus.Confirmed);
        CancelBookingCommand command = new(booking.Id, userId, false);

        Payment payment = new()
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            TransactionId = "txn-paypal-123",
            Amount = 300m,
            Status = PaymentStatus.Succeeded
        };

        _bookingRepoMock.Setup(b => b.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _paymentRepoMock.Setup(p => p.GetByBookingIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act
        MediatR.Unit result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        _paymentGatewayMock.Verify(g => g.RefundOrderAsync(payment.TransactionId, It.IsAny<CancellationToken>()), Times.Once);
        payment.Status.Should().Be(PaymentStatus.Refunded);
        booking.Status.Should().Be(BookingStatus.Cancelled);
        _bookingRepoMock.Verify(b => b.Update(booking), Times.Once);
        _paymentRepoMock.Verify(p => p.Update(payment), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAdminCancelsBookingWithoutPayment_CancelsBookingSuccessfully()
    {
        // Arrange
        Guid ownerId = Guid.NewGuid();
        Guid adminId = Guid.NewGuid();
        Booking booking = TestHelper.CreateTestBooking(userId: ownerId, status: BookingStatus.Pending);
        CancelBookingCommand command = new(booking.Id, adminId, true);

        _bookingRepoMock.Setup(b => b.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _paymentRepoMock.Setup(p => p.GetByBookingIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        // Act
        MediatR.Unit result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        booking.Status.Should().Be(BookingStatus.Cancelled);
        _paymentGatewayMock.Verify(g => g.RefundOrderAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
