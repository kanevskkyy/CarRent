using CarRent.Application.Models.Auth;

namespace CarRent.Application.Interfaces.Services;

public interface IExternalAuthProvider
{
    string Provider { get; }
    
    Task<ExternalUserInfo> ValidateAsync(string idToken, CancellationToken cancellationToken);
}