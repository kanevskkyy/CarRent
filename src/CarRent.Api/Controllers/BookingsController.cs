using System.Security.Claims;
using CarRent.Application.Bookings.Commands.CancelBooking;
using CarRent.Application.Bookings.Commands.CreateBooking;
using CarRent.Application.Bookings.Queries.GetDailyBookingStats;
using CarRent.Application.Bookings.Queries.GetPagedBookings;
using CarRent.Application.DTOs.Bookings;
using CarRent.Application.Interfaces.Services;
using CarRent.Application.Models.Bookings;
using CarRent.Application.Models.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IUserClaimsService _userClaimsService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(
        ISender sender, 
        IUserClaimsService userClaimsService, 
        ILogger<BookingsController> logger)
    {
        _sender = sender;
        _userClaimsService = userClaimsService;
        _logger = logger;
    }

    /// <summary>
    /// Get paged list of bookings with filters.
    /// Regular users see only their own bookings, while Admins can view all bookings.
    /// </summary>
    /// <param name="filter">Filtering, sorting, and pagination parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of bookings matching the criteria.</returns>
    [HttpGet]
    public async Task<ActionResult<PagedList<BookingListResponse>>> GetPagedBookings(
        [FromQuery] BookingFilterModel filter, 
        CancellationToken cancellationToken)
    {
        Guid userId = _userClaimsService.UserId;
        bool isAdmin = _userClaimsService.IsAdmin;

        _logger.LogInformation("Fetching paged bookings for User ID: {UserId}, IsAdmin: {IsAdmin}", userId, isAdmin);

        GetPagedBookingsQuery query = new GetPagedBookingsQuery(filter, userId, isAdmin);
        PagedList<BookingListResponse> result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Create a new car booking for the currently authenticated user.
    /// </summary>
    /// <param name="request">Booking creation details including car ID and date range.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Details of the newly created booking.</returns>
    [HttpPost]
    public async Task<ActionResult<BookingListResponse>> Create(
        [FromBody] CreateBookingRequest request, 
        CancellationToken cancellationToken)
    {
        Guid userId = _userClaimsService.UserId;

        _logger.LogInformation("Creating booking for Car ID: {CarId} by User ID: {UserId}", request.CarId, userId);

        CreateBookingCommand command = new CreateBookingCommand(userId, request);
        BookingListResponse result = await _sender.Send(command, cancellationToken);

        _logger.LogInformation("Booking created successfully with ID: {Id}", result.Id);
        return Ok(result);
    }

    /// <summary>
    /// Cancel an existing booking by its ID.
    /// Users can cancel their own bookings, while Admins can cancel any booking.
    /// </summary>
    /// <param name="id">Booking ID (GUID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        Guid userId = _userClaimsService.UserId;
        bool isAdmin = _userClaimsService.IsAdmin;

        _logger.LogInformation("Cancelling booking with ID: {Id} by User ID: {UserId}", id, userId);

        CancelBookingCommand command = new CancelBookingCommand(id, userId, isAdmin);
        await _sender.Send(command, cancellationToken);

        _logger.LogInformation("Booking with ID: {Id} successfully cancelled.", id);
        return NoContent();
    }

    /// <summary>
    /// Get daily booking count statistics for the last 48 hours (Admin only).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Daily booking count statistics.</returns>
    [HttpGet("stats/daily")]
    [AllowAnonymous]
    public async Task<ActionResult<List<DailyBookingCountResponse>>> GetDailyBookingStats(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching daily booking stats for the last 48 hours.");

        GetDailyBookingStatsQuery query = new GetDailyBookingStatsQuery();
        List<DailyBookingCountResponse> result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }
}