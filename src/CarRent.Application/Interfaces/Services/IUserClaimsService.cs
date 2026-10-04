namespace CarRent.Application.Interfaces.Services;

public interface IUserClaimsService
{
    Guid UserId { get; }
    bool IsAdmin { get; }
    bool IsAuthenticated { get; }
}