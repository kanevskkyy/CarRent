using CarRent.Application.Abstractions;
using CarRent.Application.DTOs.Bookings;

namespace CarRent.Application.Bookings.Commands.CreateBooking;

public record CreateBookingCommand(
    Guid UserId, 
    CreateBookingRequest Request
) : ICommand<BookingListResponse>;