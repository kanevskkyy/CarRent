using CarRent.Application.Abstractions;

namespace CarRent.Application.Bookings.Commands.CancelBooking;

public record CancelBookingCommand(
    Guid BookingId,
    Guid UserId,
    bool IsAdmin) : ICommand;