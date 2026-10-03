using CarRent.Application.Cars.Commands.CreateCar;
using CarRent.Application.Cars.Commands.DeleteCar;
using CarRent.Application.Cars.Commands.UpdateCar;
using CarRent.Application.Cars.Queries.GetCarById;
using CarRent.Application.Cars.Queries.GetPagedCars;
using CarRent.Application.DTOs.Cars;
using CarRent.Application.Models.Cars;
using CarRent.Application.Models.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/cars")]
public class CarsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<CarsController> _logger;

    public CarsController(ISender sender, ILogger<CarsController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Get paged list of cars with filters.
    /// </summary>
    /// <param name="filter">Filter options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of cars.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedList<CarListResponse>>> GetPagedCars([FromQuery] CarFilterModel filter,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching paged cars with filters.");
        GetPagedCarsQuery query = new GetPagedCarsQuery(filter);
        PagedList<CarListResponse> result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get car details by ID.
    /// </summary>
    /// <param name="id">Car ID (GUID).</param>
    /// <param name="currentUserId">Current user ID (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Car details.</returns>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<CarDetailResponse>> GetById(Guid id, [FromQuery] Guid? currentUserId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching car with ID: {Id}", id);
        GetCarByIdQuery query = new GetCarByIdQuery(id, currentUserId);
        CarDetailResponse? result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Create a new car.
    /// </summary>
    /// <param name="request">Car creation data.</param>
    /// <param name="photo">Car image file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created car details.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<CarDetailResponse>> Create([FromForm] CreateCarRequest request, IFormFile photo,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new car with model: {Model}", request);

        using Stream photoStream = photo.OpenReadStream();
        CreateCarCommand command = new CreateCarCommand(request, photoStream, photo.FileName);

        CarDetailResponse createdCar = await _sender.Send(command, cancellationToken);

        _logger.LogInformation("Car created with ID: {Id}", createdCar.Id);
        return CreatedAtAction(nameof(GetById), new { id = createdCar.Id }, createdCar);
    }

    /// <summary>
    /// Update an existing car by ID.
    /// </summary>
    /// <param name="id">Car ID (GUID).</param>
    /// <param name="request">Car update data.</param>
    /// <param name="photo">New car image file (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated car details.</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<CarDetailResponse>> Update(Guid id, [FromForm] UpdateCarRequest request,
        IFormFile? photo, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating car with ID: {Id}", id);

        Stream? photoStream = null;
        string? fileName = null;

        if (photo != null && photo.Length > 0)
        {
            photoStream = photo.OpenReadStream();
            fileName = photo.FileName;
        }

        UpdateCarCommand command = new UpdateCarCommand(id, request, photoStream, fileName);
        CarDetailResponse? updatedCar = await _sender.Send(command, cancellationToken);

        _logger.LogInformation("Car with ID {Id} updated.", id);
        return Ok(updatedCar);
    }

    /// <summary>
    /// Delete a car by ID.
    /// </summary>
    /// <param name="id">Car ID (GUID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting car with ID: {Id}", id);

        DeleteCarCommand command = new DeleteCarCommand(id);
        await _sender.Send(command, cancellationToken);

        _logger.LogInformation("Car with ID {Id} deleted.", id);
        return NoContent();
    }
}