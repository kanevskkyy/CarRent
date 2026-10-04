using CarRent.Application.DTOs.Payments;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Application.Models.Payments;
using CarRent.Application.Payments.Commands.CreatePayment;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using CarRent.Tests.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace CarRent.Tests.Unit.Handlers.Payments;

public class CreatePaymentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBookingRepository> _bookingRepoMock;
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly Mock<IPaymentGateway> _paymentGatewayMock;
    private readonly CreatePaymentCommandHandler _handler;

    public CreatePaymentCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _bookingRepoMock = new Mock<IBookingRepository>();
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _paymentGatewayMock = new Mock<IPaymentGateway>();

        _unitOfWorkMock.Setup(u => u.Bookings).Returns(_bookingRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Payments).Returns(_paymentRepoMock.Object);

        _handler = new CreatePaymentCommandHandler(_unitOfWorkMock.Object, _paymentGatewayMock.Object);
    }

    [Fact]
    public async Task Handle_WhenBookingNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid bookingId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        CreatePaymentCommand command = new(bookingId, userId);

        _bookingRepoMock.Setup(b => b.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Booking with id {bookingId} not found.");
    }

    [Fact]
    public async Task Handle_WhenBookingBelongsToAnotherUser_ThrowsNotFoundException()
    {
        // Arrange
        Guid bookingOwnerId = Guid.NewGuid();
        Guid differentUserId = Guid.NewGuid();
        Booking booking = TestHelper.CreateTestBooking(userId: bookingOwnerId);
        CreatePaymentCommand command = new(booking.Id, differentUserId);

        _bookingRepoMock.Setup(b => b.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Booking with id {booking.Id} not found.");
    }

    [Fact]
    public async Task Handle_WhenBookingNotPending_ThrowsValidationException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Booking booking = TestHelper.CreateTestBooking(userId: userId, status: BookingStatus.Cancelled);
        CreatePaymentCommand command = new(booking.Id, userId);

        _bookingRepoMock.Setup(b => b.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Only pending bookings can be paid*");
    }

    [Fact]
    public async Task Handle_WhenPaymentAlreadySucceeded_ThrowsConflictException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Booking booking = TestHelper.CreateTestBooking(userId: userId, status: BookingStatus.Pending);
        CreatePaymentCommand command = new(booking.Id, userId);
        Payment existingPayment = new() { BookingId = booking.Id, Status = PaymentStatus.Succeeded };

        _bookingRepoMock.Setup(b => b.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _paymentRepoMock.Setup(p => p.GetByBookingIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("This booking is already paid.");
    }

    [Fact]
    public async Task Handle_WhenNoExistingPayment_CreatesOrderAndSavesNewPayment()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Booking booking = TestHelper.CreateTestBooking(userId: userId, status: BookingStatus.Pending, totalPrice: 250m);
        CreatePaymentCommand command = new(booking.Id, userId);

        PaymentOrder order = new("paypal-order-1", "https://paypal.com/approve/1");
        _bookingRepoMock.Setup(b => b.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _paymentRepoMock.Setup(p => p.GetByBookingIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);
        _paymentGatewayMock.Setup(g => g.CreateOrderAsync(booking.TotalPrice, It.IsAny<string>(), booking.Id.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        Payment? addedPayment = null;
        _paymentRepoMock.Setup(p => p.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Callback<Payment, CancellationToken>((p, _) => addedPayment = p)
            .Returns(Task.CompletedTask);

        // Act
        CreatePaymentResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be(order.OrderId);
        result.ApprovalUrl.Should().Be(order.ApprovalUrl);
        result.BookingId.Should().Be(booking.Id);

        addedPayment.Should().NotBeNull();
        addedPayment!.TransactionId.Should().Be(order.OrderId);
        addedPayment.Amount.Should().Be(booking.TotalPrice);
        addedPayment.Status.Should().Be(PaymentStatus.Pending);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenExistingPendingPayment_UpdatesTransactionId()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Booking booking = TestHelper.CreateTestBooking(userId: userId, status: BookingStatus.Pending, totalPrice: 300m);
        CreatePaymentCommand command = new(booking.Id, userId);
        Payment existingPayment = new()
        {
            BookingId = booking.Id,
            TransactionId = "old-order",
            Amount = 300m,
            Status = PaymentStatus.Pending
        };

        PaymentOrder newOrder = new("new-paypal-order", "https://paypal.com/approve/new");
        _bookingRepoMock.Setup(b => b.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _paymentRepoMock.Setup(p => p.GetByBookingIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);
        _paymentGatewayMock.Setup(g => g.CreateOrderAsync(booking.TotalPrice, It.IsAny<string>(), booking.Id.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newOrder);

        // Act
        CreatePaymentResponse result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.OrderId.Should().Be("new-paypal-order");
        existingPayment.TransactionId.Should().Be("new-paypal-order");
        _paymentRepoMock.Verify(p => p.Update(existingPayment), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
