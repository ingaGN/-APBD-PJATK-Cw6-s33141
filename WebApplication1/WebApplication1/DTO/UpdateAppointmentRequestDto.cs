using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTO;

public class UpdateAppointmentRequestDto
{
    [Required] public int IdAppointment { get; set; }
    [Required] public int IdPatient { get; set; }
    [Required] public int IdDoctor { get; set; }
    [Required] public DateTime AppointmentDate { get; set; }
    [Required] public string Status { get; set; }
    [Required] public string Reason { get; set; }
    [Required] public string InternalNotes { get; set; }
    
}