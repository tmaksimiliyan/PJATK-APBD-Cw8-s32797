namespace cw8.DTOs;

public record BedAssignmentDto(
    int Id,
    DateTime From,
    DateTime? To,
    BedDto Bed
);