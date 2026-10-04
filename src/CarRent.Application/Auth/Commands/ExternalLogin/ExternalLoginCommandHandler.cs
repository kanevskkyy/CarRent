using CarRent.Application.Abstractions;
using CarRent.Application.DTOs.Auth;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Services;
using CarRent.Application.Models.Auth;
using CarRent.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CarRent.Application.Auth.Commands.ExternalLogin;

public class ExternalLoginCommandHandler : ICommandHandler<ExternalLoginCommand, AuthResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly IEnumerable<IExternalAuthProvider> _providers;
    private readonly IAuthTokenService _tokenService;

    public ExternalLoginCommandHandler(
        UserManager<User> userManager,
        IEnumerable<IExternalAuthProvider> providers,
        IAuthTokenService tokenService)
    {
        _userManager = userManager;
        _providers = providers;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(ExternalLoginCommand command, CancellationToken cancellationToken)
    {
        IExternalAuthProvider provider = _providers.FirstOrDefault(p =>
                string.Equals(p.Provider, command.Provider, StringComparison.OrdinalIgnoreCase))
            ?? throw new ValidationException($"Provider '{command.Provider}' is not supported.");

        ExternalUserInfo info = await provider.ValidateAsync(command.Request.IdToken, cancellationToken);

        User? user = await _userManager.FindByLoginAsync(info.Provider, info.Subject);

        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(info.Email) || !info.EmailVerified)
                throw new UnauthorizedException($"{info.Provider} account has no verified email.");

            user = await _userManager.FindByEmailAsync(info.Email);

            if (user is null)
            {
                string fullName = string.IsNullOrWhiteSpace(info.FullName)
                    ? info.Email.Split('@')[0]
                    : info.FullName;

                user = new User
                {
                    Email = info.Email,
                    UserName = info.Email,
                    FullName = fullName.Length > 150 ? fullName[..150] : fullName,
                    EmailConfirmed = true
                };

                IdentityResult createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    throw new ValidationException(string.Join("; ", createResult.Errors.Select(e => e.Description)));

                await _userManager.AddToRoleAsync(user, "User");
            }

            IdentityResult linkResult = await _userManager.AddLoginAsync(
                user, new UserLoginInfo(info.Provider, info.Subject, info.Provider));
            if (!linkResult.Succeeded)
                throw new ValidationException(string.Join("; ", linkResult.Errors.Select(e => e.Description)));
        }

        return await _tokenService.IssueAsync(user, cancellationToken);
    }
}