using CarRent.Application.Abstractions;
using CarRent.Application.DTOs.Auth;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CarRent.Application.Auth.Commands.Register;

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, AuthResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly IAuthTokenService _tokenService;

    public RegisterCommandHandler(UserManager<User> userManager, IAuthTokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
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

        return await _tokenService.IssueAsync(user, cancellationToken);
    }
}