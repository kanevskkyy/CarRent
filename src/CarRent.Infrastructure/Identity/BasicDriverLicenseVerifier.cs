using System.Text.RegularExpressions;
using CarRent.Application.Interfaces.Services;

namespace CarRent.Infrastructure.Identity;

public class BasicDriverLicenseVerifier : IDriverLicenseVerifier
{
    // 3 letters + 6 digits, e.g. ABC123456
    private static readonly Regex Format = new(@"^[A-Za-zА-Яа-яІіЇїЄєҐґ]{3}\d{6}$", RegexOptions.Compiled);

    public Task<bool> VerifyAsync(string licenseNumber, CancellationToken cancellationToken)
    {
        string normalized = (licenseNumber ?? string.Empty).Replace(" ", "").Trim();
        return Task.FromResult(Format.IsMatch(normalized));
    }
}