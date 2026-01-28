using BookLibrary.Application.DTOs;

namespace BookLibrary.Application.Interfaces;

public interface IPlaceService
{
    Task<IEnumerable<PlaceDto>> GetAllPlacesAsync(CancellationToken cancellationToken = default);
    Task<PlaceDto?> GetPlaceByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PlaceDto> CreatePlaceAsync(CreatePlaceDto createPlaceDto, CancellationToken cancellationToken = default);
    Task<PlaceDto?> UpdatePlaceAsync(int id, UpdatePlaceDto updatePlaceDto, CancellationToken cancellationToken = default);
    Task<bool> DeletePlaceAsync(int id, CancellationToken cancellationToken = default);
}
