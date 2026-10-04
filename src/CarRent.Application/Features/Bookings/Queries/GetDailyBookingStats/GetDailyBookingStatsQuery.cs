using CarRent.Application.Abstractions.Queries;
using CarRent.Application.DTOs.Bookings;

namespace CarRent.Application.Bookings.Queries.GetDailyBookingStats;

public record GetDailyBookingStatsQuery : IQuery<List<DailyBookingCountResponse>>;