using AutoMapper;
using CarRent.Application.Common.Settings;
using CarRent.Application.DTOs.Auth;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CarRent.Infrastructure.Services;

public class AuthTokenService : IAuthTokenService
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly JwtSettings _jwtSettings;

    public AuthTokenService(
        UserManager<User> userManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<AuthResponse> IssueAsync(User user, CancellationToken cancellationToken = default)
    {
        IList<string> roles = await _userManager.GetRolesAsync(user);

        string accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        string refreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();

        DateTime now = DateTime.UtcNow;

        List<Domain.Entities.RefreshToken> activeTokens =
            await _unitOfWork.RefreshTokens.GetActiveByUserIdAsync(user.Id, cancellationToken);
        foreach (Domain.Entities.RefreshToken token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedOnUtc = now;
            token.ReplacedByToken = refreshTokenValue;
        }

        Domain.Entities.RefreshToken refreshToken = new()
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            ExpiresOnUtc = now.AddDays(_jwtSettings.RefreshTokenExpiryDays)
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