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
            
            command.Parameters.Clear();

            if (list is null)
            {
                throw new NotFoundExcpetion("Appointment not found");
            }
                
            return list;
            
        }

        public async Task AddAppointment(CreateAppointmentRequestDto createDto)
        {
            await using var connection = new SqlConnection(config.GetConnectionString("Default"));
            await using var command = new SqlCommand();
            
            await connection.OpenAsync();

            command.Connection = connection;

            command.CommandText = """
                                    select 1 from doctors where IdDoctor = @idDoctor
                                  """;
            command.Parameters.AddWithValue("@idDoctor", createDto.IdDoctor);
            
            var doctorExists = await command.ExecuteScalarAsync();
            if (doctorExists is null)
            {
                throw new NotFoundExcpetion("Doctor not found");
            }
            
            command.Parameters.Clear();
            
            command.CommandText = """
                                    select 1 from patients where IdPatient = @idPatient
                                  """;
            command.Parameters.AddWithValue("@idPatient", createDto.IdPatient);
            
            var patientExists = await command.ExecuteScalarAsync();
            if (patientExists is null)
            {
                throw new NotFoundExcpetion("Patient not found");
            }
            
            command.Parameters.Clear();

            if (createDto.AppointmentDate < DateTime.Now)
            {
                throw new NotFoundExcpetion("cant schedule appointment in the past");
            }
            
            await using var transaction = await connection.BeginTransactionAsync();
            command.Transaction = (SqlTransaction)transaction;

            try
            {
                command.CommandText = """
                                      insert into Appointments (IdPatient, IdDoctor, AppointmentDate, Status, Reason, CreatedAt)
                                      values (@idPatient, @idDoctor,@appointmentDate, 'Scheduled',  @reason, @createdAt)
                                      """;
                command.Parameters.AddWithValue("@idPatient", createDto.IdPatient);
                command.Parameters.AddWithValue("@idDoctor", createDto.IdDoctor);
                command.Parameters.AddWithValue("@appointmentDate", createDto.AppointmentDate);
                command.Parameters.AddWithValue("@reason", createDto.Reason);
                command.Parameters.AddWithValue("@createdAt", DateTime.Now);
                
                await command.ExecuteNonQueryAsync();
                command.Parameters.Clear();
                
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }

        }

        public async Task UpdateAppointment(UpdateAppointmentRequestDto updateDto)
        {
            await using var connection = new SqlConnection(config.GetConnectionString("Default"));
            await using var command = new SqlCommand();
            
            await connection.OpenAsync();

            command.Connection = connection;
            
            command.CommandText = """
                                    select 1 from appointments where IdAppointment = @idAppointment
                                  """;
            command.Parameters.AddWithValue("@idAppointment", updateDto.IdAppointment);
            
            var appointmentExists = await command.ExecuteScalarAsync();
            if (appointmentExists is null)
            {
                throw new NotFoundExcpetion("Doctor not found");
            }
            
            command.Parameters.Clear();

            
            command.CommandText = """
                                    select 1 from doctors where IdDoctor = @idDoctor and isActive = 1;
                                  """;
            command.Parameters.AddWithValue("@idDoctor", updateDto.IdDoctor);
            
            var doctorExists = await command.ExecuteScalarAsync();
            if (doctorExists is null)
            {
                throw new NotFoundExcpetion("Doctor not found");
            }
            
            command.Parameters.Clear();
            
            command.CommandText = """
                                    select 1 from patients where IdPatient = @idPatient and isActive = 1;
                                  """;
            command.Parameters.AddWithValue("@idPatient", updateDto.IdPatient);
            
            var patientExists = await command.ExecuteScalarAsync();
            if (patientExists is null)
            {
                throw new NotFoundExcpetion("Patient not found");
            }
            
            command.Parameters.Clear();
            
            await using var transaction = await connection.BeginTransactionAsync();
            command.Transaction = (SqlTransaction)transaction;

            try
            {
                command.CommandText = """
                                      update Appointments set idPatient = @idPatient, idDoctor = @idDoctor, 
                                                              appointmentDate = @appointmentDate, status = @status,
                                                              reason = @reason, InternalNotes = @internalNotes
                                      """;
            
                command.Parameters.AddWithValue("@idDoctor", updateDto.IdDoctor);
                command.Parameters.AddWithValue("@idPatient", updateDto.IdPatient);
                command.Parameters.AddWithValue("@appointmentDate", updateDto.AppointmentDate);
                command.Parameters.AddWithValue("@status", updateDto.Status);
                command.Parameters.AddWithValue("@reason", updateDto.Reason);
                command.Parameters.AddWithValue("@internalNotes", updateDto.InternalNotes);
                
                await command.ExecuteNonQueryAsync();
                command.Parameters.Clear();
                
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
            


        }
}