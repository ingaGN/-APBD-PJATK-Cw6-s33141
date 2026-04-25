using Microsoft.Data.SqlClient;
using WebApplication1.DTO;
using WebApplication1.Exceptions;

namespace WebApplication1.Service;

public class AppointmentService(IConfiguration config)
{
        public async Task<AppointmentDetailsDto> GetAppointmentDetails(int appId)
        {
                AppointmentDetailsDto? details = null;
                
                await using var connection = new SqlConnection(config.GetConnectionString("Default"));
                await using var command = new SqlCommand();
                
                await connection.OpenAsync();

                command.Connection = connection;

                command.CommandText = """
                                      select a.IdAppointment, p.FirstName as PatientFirstName, p.LastName as PatientLastName, p.DateOfBirth, 
                                             d.FirstName as DoctorsFirstName, d.LastName as DoctorsLastName,
                                             a.AppointmentDate, a.Status, a.Reason, A.InternalNotes, A.CreatedAt
                                      from Appointments A 
                                      left join Patients P on a.IdPatient = p.IdPatient
                                      left join Doctors D on a.IdDoctor = d.IdDoctor
                                      where a.IdAppointment = @idAppointment
                                      """;
                
                command.Parameters.AddWithValue("@idAppointment", appId);
                
                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    details = new AppointmentDetailsDto
                    {
                        PatientFirstName =  (string)reader["PatientFirstName"],
                        PatientLastName = (string)reader["PatientLastName"],
                        PatientBirthDate = (DateTime)reader["DateOfBirth"],
                        DoctorsFirstName = (string)reader["DoctorsFirstName"],
                        DoctorsLastName =  (string)reader["DoctorsLastName"],
                        AppointmentDate =  (DateTime)reader["AppointmentDate"],
                        Status = (string)reader["Status"],
                        Reason = (string)reader["Reason"],
                        InternalNotes = reader["InternalNotes"] ==  DBNull.Value 
                            ? null 
                            : (string)reader["InternalNotes"],
                        CreatedAt =  (DateTime)reader["CreatedAt"]
                        
                    };
                    
                }

                if (details is null)
                {
                    throw new NotFoundExcpetion("Appointment not found");
                }
                
                return details;
        }

        public async Task<List<AppointmentListDto>> GetAppointmentList(string? status, string? nazwisko)
        {
            var list = new List<AppointmentListDto>();
                
            await using var connection = new SqlConnection(config.GetConnectionString("Default"));
            await using var command = new SqlCommand();
            
            await connection.OpenAsync();

            command.Connection = connection;

            command.CommandText = """
                                        select a.IdAppointment, a.AppointmentDate, a.Status, a.Reason, concat(p.FirstName,' ', p.LastName) as Pacjent
                                  from Appointments A 
                                  left join Patients P on a.IdPatient = p.IdPatient
                                  WHERE (@Status IS NULL OR a.Status = @Status)
                                  AND (@PatientLastName IS NULL OR p.LastName = @PatientLastName)
                                  
                                  """;
            command.Parameters.AddWithValue("@Status", (object?)status ?? DBNull.Value);
            command.Parameters.AddWithValue("@PatientLastName", (object?)nazwisko ?? DBNull.Value);

            
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                
                list.Add( new AppointmentListDto
                {
                    IdAppointment =  (int)reader["IdAppointment"],
                    AppointmentDate =  (DateTime)reader["AppointmentDate"],
                    Status = (string)reader["Status"],
                    Reason = (string)reader["Reason"],
                    PatientFullName = (string)reader["Pacjent"]
                        
                });
                
                
                    
            }

            if (list is null)
            {
                throw new NotFoundExcpetion("Appointment not found");
            }
                
            return list;
            
        }
}