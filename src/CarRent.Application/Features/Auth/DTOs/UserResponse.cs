using CarRent.Domain.Enums;

namespace CarRent.Application.DTOs.Auth;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? DriverLicenseNumber { get; set; }
    public DriverLicenseVerificationStatus LicenseStatus { get; set; }
    public List<string> Roles { get; set; } = new();
}
