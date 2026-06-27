using BookLibrary.Application.DTOs;
using BookLibrary.Application.Services;
using BookLibrary.Domain.Entities;
using BookLibrary.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace BookLibrary.Tests;

public class PlaceServiceTests
{
    private readonly Mock<IPlaceRepository> _repo = new();
    private readonly PlaceService _sut;

    public PlaceServiceTests() => _sut = new PlaceService(_repo.Object);

    private static Place MakePlace(int id = 1, string descr = "Shelf A") => new()
    {
        Id = id,
        Descr = descr
    };

    // --- GetAllPlacesAsync ---

    [Fact]
    public async Task GetAllPlacesAsync_ReturnsMappedDtoForEachPlace()
    {
        var places = new[] { MakePlace(1, "Shelf A"), MakePlace(2, "Shelf B") };
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(places);

        var result = (await _sut.GetAllPlacesAsync()).ToList();

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(1);
        result[0].Descr.Should().Be("Shelf A");
        result[1].Id.Should().Be(2);
        result[1].Descr.Should().Be("Shelf B");
    }

    [Fact]
    public async Task GetAllPlacesAsync_WhenEmpty_ReturnsEmptyCollection()
    {
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Place>());

        var result = await _sut.GetAllPlacesAsync();

        result.Should().BeEmpty();
    }

    // --- GetPlaceByIdAsync ---

    [Fact]
    public async Task GetPlaceByIdAsync_WhenFound_ReturnsMappedDto()
    {
        _repo.Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync(MakePlace(42, "Attic"));

        var result = await _sut.GetPlaceByIdAsync(42);

        result.Should().NotBeNull();
        result!.Id.Should().Be(42);
        result.Descr.Should().Be("Attic");
    }

    [Fact]
    public async Task GetPlaceByIdAsync_WhenNotFound_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Place?)null);

        var result = await _sut.GetPlaceByIdAsync(99);

        result.Should().BeNull();
    }

    // --- CreatePlaceAsync ---

    [Fact]
    public async Task CreatePlaceAsync_MapsDescrToNewEntity()
    {
        var dto = new CreatePlaceDto("Living Room");
        Place? captured = null;
        _repo
            .Setup(r => r.AddAsync(It.IsAny<Place>(), It.IsAny<CancellationToken>()))
            .Callback<Place, CancellationToken>((p, _) => captured = p)
            .ReturnsAsync((Place p, CancellationToken _) => p);

        await _sut.CreatePlaceAsync(dto);

        captured.Should().NotBeNull();
        captured!.Descr.Should().Be("Living Room");
    }

    [Fact]
    public async Task CreatePlaceAsync_ReturnsCreatedPlaceAsDto()
    {
        var dto = new CreatePlaceDto("Garage");
        var created = new Place { Id = 5, Descr = "Garage" };
        _repo.Setup(r => r.AddAsync(It.IsAny<Place>(), It.IsAny<CancellationToken>())).ReturnsAsync(created);

        var result = await _sut.CreatePlaceAsync(dto);

        result.Id.Should().Be(5);
        result.Descr.Should().Be("Garage");
    }

    // --- UpdatePlaceAsync ---

    [Fact]
    public async Task UpdatePlaceAsync_WhenFound_UpdatesDescrOnEntity()
    {
        var existing = MakePlace(10, "Old Descr");
        var dto = new UpdatePlaceDto("New Descr");
        _repo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _repo.Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _sut.UpdatePlaceAsync(10, dto);

        existing.Descr.Should().Be("New Descr");
    }

    [Fact]
    public async Task UpdatePlaceAsync_WhenFound_ReturnsUpdatedDto()
    {
        var existing = MakePlace(10, "Old");
        var dto = new UpdatePlaceDto("New");
        _repo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _repo.Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _sut.UpdatePlaceAsync(10, dto);

        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
        result.Descr.Should().Be("New");
    }

    [Fact]
    public async Task UpdatePlaceAsync_WhenFound_CallsRepositoryUpdate()
    {
        var existing = MakePlace(10);
        _repo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _repo.Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _sut.UpdatePlaceAsync(10, new UpdatePlaceDto("New"));

        _repo.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePlaceAsync_WhenNotFound_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Place?)null);

        var result = await _sut.UpdatePlaceAsync(99, new UpdatePlaceDto("x"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdatePlaceAsync_WhenNotFound_DoesNotCallRepositoryUpdate()
    {
        _repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Place?)null);

        await _sut.UpdatePlaceAsync(99, new UpdatePlaceDto("x"));

        _repo.Verify(r => r.UpdateAsync(It.IsAny<Place>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- DeletePlaceAsync ---

    [Fact]
    public async Task DeletePlaceAsync_WhenFound_ReturnsTrue()
    {
        _repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(MakePlace(5));
        _repo.Setup(r => r.DeleteAsync(5, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _sut.DeletePlaceAsync(5);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeletePlaceAsync_WhenFound_CallsRepositoryDelete()
    {
        _repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(MakePlace(5));
        _repo.Setup(r => r.DeleteAsync(5, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _sut.DeletePlaceAsync(5);

        _repo.Verify(r => r.DeleteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePlaceAsync_WhenNotFound_ReturnsFalse()
    {
        _repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Place?)null);

        var result = await _sut.DeletePlaceAsync(99);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeletePlaceAsync_WhenNotFound_DoesNotCallRepositoryDelete()
    {
        _repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Place?)null);

        await _sut.DeletePlaceAsync(99);

        _repo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
