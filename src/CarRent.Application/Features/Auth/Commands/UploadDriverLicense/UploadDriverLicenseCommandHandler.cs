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
    private readonly IUnitOfWork _unitOfWork;

    public UploadDriverLicenseCommandHandler(
        UserManager<User> userManager,
        IImageUploaderService imageUploaderService,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _imageUploaderService = imageUploaderService;
        _unitOfWork = unitOfWork;
    }

    public async Task<UploadDriverLicenseResponse> Handle(UploadDriverLicenseCommand command, CancellationToken cancellationToken)
    {
        // TODO: Implement driver license upload logic
        // 1. Find user by command.UserId
        // 2. Upload image to cloud storage via _imageUploaderService
        // 3. Save the image URL to user.DriverLicenseNumber or separate field
        // 4. Set user.LicenseStatus = DriverLicenseVerificationStatus.Pending
        // 5. Save changes via UserManager
        // 6. Return UploadDriverLicenseResponse with message and status
        throw new NotImplementedException("UploadDriverLicense is not yet implemented.");
    }
}
