using AutoMapper;
using CarRent.Application.Abstractions;
using CarRent.Application.Auth.Commands.Register;
using CarRent.Application.DTOs.Auth;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CarRent.Application.Auth.Commands.Register;

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, AuthResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RegisterCommandHandler(
        UserManager<User> userManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AuthResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        User? existingUser = await _userManager.FindByEmailAsync(command.Request.Email);
        if (existingUser is not null)
            throw new ConflictException($"User with email '{command.Request.Email}' already exists.");

        User user = new()
        {
            Email = command.Request.Email,
            UserName = command.Request.Email,
            FullName = command.Request.FullName,
            DriverLicenseNumber = command.Request.DriverLicenseNumber
        };

        IdentityResult result = await _userManager.CreateAsync(user, command.Request.Password);
        if (!result.Succeeded)
        {
            string errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new ValidationException(errors);
        }

        await _userManager.AddToRoleAsync(user, "User");

        IList<string> roles = await _userManager.GetRolesAsync(user);

        string accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        string refreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();

        Domain.Entities.RefreshToken refreshToken = new()
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            ExpiresOnUtc = DateTime.UtcNow.AddDays(30)
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        UserResponse userResponse = _mapper.Map<UserResponse>(user);
        userResponse.Roles = roles.ToList();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            RefreshTokenExpiresOn = refreshToken.ExpiresOnUtc,
            User = userResponse
        };
    }
}
