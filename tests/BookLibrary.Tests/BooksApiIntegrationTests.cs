using System.Net;
using System.Net.Http.Json;
using BookLibrary.Application.DTOs;
using BookLibrary.Application.Interfaces;
using BookLibrary.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BookLibrary.Tests;

public class BooksApiIntegrationTests : IClassFixture<BooksApiIntegrationTests.ApiFactory>
{
    private readonly HttpClient _client;

    public BooksApiIntegrationTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    // Minimal valid payload — publishedYear omitted (null) by default
    private static object ValidPayload(string? publishedYear = "2024") => new
    {
        isbn = "978-3-16-148410-0",
        title = "Integration Test Book",
        author = "Test Author",
        publisher = "Test Publisher",
        publishedYear,
        pageCount = 100
    };

    // --- Tests that directly reproduce the attribute-placement bug ---
    // Before the fix ([property: ValidPublishedYear] instead of [ValidPublishedYear]),
    // ANY POST to /api/books returned 500 because ASP.NET Core's model validation
    // pipeline threw InvalidOperationException before even reaching the controller.

    [Fact]
    public async Task Post_WithValidPublishedYear_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/api/books", ValidPayload("2024"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Post_WithNullPublishedYear_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/api/books", ValidPayload(null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    // --- Validation rejection tests ---

    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("")]
    [InlineData("20a4")]
    public async Task Post_WithInvalidPublishedYear_Returns400(string invalidYear)
    {
        var response = await _client.PostAsJsonAsync("/api/books", ValidPayload(invalidYear));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_WithInvalidPublishedYear_ResponseBodyContainsFieldName()
    {
        var response = await _client.PostAsJsonAsync("/api/books", ValidPayload("abc"));
        var body = await response.Content.ReadAsStringAsync();

        body.Should().ContainEquivalentOf("publishedYear");
    }

    // --- PUT uses the same validation attribute ---

    [Fact]
    public async Task Put_WithInvalidPublishedYear_Returns400()
    {
        var payload = new
        {
            isbn = "978-3-16-148410-0",
            title = "Title",
            author = "Author",
            publisher = "Pub",
            publishedYear = "bad",
            pageCount = 100
        };

        var response = await _client.PutAsJsonAsync("/api/books/1", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ---------------------------------------------------------------

    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove the PostgreSQL DbContext (requires a live DB connection)
                var npgsqlDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<BookLibraryDbContext>));
                if (npgsqlDescriptor != null) services.Remove(npgsqlDescriptor);

                services.AddDbContext<BookLibraryDbContext>(options =>
                    options.UseInMemoryDatabase("IntegrationTestDb"));

                // Replace IBookService with a mock so the test does not need a real
                // database and focuses purely on the HTTP pipeline (model binding +
                // validation) — the layer where the attribute-placement bug lived.
                var existing = services.SingleOrDefault(d => d.ServiceType == typeof(IBookService));
                if (existing != null) services.Remove(existing);

                var mock = new Mock<IBookService>();
                mock.Setup(s => s.CreateBookAsync(
                        It.IsAny<CreateBookDto>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new BookDto(
                        1, "978-3-16-148410-0", "Integration Test Book",
                        "Test Author", "Test Publisher", "2024", 100, null, null, null));

                services.AddScoped<IBookService>(_ => mock.Object);
            });
        }
    }
}
