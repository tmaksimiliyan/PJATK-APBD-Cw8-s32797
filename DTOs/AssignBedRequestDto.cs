using System.ComponentModel.DataAnnotations;

namespace cw8.DTOs;

public class AssignBedRequestDto
{
    [Required]
    public DateTime From { get; set; }

    public DateTime? To { get; set; }

    [Required]
    public string BedType { get; set; } = null!;

    [Required]
    public string Ward { get; set; } = null!;
}