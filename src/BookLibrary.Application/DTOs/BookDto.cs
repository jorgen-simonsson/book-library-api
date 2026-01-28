using System.Text.Json;

namespace BookLibrary.Application.DTOs;

public record BookDto(
    int Id,
    string Isbn,
    string Title,
    string Author,
    string Publisher,
    string PublishedYear,
    int PageCount,
    int? PlaceId,
    JsonDocument? ApiInfo,
    string? PlaceDescr
);

public record CreateBookDto(
    string Isbn,
    string Title,
    string Author,
    string Publisher,
    string PublishedYear,
    int PageCount,
    int? PlaceId,
    JsonDocument? ApiInfo
);

public record UpdateBookDto(
    string Isbn,
    string Title,
    string Author,
    string Publisher,
    string PublishedYear,
    int PageCount,
    int? PlaceId,
    JsonDocument? ApiInfo
);
