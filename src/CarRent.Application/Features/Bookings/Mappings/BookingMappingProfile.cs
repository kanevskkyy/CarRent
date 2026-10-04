using AutoMapper;
using CarRent.Application.DTOs.Bookings;
using CarRent.Domain.Entities;

namespace CarRent.Application.Mappings;

public class BookingMappingProfile : Profile
{
    public BookingMappingProfile()
    {
        CreateMap<Booking, BookingListResponse>()
            .ForMember(dest => dest.Car, opt => opt.MapFrom(src => src.Car))
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.Payment != null ? (Domain.Enums.PaymentStatus?)src.Payment.Status : null));
        
        CreateMap<KeyValuePair<string, int>, DailyBookingCountResponse>()
            .ForMember(dest => dest.Day, opt => opt.MapFrom(src => src.Key))
            .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Value));

        CreateMap<CreateBookingRequest, Booking>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Car, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.TotalPrice, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Payment, opt => opt.Ignore());
    }
}