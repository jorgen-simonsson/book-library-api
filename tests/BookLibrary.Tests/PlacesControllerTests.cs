using BookLibrary.Api.Controllers;
using BookLibrary.Application.DTOs;
using BookLibrary.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BookLibrary.Tests;

public class PlacesControllerTests
{
    private readonly Mock<IPlaceService> _service = new();
    private readonly PlacesController _sut;

    public PlacesControllerTests() => _sut = new PlacesController(_service.Object);

    private static PlaceDto MakeDto(int id = 1, string descr = "Shelf A") => new(id, descr);

    // --- GET /api/places ---

    [Fact]
    public async Task GetAll_ReturnsOkWithAllPlaces()
    {
        var places = new[] { MakeDto(1, "Shelf A"), MakeDto(2, "Shelf B") };
        _service.Setup(s => s.GetAllPlacesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(places);

        var result = await _sut.GetAll(CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<PlaceDto>>()
            .Which.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ReturnsOkWithEmptyList()
    {
        _service.Setup(s => s.GetAllPlacesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<PlaceDto>());

        var result = await _sut.GetAll(CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<PlaceDto>>()
            .Which.Should().BeEmpty();
    }

    // --- GET /api/places/{id} ---

    [Fact]
    public async Task GetById_WhenFound_ReturnsOkWithPlace()
    {
        var dto = MakeDto(42, "Attic");
        _service.Setup(s => s.GetPlaceByIdAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var result = await _sut.GetById(42, CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _service.Setup(s => s.GetPlaceByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((PlaceDto?)null);

        var result = await _sut.GetById(99, CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // --- POST /api/places ---

    [Fact]
    public async Task Create_ReturnsCreatedAtActionWithPlace()
    {
        var createDto = new CreatePlaceDto("Living Room");
        var created = MakeDto(5, "Living Room");
        _service.Setup(s => s.CreatePlaceAsync(createDto, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        var result = await _sut.Create(createDto, CancellationToken.None);

        var createdAt = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAt.ActionName.Should().Be(nameof(_sut.GetById));
        createdAt.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(5);
        createdAt.Value.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task Create_ReturnsStatus201()
    {
        var createDto = new CreatePlaceDto("Garage");
        _service.Setup(s => s.CreatePlaceAsync(createDto, It.IsAny<CancellationToken>())).ReturnsAsync(MakeDto(1));

        var result = await _sut.Create(createDto, CancellationToken.None);

        result.Result.Should().BeOfType<CreatedAtActionResult>()
            .Which.StatusCode.Should().Be(201);
    }

    // --- PUT /api/places/{id} ---

    [Fact]
    public async Task Update_WhenFound_ReturnsOkWithUpdatedPlace()
    {
        var updateDto = new UpdatePlaceDto("New Descr");
        var updated = MakeDto(10, "New Descr");
        _service.Setup(s => s.UpdatePlaceAsync(10, updateDto, It.IsAny<CancellationToken>())).ReturnsAsync(updated);

        var result = await _sut.Update(10, updateDto, CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(updated);
    }

    [Fact]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        var updateDto = new UpdatePlaceDto("x");
        _service.Setup(s => s.UpdatePlaceAsync(99, updateDto, It.IsAny<CancellationToken>())).ReturnsAsync((PlaceDto?)null);

        var result = await _sut.Update(99, updateDto, CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // --- DELETE /api/places/{id} ---

    [Fact]
    public async Task Delete_WhenFound_ReturnsNoContent()
    {
        _service.Setup(s => s.DeletePlaceAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _sut.Delete(5, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_WhenNotFound_ReturnsNotFound()
    {
        _service.Setup(s => s.DeletePlaceAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.Delete(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }
}
