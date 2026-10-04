using AutoMapper;
using CarRent.Application.DTOs.Auth;
using CarRent.Domain.Entities;

namespace CarRent.Application.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserResponse>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore()); // Roles are loaded separately from UserManager
    }
}
