namespace cw8.DTOs;

public record BedDto(
    int Id,
    BedTypeDto BedType,
    RoomDto Room
);