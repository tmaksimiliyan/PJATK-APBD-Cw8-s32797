using cw8.Data;
using cw8.DTOs;
using cw8.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cw8.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
    private readonly HospitalDbContext _context;

    public PatientsController(HospitalDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatients([FromQuery] string? search)
    {
        var query = _context.Patients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search}%";

            query = query.Where(p =>
                EF.Functions.Like(p.FirstName, pattern) ||
                EF.Functions.Like(p.LastName, pattern));
        }

        var patients = await query
            .Select(p => new PatientDto(
                p.Pesel.Trim(),
                p.FirstName,
                p.LastName,
                p.Age,
                p.Sex ? "Male" : "Female",

                p.Admissions
                    .Select(a => new AdmissionDto(
                        a.Id,
                        a.AdmissionDate,
                        a.DischargeDate,
                        new WardDto(
                            a.Ward.Id,
                            a.Ward.Name,
                            a.Ward.Description
                        )
                    ))
                    .ToList(),

                p.BedAssignments
                    .Select(ba => new BedAssignmentDto(
                        ba.Id,
                        ba.From,
                        ba.To,
                        new BedDto(
                            ba.Bed.Id,
                            new BedTypeDto(
                                ba.Bed.BedType.Id,
                                ba.Bed.BedType.Name,
                                ba.Bed.BedType.Description
                            ),
                            new RoomDto(
                                ba.Bed.Room.Id,
                                ba.Bed.Room.HasTv,
                                new WardDto(
                                    ba.Bed.Room.Ward.Id,
                                    ba.Bed.Room.Ward.Name,
                                    ba.Bed.Room.Ward.Description
                                )
                            )
                        )
                    ))
                    .ToList()
            ))
            .ToListAsync();

        return Ok(patients);
    }

    [HttpPost("{pesel}/bedassignments")]
    public async Task<IActionResult> AssignBedToPatient(
        [FromRoute] string pesel,
        [FromBody] AssignBedRequestDto request)
    {
        if (request.To.HasValue && request.To <= request.From)
        {
            return BadRequest(new
            {
                message = "Data końcowa musi być późniejsza niż data początkowa."
            });
        }

        var patientExists = await _context.Patients
            .AnyAsync(p => p.Pesel == pesel);

        if (!patientExists)
        {
            return NotFound(new
            {
                message = $"Nie znaleziono pacjenta o numerze PESEL: {pesel}."
            });
        }

        var wardExists = await _context.Wards
            .AnyAsync(w => w.Name == request.Ward);

        if (!wardExists)
        {
            return NotFound(new
            {
                message = $"Nie znaleziono oddziału: {request.Ward}."
            });
        }

        var bedTypeExists = await _context.BedTypes
            .AnyAsync(bt => bt.Name == request.BedType);

        if (!bedTypeExists)
        {
            return NotFound(new
            {
                message = $"Nie znaleziono typu łóżka: {request.BedType}."
            });
        }

        var freeBed = await _context.Beds
            .Include(b => b.BedType)
            .Include(b => b.Room)
            .ThenInclude(r => r.Ward)
            .Where(b =>
                b.BedType.Name == request.BedType &&
                b.Room.Ward.Name == request.Ward)
            .Where(b => !b.BedAssignments.Any(assignment =>
                (!request.To.HasValue || assignment.From < request.To.Value) &&
                (!assignment.To.HasValue || request.From < assignment.To.Value)))
            .FirstOrDefaultAsync();

        if (freeBed is null)
        {
            return NotFound(new
            {
                message = $"Nie znaleziono wolnego łóżka typu '{request.BedType}' na oddziale '{request.Ward}' w podanym terminie."
            });
        }

        var newAssignment = new BedAssignment
        {
            PatientPesel = pesel,
            BedId = freeBed.Id,
            From = request.From,
            To = request.To
        };

        _context.BedAssignments.Add(newAssignment);
        await _context.SaveChangesAsync();

        return Created(
            $"/api/patients/{pesel}/bedassignments/{newAssignment.Id}",
            new
            {
                id = newAssignment.Id,
                patientPesel = newAssignment.PatientPesel.Trim(),
                bedId = newAssignment.BedId,
                from = newAssignment.From,
                to = newAssignment.To
            }
        );
    }
}