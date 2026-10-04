using CarRent.Domain.Enums;

namespace CarRent.Application.DTOs.Auth;

public class UploadDriverLicenseResponse
{
    public string Message { get; set; } = string.Empty;
    public DriverLicenseVerificationStatus Status { get; set; }
}
