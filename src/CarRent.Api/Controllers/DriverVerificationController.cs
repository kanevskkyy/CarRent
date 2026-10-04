using CarRent.Application.Auth.Commands.UploadDriverLicense;
using CarRent.Application.DTOs.Auth;
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
        IFormFile file,
        CancellationToken cancellationToken)
    {
        Guid userId = _userClaimsService.UserId;

        _logger.LogInformation(
            "Uploading driver license for User ID: {UserId}",
            userId);

        using Stream stream = file.OpenReadStream();

        UploadDriverLicenseCommand command =
            new(userId, stream, file.FileName);

        UploadDriverLicenseResponse result =
            await _sender.Send(command, cancellationToken);

        _logger.LogInformation(
            "Driver license uploaded for User ID: {UserId}",
            userId);

        return Ok(result);
    }
}