using CarRent.Application.Auth.Commands.Login;
using CarRent.Application.Auth.Commands.RefreshToken;
using CarRent.Application.Auth.Commands.Register;
using CarRent.Application.Auth.Commands.RevokeToken;
using CarRent.Application.Auth.Commands.UploadDriverLicense;
using CarRent.Application.DTOs.Auth;
using CarRent.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

/// <summary>
/// Authentication and authorization endpoints.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IUserClaimsService _userClaimsService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ISender sender,
        IUserClaimsService userClaimsService,
        ILogger<AuthController> logger)
    {
        _sender = sender;
        _userClaimsService = userClaimsService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    /// <param name="request">User registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication tokens and user info.</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering new user with email: {Email}", request.Email);

        RegisterCommand command = new(request);
        AuthResponse result = await _sender.Send(command, cancellationToken);

        _logger.LogInformation("User registered successfully: {Email}", request.Email);
        return Ok(result);
    }

    /// <summary>
    /// Authenticate with email and password.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication tokens and user info.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        LoginCommand command = new(request);
        AuthResponse result = await _sender.Send(command, cancellationToken);

        _logger.LogInformation("User logged in successfully: {Email}", request.Email);
        return Ok(result);
    }

    /// <summary>
    /// Refresh the access token using a valid refresh token.
    /// </summary>
    /// <param name="request">Refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>New pair of authentication tokens.</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Refreshing access token.");

        RefreshTokenCommand command = new(request);
        AuthResponse result = await _sender.Send(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Revoke a refresh token (logout).
    /// </summary>
    /// <param name="request">Refresh token to revoke.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<ActionResult> Logout(
        [FromBody] RevokeTokenRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Revoking refresh token (logout).");

        RevokeTokenCommand command = new(request);
        await _sender.Send(command, cancellationToken);

        _logger.LogInformation("Token revoked successfully.");
        return NoContent();
    }
}
