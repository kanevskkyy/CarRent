using CarRent.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace CarRent.Domain.Entities;

public class User : IdentityUser<Guid> 
{
    public string FullName { get; set; } = string.Empty;

    public string? DriverLicenseNumber { get; set; }
    public DriverLicenseVerificationStatus LicenseStatus { get; set; } = DriverLicenseVerificationStatus.NotSubmitted;
    
    public List<Booking> Bookings { get; set; } = new();
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}