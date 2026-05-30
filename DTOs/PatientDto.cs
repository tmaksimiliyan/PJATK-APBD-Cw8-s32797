namespace cw8.DTOs;

public record PatientDto(
    string Pesel,
    string FirstName,
    string LastName,
    int Age,
    string Sex,
    List<AdmissionDto> Admissions,
    List<BedAssignmentDto> BedAssignments
);