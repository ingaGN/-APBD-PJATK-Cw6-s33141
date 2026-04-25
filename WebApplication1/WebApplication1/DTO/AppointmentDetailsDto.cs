namespace WebApplication1.DTO;

public class AppointmentDetailsDto
{
    public int Id { get; set; }
    public string PatientFirstName { get; set; }
    public string PatientLastName { get; set; }
    public DateTime PatientBirthDate { get; set; }
    public string DoctorsFirstName { get; set; }
    public string DoctorsLastName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; }
    public string Reason { get; set; }
    public string InternalNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    
}