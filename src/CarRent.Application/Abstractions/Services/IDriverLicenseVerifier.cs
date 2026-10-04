namespace CarRent.Application.Interfaces.Services;

public interface IDriverLicenseVerifier
{
    Task<bool> VerifyAsync(string licenseNumber, CancellationToken cancellationToken);
}