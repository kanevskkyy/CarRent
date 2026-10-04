using AutoMapper;
using CarRent.Application.Abstractions.Queries;
using CarRent.Application.DTOs.Bookings;
using CarRent.Application.Interfaces.Repositories;

namespace CarRent.Application.Bookings.Queries.GetDailyBookingStats;

public class GetDailyBookingStatsQueryHandler : IQueryHandler<GetDailyBookingStatsQuery, List<DailyBookingCountResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDailyBookingStatsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<DailyBookingCountResponse>> Handle(GetDailyBookingStatsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<string, int> statsDict = await _unitOfWork.Bookings.GetDailyBookingCountsForLast48HoursAsync(cancellationToken);

        return _mapper.Map<List<DailyBookingCountResponse>>(statsDict);
    }
}