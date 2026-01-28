using BookLibrary.Domain.Entities;
using BookLibrary.Domain.Interfaces;
using BookLibrary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookLibrary.Infrastructure.Repositories;

public class PlaceRepository : IPlaceRepository
{
    private readonly BookLibraryDbContext _context;

    public PlaceRepository(BookLibraryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Place>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Places.ToListAsync(cancellationToken);
    }

    public async Task<Place?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Places.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Place> AddAsync(Place place, CancellationToken cancellationToken = default)
    {
        _context.Places.Add(place);
        await _context.SaveChangesAsync(cancellationToken);
        return place;
    }

    public async Task UpdateAsync(Place place, CancellationToken cancellationToken = default)
    {
        _context.Places.Update(place);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var place = await _context.Places.FindAsync(new object[] { id }, cancellationToken);
        if (place != null)
        {
            _context.Places.Remove(place);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
