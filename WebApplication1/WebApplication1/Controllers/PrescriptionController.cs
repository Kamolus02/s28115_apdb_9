using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Context;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrescriptionsController : ControllerBase
{
    private readonly HospitalDbContext _context;

    public PrescriptionsController(HospitalDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> AddPrescription([FromBody] AddPrescriptionRequest request)
    {
        if (request.Medicaments.Count > 10)
        {
            return BadRequest("A prescription can include a maximum of 10 medicaments.");
        }

        if (request.DueDate < request.Date)
        {
            return BadRequest("DueDate must be greater than or equal to Date.");
        }

        var patient = await _context.Patients.FindAsync(request.PatientId) ?? new Patient
        {
            FirstName = request.PatientFirstName,
            LastName = request.PatientLastName,
            BirthDate = request.BirthDate
        };

        if (patient.IdPatient == 0)
        {
            _context.Patients.Add(patient);
        }

        var doctor = await _context.Doctors.FindAsync(request.DoctorId);
        if (doctor == null)
        {
            return NotFound("Doctor not found.");
        }

        var prescription = new Prescription
        {
            Date = request.Date,
            DueDate = request.DueDate,
            Doctor = doctor,
            Patient = patient,
            PrescriptionMedicaments = new List<Prescription_Medicament>()
        };

        foreach (var medDto in request.Medicaments)
        {
            var medicament = await _context.Medicaments.FindAsync(medDto.MedicamentId);
            if (medicament == null)
            {
                return NotFound($"Medicament with ID {medDto.MedicamentId} not found.");
            }

            prescription.PrescriptionMedicaments.Add(new Prescription_Medicament
            {
                Medicament = medicament,
                Details = medDto.Details
            });
        }

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();

        return Ok(prescription);
    }
    
    [HttpGet("{patientId}")]
    public async Task<IActionResult> GetPatientDetails(int patientId)
    {
        var patient = await _context.Patients
            .Include(p => p.Prescriptions)
            .ThenInclude(pr => pr.PrescriptionMedicaments)
            .ThenInclude(pm => pm.Medicament)
            .Include(p => p.Prescriptions)
            .ThenInclude(pr => pr.Doctor)
            .FirstOrDefaultAsync(p => p.IdPatient == patientId);

        if (patient == null)
        {
            return NotFound("Patient not found.");
        }

        var response = new
        {
            patient.IdPatient,
            patient.FirstName,
            patient.LastName,
            patient.BirthDate,
            Prescriptions = patient.Prescriptions.Select(prescription => new
            {
                prescription.IdPrescription,
                prescription.Date,
                prescription.DueDate,
                Doctor = new
                {
                    prescription.Doctor.IdDoctor,
                    prescription.Doctor.FirstName,
                    prescription.Doctor.LastName
                },
                Medicaments = prescription.PrescriptionMedicaments.Select(pm => new
                {
                    pm.Medicament.IdMedicament,
                    pm.Medicament.Name,
                    pm.Details
                })
            }).OrderBy(p => p.DueDate)
        };

        return Ok(response);
    }

}
