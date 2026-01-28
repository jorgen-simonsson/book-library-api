using BookLibrary.Application.DTOs;
using BookLibrary.Application.Interfaces;
using BookLibrary.Domain.Entities;
using BookLibrary.Domain.Interfaces;

namespace BookLibrary.Application.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<IEnumerable<BookDto>> GetAllBooksAsync(CancellationToken cancellationToken = default)
    {
        var books = await _bookRepository.GetAllAsync(cancellationToken);
        return books.Select(MapToDto);
    }

    public async Task<BookDto?> GetBookByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var book = await _bookRepository.GetByIdAsync(id, cancellationToken);
        return book == null ? null : MapToDto(book);
    }

    public async Task<BookDto> CreateBookAsync(CreateBookDto createBookDto, CancellationToken cancellationToken = default)
    {
        var book = new Book
        {
            Isbn = createBookDto.Isbn,
            Title = createBookDto.Title,
            Author = createBookDto.Author,
            Publisher = createBookDto.Publisher,
            PublishedYear = createBookDto.PublishedYear,
            PageCount = createBookDto.PageCount,
            PlaceId = createBookDto.PlaceId,
            ApiInfo = createBookDto.ApiInfo
        };

        var createdBook = await _bookRepository.AddAsync(book, cancellationToken);
        return MapToDto(createdBook);
    }

    public async Task<BookDto?> UpdateBookAsync(int id, UpdateBookDto updateBookDto, CancellationToken cancellationToken = default)
    {
        var existingBook = await _bookRepository.GetByIdAsync(id, cancellationToken);
        if (existingBook == null)
            return null;

        existingBook.Isbn = updateBookDto.Isbn;
        existingBook.Title = updateBookDto.Title;
        existingBook.Author = updateBookDto.Author;
        existingBook.Publisher = updateBookDto.Publisher;
        existingBook.PublishedYear = updateBookDto.PublishedYear;
        existingBook.PageCount = updateBookDto.PageCount;
        existingBook.PlaceId = updateBookDto.PlaceId;
        existingBook.ApiInfo = updateBookDto.ApiInfo;

        await _bookRepository.UpdateAsync(existingBook, cancellationToken);
        return MapToDto(existingBook);
    }

    public async Task<bool> DeleteBookAsync(int id, CancellationToken cancellationToken = default)
    {
        var existingBook = await _bookRepository.GetByIdAsync(id, cancellationToken);
        if (existingBook == null)
            return false;

        await _bookRepository.DeleteAsync(id, cancellationToken);
        return true;
    }

    private static BookDto MapToDto(Book book)
    {
        return new BookDto(
            book.Id,
            book.Isbn,
            book.Title,
            book.Author,
            book.Publisher,
            book.PublishedYear,
            book.PageCount,
            book.PlaceId,
            book.ApiInfo,
            book.Place?.Descr
        );
    }
}
