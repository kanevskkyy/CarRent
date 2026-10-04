namespace CarRent.Application.Models.Auth;

public record ExternalUserInfo(
    string Provider,
    string Subject,
    string? Email,
    bool EmailVerified,
    string? FullName);