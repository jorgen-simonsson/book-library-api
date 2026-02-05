using BookLibrary.Application.DTOs;
using BookLibrary.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookLibrary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    /// <summary>
    /// Get all books
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetAll(CancellationToken cancellationToken)
    {
        var books = await _bookService.GetAllBooksAsync(cancellationToken);
        return Ok(books);
    }

    /// <summary>
    /// Get a book by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetBookByIdAsync(id, cancellationToken);
        if (book == null)
            return NotFound();
        return Ok(book);
    }

    /// <summary>
    /// Get a book by ISBN
    /// </summary>
    [HttpGet("isbn/{isbn}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookDto>> GetByIsbn(string isbn, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetBookByIsbnAsync(isbn, cancellationToken);
        if (book == null)
            return NotFound();
        return Ok(book);
    }

    /// <summary>
    /// Create a new book
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BookDto>> Create([FromBody] CreateBookDto createBookDto, CancellationToken cancellationToken)
    {
        var book = await _bookService.CreateBookAsync(createBookDto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    /// <summary>
    /// Update an existing book
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookDto>> Update(int id, [FromBody] UpdateBookDto updateBookDto, CancellationToken cancellationToken)
    {
        var book = await _bookService.UpdateBookAsync(id, updateBookDto, cancellationToken);
        if (book == null)
            return NotFound();
        return Ok(book);
    }

    /// <summary>
    /// Delete a book
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _bookService.DeleteBookAsync(id, cancellationToken);
        if (!result)
            return NotFound();
        return NoContent();
    }
}
