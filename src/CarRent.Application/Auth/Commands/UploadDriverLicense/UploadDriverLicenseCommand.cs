using CarRent.Application.Abstractions;
using CarRent.Application.DTOs.Auth;

namespace CarRent.Application.Auth.Commands.UploadDriverLicense;

public record UploadDriverLicenseCommand(
    Guid UserId,
    Stream ImageStream,
    string FileName
) : ICommand<UploadDriverLicenseResponse>;
