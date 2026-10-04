using CarRent.Application.DTOs.Cars;
using CarRent.Application.Favorites.Commands.AddFavorite;
using CarRent.Application.Favorites.Commands.RemoveFavorite;
using CarRent.Application.Favorites.Queries.GetFavoriteCars;
using CarRent.Application.Interfaces.Services;
using CarRent.Application.Models.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

/// <summary>
/// Favorite (liked) cars of the current user.
/// </summary>
[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IUserClaimsService _userClaimsService;
    private readonly ILogger<FavoritesController> _logger;

    public FavoritesController(
        ISender sender,
        IUserClaimsService userClaimsService,
        ILogger<FavoritesController> logger)
    {
        _sender = sender;
        _userClaimsService = userClaimsService;
        _logger = logger;
    }

    /// <summary>
    /// Get the paged list of the current user's favorite cars (newest first).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedList<CarListResponse>>> Get(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        Guid userId = _userClaimsService.UserId;

        PagedList<CarListResponse> result = await _sender.Send(
            new GetFavoriteCarsQuery(userId, pageNumber, pageSize), cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Add a car to favorites (safe to call twice).
    /// </summary>
    [HttpPost("{carId:guid}")]
    public async Task<ActionResult> Add(Guid carId, CancellationToken cancellationToken)
    {
        Guid userId = _userClaimsService.UserId;

        await _sender.Send(new AddFavoriteCommand(userId, carId), cancellationToken);

        _logger.LogInformation("Car {CarId} added to favorites of User {UserId}", carId, userId);
        return NoContent();
    }

    /// <summary>
    /// Remove a car from favorites (safe to call twice).
    /// </summary>
    [HttpDelete("{carId:guid}")]
    public async Task<ActionResult> Remove(Guid carId, CancellationToken cancellationToken)
    {
        Guid userId = _userClaimsService.UserId;

        await _sender.Send(new RemoveFavoriteCommand(userId, carId), cancellationToken);

        _logger.LogInformation("Car {CarId} removed from favorites of User {UserId}", carId, userId);
        return NoContent();
    }
}