using CarRent.Application.Abstractions;
using CarRent.Application.DTOs.Auth;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CarRent.Application.Auth.Commands.Login;

public class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly IAuthTokenService _tokenService;

    public LoginCommandHandler(UserManager<User> userManager, IAuthTokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        User? user = await _userManager.FindByEmailAsync(command.Request.Email);
        if (user is null)
            throw new UnauthorizedException("Invalid email or password.");

        bool passwordValid = await _userManager.CheckPasswordAsync(user, command.Request.Password);
        if (!passwordValid)
            throw new UnauthorizedException("Invalid email or password.");

        return await _tokenService.IssueAsync(user, cancellationToken);
    }
}