using AutoMapper;
using CarRent.Application.Abstractions;
using CarRent.Application.Auth.Commands.Login;
using CarRent.Application.DTOs.Auth;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CarRent.Application.Auth.Commands.Login;

public class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LoginCommandHandler(
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

    public async Task<AuthResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        User? user = await _userManager.FindByEmailAsync(command.Request.Email);
        if (user is null)
            throw new UnauthorizedException("Invalid email or password.");

        bool passwordValid = await _userManager.CheckPasswordAsync(user, command.Request.Password);
        if (!passwordValid)
            throw new UnauthorizedException("Invalid email or password.");

        IList<string> roles = await _userManager.GetRolesAsync(user);

        string accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        string refreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();

        List<Domain.Entities.RefreshToken> activeTokens = await _unitOfWork.RefreshTokens.GetActiveByUserIdAsync(user.Id, cancellationToken);
        foreach (Domain.Entities.RefreshToken token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedOnUtc = DateTime.UtcNow;
            token.ReplacedByToken = refreshTokenValue;
        }

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
