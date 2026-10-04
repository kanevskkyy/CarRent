using CarRent.Application.Abstractions;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthTokenService _tokenService;

    public RefreshTokenCommandHandler(
        UserManager<User> userManager,
        IUnitOfWork unitOfWork,
        IAuthTokenService tokenService)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
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

        return await _tokenService.IssueAsync(user, cancellationToken);
    }
}