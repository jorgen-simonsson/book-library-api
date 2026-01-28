using BookLibrary.Application.DTOs;

namespace BookLibrary.Application.Interfaces;

public interface IBookService
{
    Task<IEnumerable<BookDto>> GetAllBooksAsync(CancellationToken cancellationToken = default);
    Task<BookDto?> GetBookByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<BookDto> CreateBookAsync(CreateBookDto createBookDto, CancellationToken cancellationToken = default);
    Task<BookDto?> UpdateBookAsync(int id, UpdateBookDto updateBookDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteBookAsync(int id, CancellationToken cancellationToken = default);
}
