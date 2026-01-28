using BookLibrary.Application.DTOs;
using BookLibrary.Application.Interfaces;
using BookLibrary.Domain.Entities;
using BookLibrary.Domain.Interfaces;

namespace BookLibrary.Application.Services;

public class PlaceService : IPlaceService
{
    private readonly IPlaceRepository _placeRepository;

    public PlaceService(IPlaceRepository placeRepository)
    {
        _placeRepository = placeRepository;
    }

    public async Task<IEnumerable<PlaceDto>> GetAllPlacesAsync(CancellationToken cancellationToken = default)
    {
        var places = await _placeRepository.GetAllAsync(cancellationToken);
        return places.Select(MapToDto);
    }

    public async Task<PlaceDto?> GetPlaceByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var place = await _placeRepository.GetByIdAsync(id, cancellationToken);
        return place == null ? null : MapToDto(place);
    }

    public async Task<PlaceDto> CreatePlaceAsync(CreatePlaceDto createPlaceDto, CancellationToken cancellationToken = default)
    {
        var place = new Place
        {
            Descr = createPlaceDto.Descr
        };

        var createdPlace = await _placeRepository.AddAsync(place, cancellationToken);
        return MapToDto(createdPlace);
    }

    public async Task<PlaceDto?> UpdatePlaceAsync(int id, UpdatePlaceDto updatePlaceDto, CancellationToken cancellationToken = default)
    {
        var existingPlace = await _placeRepository.GetByIdAsync(id, cancellationToken);
        if (existingPlace == null)
            return null;

        existingPlace.Descr = updatePlaceDto.Descr;

        await _placeRepository.UpdateAsync(existingPlace, cancellationToken);
        return MapToDto(existingPlace);
    }

    public async Task<bool> DeletePlaceAsync(int id, CancellationToken cancellationToken = default)
    {
        var existingPlace = await _placeRepository.GetByIdAsync(id, cancellationToken);
        if (existingPlace == null)
            return false;

        await _placeRepository.DeleteAsync(id, cancellationToken);
        return true;
    }

    private static PlaceDto MapToDto(Place place)
    {
        return new PlaceDto(place.Id, place.Descr);
    }
}
