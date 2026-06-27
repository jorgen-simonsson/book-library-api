using BookLibrary.Api.Controllers;
using BookLibrary.Application.DTOs;
using BookLibrary.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BookLibrary.Tests;

public class BooksControllerTests
{
    private readonly Mock<IBookService> _service = new();
    private readonly BooksController _sut;

    public BooksControllerTests() => _sut = new BooksController(_service.Object);

    private static BookDto MakeDto(int id = 1, string isbn = "978-3-16-148410-0") =>
        new(id, isbn, "A Test Book", "Test Author", "Test Publisher", "2024", 300, null, null, null);

    // --- GET /api/books ---

    [Fact]
    public async Task GetAll_ReturnsOkWithAllBooks()
    {
        var books = new[] { MakeDto(1), MakeDto(2, "978-0-00-000000-0") };
        _service.Setup(s => s.GetAllBooksAsync(It.IsAny<CancellationToken>())).ReturnsAsync(books);

        var result = await _sut.GetAll(CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<BookDto>>()
            .Which.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ReturnsOkWithEmptyList()
    {
        _service.Setup(s => s.GetAllBooksAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<BookDto>());

        var result = await _sut.GetAll(CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<BookDto>>()
            .Which.Should().BeEmpty();
    }

    // --- GET /api/books/{id} ---

    [Fact]
    public async Task GetById_WhenFound_ReturnsOkWithBook()
    {
        var dto = MakeDto(42);
        _service.Setup(s => s.GetBookByIdAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var result = await _sut.GetById(42, CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _service.Setup(s => s.GetBookByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((BookDto?)null);

        var result = await _sut.GetById(99, CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // --- GET /api/books/isbn/{isbn} ---

    [Fact]
    public async Task GetByIsbn_WhenFound_ReturnsOkWithBook()
    {
        const string isbn = "978-3-16-148410-0";
        var dto = MakeDto(1, isbn);
        _service.Setup(s => s.GetBookByIsbnAsync(isbn, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var result = await _sut.GetByIsbn(isbn, CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task GetByIsbn_WhenNotFound_ReturnsNotFound()
    {
        _service.Setup(s => s.GetBookByIsbnAsync("unknown", It.IsAny<CancellationToken>())).ReturnsAsync((BookDto?)null);

        var result = await _sut.GetByIsbn("unknown", CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // --- POST /api/books ---

    [Fact]
    public async Task Create_ReturnsCreatedAtActionWithBook()
    {
        var createDto = new CreateBookDto("978-1", "Title", "Author", "Pub", "2024", 100, null, null);
        var created = MakeDto(7, "978-1");
        _service.Setup(s => s.CreateBookAsync(createDto, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        var result = await _sut.Create(createDto, CancellationToken.None);

        var createdAt = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAt.ActionName.Should().Be(nameof(_sut.GetById));
        createdAt.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(7);
        createdAt.Value.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task Create_ReturnsStatus201()
    {
        var createDto = new CreateBookDto("isbn", "Title", "Author", "Pub", "2024", 100, null, null);
        _service.Setup(s => s.CreateBookAsync(createDto, It.IsAny<CancellationToken>())).ReturnsAsync(MakeDto(1));

        var result = await _sut.Create(createDto, CancellationToken.None);

        result.Result.Should().BeOfType<CreatedAtActionResult>()
            .Which.StatusCode.Should().Be(201);
    }

    // --- PUT /api/books/{id} ---

    [Fact]
    public async Task Update_WhenFound_ReturnsOkWithUpdatedBook()
    {
        var updateDto = new UpdateBookDto("new-isbn", "New Title", "Author", "Pub", "2025", 200, null, null);
        var updated = MakeDto(10, "new-isbn");
        _service.Setup(s => s.UpdateBookAsync(10, updateDto, It.IsAny<CancellationToken>())).ReturnsAsync(updated);

        var result = await _sut.Update(10, updateDto, CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(updated);
    }

    [Fact]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        var updateDto = new UpdateBookDto("i", "t", "a", "p", "y", 0, null, null);
        _service.Setup(s => s.UpdateBookAsync(99, updateDto, It.IsAny<CancellationToken>())).ReturnsAsync((BookDto?)null);

        var result = await _sut.Update(99, updateDto, CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // --- DELETE /api/books/{id} ---

    [Fact]
    public async Task Delete_WhenFound_ReturnsNoContent()
    {
        _service.Setup(s => s.DeleteBookAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _sut.Delete(5, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_WhenNotFound_ReturnsNotFound()
    {
        _service.Setup(s => s.DeleteBookAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.Delete(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }
}
