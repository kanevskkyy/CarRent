using CarRent.Application.DTOs.Auth;
using CarRent.Domain.Entities;

namespace CarRent.Application.Interfaces.Services;

public interface IAuthTokenService
{
    Task<AuthResponse> IssueAsync(User user, CancellationToken cancellationToken = default);
}