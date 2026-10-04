using MediatR;
using CarRent.Application.Abstractions;
using CarRent.Application.Auth.Commands.UploadDriverLicense;
using CarRent.Application.DTOs.Auth;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Repositories;
using CarRent.Application.Interfaces.Services;
using CarRent.Domain.Entities;
using CarRent.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace CarRent.Application.Auth.Commands.UploadDriverLicense;

public class UploadDriverLicenseCommandHandler : ICommandHandler<UploadDriverLicenseCommand, UploadDriverLicenseResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly IImageUploaderService _imageUploaderService;
    private readonly IDriverLicenseVerifier _verifier;

    public UploadDriverLicenseCommandHandler(
        UserManager<User> userManager,
        IImageUploaderService imageUploaderService,
        IDriverLicenseVerifier verifier)
    {
        _userManager = userManager;
        _imageUploaderService = imageUploaderService;
        _verifier = verifier;
    }

    public async Task<UploadDriverLicenseResponse> Handle(
        UploadDriverLicenseCommand command, CancellationToken cancellationToken)
    {
        User? user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
            throw new NotFoundException("User not found.");

        if (user.LicenseStatus == DriverLicenseVerificationStatus.Verified)
            throw new ConflictException("Driver license is already verified.");

        string number = command.LicenseNumber.Replace(" ", "").Trim().ToUpperInvariant();

        bool isValid = await _verifier.VerifyAsync(number, cancellationToken);
        if (!isValid)
        {
            user.LicenseStatus = DriverLicenseVerificationStatus.Rejected;
            await SaveAsync(user);

            return new UploadDriverLicenseResponse
            {
                Message = "Invalid driver license number format.",
                Status = user.LicenseStatus
            };
        }

        string imageUrl = await _imageUploaderService.UploadAsync(
            command.ImageStream, command.FileName, cancellationToken);

        user.DriverLicenseNumber = number;
        user.DriverLicenseImageUrl = imageUrl;
        user.LicenseStatus = DriverLicenseVerificationStatus.Verified;
        await SaveAsync(user);

        return new UploadDriverLicenseResponse
        {
            Message = "Driver license verified.",
            Status = user.LicenseStatus
        };
    }

    private async Task SaveAsync(User user)
    {
        IdentityResult result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new ValidationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}