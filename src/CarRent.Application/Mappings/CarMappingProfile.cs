using AutoMapper;
using CarRent.Application.DTOs.Bookings;
using CarRent.Application.DTOs.Cars;
using CarRent.Domain.Entities;

namespace CarRent.Application.Mappings;

public class CarMappingProfile : Profile
{
    public CarMappingProfile()
    {
        CreateMap<Car, CarListResponse>();

        CreateMap<Car, CarDetailResponse>()
            .ForMember(dest => dest.Bookings, opt => opt.MapFrom(src => src.Bookings))
            .ForMember(dest => dest.IsDriverLicenseVerified, opt => opt.Ignore()); 

        CreateMap<Booking, CarBookingResponse>();
        
        CreateMap<Car, BookingCarResponse>();

        CreateMap<CreateCarRequest, Car>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PhotoUrl, opt => opt.Ignore()) 
            .ForMember(dest => dest.Bookings, opt => opt.Ignore());

        CreateMap<UpdateCarRequest, Car>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PhotoUrl, opt => opt.Ignore())
            .ForMember(dest => dest.Bookings, opt => opt.Ignore());
    }
}