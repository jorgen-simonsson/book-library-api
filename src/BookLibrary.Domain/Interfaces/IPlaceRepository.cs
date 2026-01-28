using BookLibrary.Domain.Entities;

namespace BookLibrary.Domain.Interfaces;

public interface IPlaceRepository
{
    Task<IEnumerable<Place>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Place?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Place> AddAsync(Place place, CancellationToken cancellationToken = default);
    Task UpdateAsync(Place place, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
