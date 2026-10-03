using CarRent.Application.Abstractions;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using MediatR;

namespace CarRent.Application.Bookings.Commands.CancelBooking;

public class CancelBookingCommandHandler : ICommandHandler<CancelBookingCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelBookingCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CancelBookingCommand command, CancellationToken cancellationToken)
    {
        Booking? booking = await _unitOfWork.Bookings.GetByIdAsync(command.BookingId, cancellationToken);
        if (booking == null)
            throw new NotFoundException($"Booking with id {command.BookingId} not found.");

        if (!command.IsAdmin && booking.UserId != command.UserId)
            throw new ValidationException("You do not have permission to cancel this booking.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new ValidationException("Booking is already cancelled.");

        booking.Status = BookingStatus.Cancelled;

        _unitOfWork.Bookings.Update(booking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}