namespace WebApplication1.Models;

public class AddPrescriptionRequest
{
    public int? PatientId { get; set; }
    public string PatientFirstName { get; set; }
    public string PatientLastName { get; set; }
    public DateTime BirthDate { get; set; }
    public int DoctorId { get; set; }
    public List<MedicamentDto> Medicaments { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
}