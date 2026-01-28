namespace BookLibrary.Application.DTOs;

public record PlaceDto(
    int Id,
    string Descr
);

public record CreatePlaceDto(
    string Descr
);

public record UpdatePlaceDto(
    string Descr
);
