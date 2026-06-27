using BookLibrary.Application.DTOs;
using BookLibrary.Application.Services;
using BookLibrary.Domain.Entities;
using BookLibrary.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace BookLibrary.Tests;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _repo = new();
    private readonly BookService _sut;

    public BookServiceTests() => _sut = new BookService(_repo.Object);

    private static Book MakeBook(int id = 1, string isbn = "978-3-16-148410-0") => new()
    {
        Id = id,
        Isbn = isbn,
        Title = "A Test Book",
        Author = "Test Author",
        Publisher = "Test Publisher",
        PublishedYear = "2024",
        PageCount = 300,
        PlaceId = null,
        ApiInfo = null,
        Place = null
    };

    // --- GetAllBooksAsync ---

    [Fact]
    public async Task GetAllBooksAsync_ReturnsMappedDtoForEachBook()
    {
        var books = new[] { MakeBook(1), MakeBook(2, "978-0-00-000000-0") };
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(books);

        var result = (await _sut.GetAllBooksAsync()).ToList();

        result.Should().HaveCount(2);
        result.Select(b => b.Id).Should().BeEquivalentTo(new[] { 1, 2 });
    }

    [Fact]
    public async Task GetAllBooksAsync_WhenEmpty_ReturnsEmptyCollection()
    {
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Book>());

        var result = await _sut.GetAllBooksAsync();

        result.Should().BeEmpty();
    }

    // --- GetBookByIdAsync ---

    [Fact]
    public async Task GetBookByIdAsync_WhenFound_ReturnsMappedDto()
    {
        var book = MakeBook(42);
        _repo.Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync(book);

        var result = await _sut.GetBookByIdAsync(42);

        result.Should().NotBeNull();
        result!.Id.Should().Be(42);
        result.Isbn.Should().Be(book.Isbn);
        result.Title.Should().Be(book.Title);
        result.Author.Should().Be(book.Author);
        result.Publisher.Should().Be(book.Publisher);
        result.PublishedYear.Should().Be(book.PublishedYear);
        result.PageCount.Should().Be(book.PageCount);
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenNotFound_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Book?)null);

        var result = await _sut.GetBookByIdAsync(99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenBookHasPlace_MapsPlaceDescrAndPlaceId()
    {
        var book = MakeBook(1);
        book.Place = new Place { Id = 2, Descr = "Shelf A" };
        book.PlaceId = 2;
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(book);

        var result = await _sut.GetBookByIdAsync(1);

        result!.PlaceDescr.Should().Be("Shelf A");
        result.PlaceId.Should().Be(2);
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenBookHasNoPlace_PlaceDescrIsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(MakeBook(1));

        var result = await _sut.GetBookByIdAsync(1);

        result!.PlaceDescr.Should().BeNull();
        result.PlaceId.Should().BeNull();
    }

    // --- GetBookByIsbnAsync ---

    [Fact]
    public async Task GetBookByIsbnAsync_WhenFound_ReturnsMappedDto()
    {
        const string isbn = "978-3-16-148410-0";
        _repo.Setup(r => r.GetByIsbnAsync(isbn, It.IsAny<CancellationToken>())).ReturnsAsync(MakeBook(1, isbn));

        var result = await _sut.GetBookByIsbnAsync(isbn);

        result.Should().NotBeNull();
        result!.Isbn.Should().Be(isbn);
    }

    [Fact]
    public async Task GetBookByIsbnAsync_WhenNotFound_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIsbnAsync("unknown-isbn", It.IsAny<CancellationToken>())).ReturnsAsync((Book?)null);

        var result = await _sut.GetBookByIsbnAsync("unknown-isbn");

        result.Should().BeNull();
    }

    // --- CreateBookAsync ---

    [Fact]
    public async Task CreateBookAsync_MapsAllFieldsToNewEntity()
    {
        var dto = new CreateBookDto("978-1", "Title", "Author", "Publisher", "2024", 200, 3, null);
        Book? captured = null;
        _repo
            .Setup(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .Callback<Book, CancellationToken>((b, _) => captured = b)
            .ReturnsAsync((Book b, CancellationToken _) => b);

        await _sut.CreateBookAsync(dto);

        captured.Should().NotBeNull();
        captured!.Isbn.Should().Be("978-1");
        captured.Title.Should().Be("Title");
        captured.Author.Should().Be("Author");
        captured.Publisher.Should().Be("Publisher");
        captured.PublishedYear.Should().Be("2024");
        captured.PageCount.Should().Be(200);
        captured.PlaceId.Should().Be(3);
        captured.ApiInfo.Should().BeNull();
    }

    [Fact]
    public async Task CreateBookAsync_ReturnsCreatedBookAsDto()
    {
        var dto = new CreateBookDto("isbn", "Title", "Author", "Pub", "2024", 100, null, null);
        var created = new Book { Id = 7, Isbn = "isbn", Title = "Title", Author = "Author", Publisher = "Pub", PublishedYear = "2024", PageCount = 100 };
        _repo.Setup(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>())).ReturnsAsync(created);

        var result = await _sut.CreateBookAsync(dto);

        result.Id.Should().Be(7);
        result.Title.Should().Be("Title");
        result.Author.Should().Be("Author");
    }

    // --- UpdateBookAsync ---

    [Fact]
    public async Task UpdateBookAsync_WhenFound_UpdatesAllFieldsOnEntity()
    {
        var existing = MakeBook(10);
        var dto = new UpdateBookDto("new-isbn", "New Title", "New Author", "New Pub", "2025", 500, 4, null);
        _repo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _repo.Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _sut.UpdateBookAsync(10, dto);

        existing.Isbn.Should().Be("new-isbn");
        existing.Title.Should().Be("New Title");
        existing.Author.Should().Be("New Author");
        existing.Publisher.Should().Be("New Pub");
        existing.PublishedYear.Should().Be("2025");
        existing.PageCount.Should().Be(500);
        existing.PlaceId.Should().Be(4);
    }

    [Fact]
    public async Task UpdateBookAsync_WhenFound_ReturnsUpdatedDto()
    {
        var existing = MakeBook(10);
        var dto = new UpdateBookDto("new-isbn", "New Title", "Author", "Pub", "2025", 100, null, null);
        _repo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _repo.Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _sut.UpdateBookAsync(10, dto);

        result.Should().NotBeNull();
        result!.Isbn.Should().Be("new-isbn");
        result.Title.Should().Be("New Title");
    }

    [Fact]
    public async Task UpdateBookAsync_WhenFound_CallsRepositoryUpdate()
    {
        var existing = MakeBook(10);
        _repo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _repo.Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _sut.UpdateBookAsync(10, new UpdateBookDto("i", "t", "a", "p", "y", 0, null, null));

        _repo.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBookAsync_WhenNotFound_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Book?)null);

        var result = await _sut.UpdateBookAsync(99, new UpdateBookDto("i", "t", "a", "p", "y", 0, null, null));

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateBookAsync_WhenNotFound_DoesNotCallRepositoryUpdate()
    {
        _repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Book?)null);

        await _sut.UpdateBookAsync(99, new UpdateBookDto("i", "t", "a", "p", "y", 0, null, null));

        _repo.Verify(r => r.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- DeleteBookAsync ---

    [Fact]
    public async Task DeleteBookAsync_WhenFound_ReturnsTrue()
    {
        _repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(MakeBook(5));
        _repo.Setup(r => r.DeleteAsync(5, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _sut.DeleteBookAsync(5);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteBookAsync_WhenFound_CallsRepositoryDelete()
    {
        _repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(MakeBook(5));
        _repo.Setup(r => r.DeleteAsync(5, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _sut.DeleteBookAsync(5);

        _repo.Verify(r => r.DeleteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteBookAsync_WhenNotFound_ReturnsFalse()
    {
        _repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Book?)null);

        var result = await _sut.DeleteBookAsync(99);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteBookAsync_WhenNotFound_DoesNotCallRepositoryDelete()
    {
        _repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Book?)null);

        await _sut.DeleteBookAsync(99);

        _repo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
