using CarRent.Application.Common.Settings;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Services;
using CarRent.Application.Models.Auth;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace CarRent.Infrastructure.ExternalServices;

public class GoogleAuthProvider : IExternalAuthProvider
{
    private readonly GoogleSettings _settings;

    public GoogleAuthProvider(IOptions<GoogleSettings> options)
    {
        _settings = options.Value;
    }

    public string Provider => "Google";

    public async Task<ExternalUserInfo> ValidateAsync(string idToken, CancellationToken cancellationToken)
    {
        try
        {
            GoogleJsonWebSignature.ValidationSettings validationSettings = new()
            {
                Audience = new[] { _settings.ClientId }
            };

            GoogleJsonWebSignature.Payload payload =
                await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);

            return new ExternalUserInfo(
                Provider,
                payload.Subject,
                payload.Email,
                payload.EmailVerified,
                payload.Name);
        }
        catch (InvalidJwtException)
        {
            throw new UnauthorizedException("Invalid Google token.");
        }
    }
}