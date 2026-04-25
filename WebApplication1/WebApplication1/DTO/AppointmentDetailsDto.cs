namespace WebApplication1.DTO;

public class AppointmentDetailsDto
{
    public int Id { get; set; }
    public string PatientFullName { get; set; }
    public DateTime PatientBirthDate { get; set; }
    public int DoctorsFullName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; }
    public string Reason { get; set; }
    public string InternalNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    
}