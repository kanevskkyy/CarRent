using CarRent.Application.Auth.Commands.UploadDriverLicense;
using CarRent.Application.DTOs.Auth;
using CarRent.Application.Exceptions;
using CarRent.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/driver-verification")]
[Authorize]
public class DriverVerificationController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IUserClaimsService _userClaimsService;
    private readonly ILogger<DriverVerificationController> _logger;

    public DriverVerificationController(
        ISender sender,
        IUserClaimsService userClaimsService,
        ILogger<DriverVerificationController> logger)
    {
        _sender = sender;
        _userClaimsService = userClaimsService;
        _logger = logger;
    }

    /// <summary>
    /// Upload driver license image for verification.
    /// </summary>
    [HttpPost("submit")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadDriverLicenseResponse>> UploadLicense(
        [FromForm] string licenseNumber,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        string[] allowed = { ".jpg", ".jpeg", ".png" };
        string ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (file.Length == 0 || file.Length > 5 * 1024 * 1024)
            throw new ValidationException("File must be between 1 byte and 5 MB.");
        if (!allowed.Contains(ext))
            throw new ValidationException("Only .jpg, .jpeg and .png files are allowed.");

        Guid userId = _userClaimsService.UserId;
        _logger.LogInformation("Uploading driver license for User ID: {UserId}", userId);

        using Stream stream = file.OpenReadStream();
        UploadDriverLicenseResponse result = await _sender.Send(
            new UploadDriverLicenseCommand(userId, stream, file.FileName, licenseNumber), cancellationToken);

        return Ok(result);
    }
}