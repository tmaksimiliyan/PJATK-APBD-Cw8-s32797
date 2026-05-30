namespace cw8.DTOs;

public record RoomDto(
    string Id,
    bool HasTv,
    WardDto Ward
);