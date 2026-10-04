using AutoMapper;
using CarRent.Application.Abstractions;
using CarRent.Application.Auth.Commands.RefreshToken;
using CarRent.Application.DTOs.Auth;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CarRent.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RefreshTokenCommandHandler(
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

    public async Task<AuthResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        Domain.Entities.RefreshToken? storedToken = await _unitOfWork.RefreshTokens
            .GetByTokenAsync(command.Request.RefreshToken, cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        User? user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
        if (user is null)
            throw new UnauthorizedException("User associated with token no longer exists.");

        IList<string> roles = await _userManager.GetRolesAsync(user);

        string newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        string newRefreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();

        storedToken.IsRevoked = true;
        storedToken.RevokedOnUtc = DateTime.UtcNow;
        storedToken.ReplacedByToken = newRefreshTokenValue;

        Domain.Entities.RefreshToken newRefreshToken = new()
        {
            Token = newRefreshTokenValue,
            UserId = user.Id,
            ExpiresOnUtc = DateTime.UtcNow.AddDays(30)
        };

        await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        UserResponse userResponse = _mapper.Map<UserResponse>(user);
        userResponse.Roles = roles.ToList();

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenValue,
            RefreshTokenExpiresOn = newRefreshToken.ExpiresOnUtc,
            User = userResponse
        };
    }
}
