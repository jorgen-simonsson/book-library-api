using BookLibrary.Application.DTOs;
using BookLibrary.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookLibrary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlacesController : ControllerBase
{
    private readonly IPlaceService _placeService;

    public PlacesController(IPlaceService placeService)
    {
        _placeService = placeService;
    }

    /// <summary>
    /// Get all places
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PlaceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PlaceDto>>> GetAll(CancellationToken cancellationToken)
    {
        var places = await _placeService.GetAllPlacesAsync(cancellationToken);
        return Ok(places);
    }

    /// <summary>
    /// Get a place by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PlaceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaceDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var place = await _placeService.GetPlaceByIdAsync(id, cancellationToken);
        if (place == null)
            return NotFound();
        return Ok(place);
    }

    /// <summary>
    /// Create a new place
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PlaceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlaceDto>> Create([FromBody] CreatePlaceDto createPlaceDto, CancellationToken cancellationToken)
    {
        var place = await _placeService.CreatePlaceAsync(createPlaceDto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = place.Id }, place);
    }

    /// <summary>
    /// Update an existing place
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PlaceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaceDto>> Update(int id, [FromBody] UpdatePlaceDto updatePlaceDto, CancellationToken cancellationToken)
    {
        var place = await _placeService.UpdatePlaceAsync(id, updatePlaceDto, cancellationToken);
        if (place == null)
            return NotFound();
        return Ok(place);
    }

    /// <summary>
    /// Delete a place
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _placeService.DeletePlaceAsync(id, cancellationToken);
        if (!result)
            return NotFound();
        return NoContent();
    }
}
