using Microsoft.AspNetCore.Mvc;
using WebApplication1.Service;
using WebApplication1.Exceptions;


namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController(AppointmentService service) : ControllerBase
{

    [HttpGet]
    [Route("{id:int}")]
    public async Task<IActionResult> GetAppointmentd([FromRoute] int id)
    {
        try
        {
            return Ok(await service.GetAppointmentDetails(id));
        }
        catch (NotFoundExcpetion e)
        {
            return NotFound(e.Message);
        }
            
    }

    [HttpGet]
    public async Task<IActionResult> GetAppointments([FromQuery] string? status, [FromQuery]string? nazwisko)
    {
        try
        {
            return Ok(await service.GetAppointmentList(status, nazwisko));
        }
        catch (NotFoundExcpetion e)
        {
            return NotFound(e.Message);
        }
    }
    
}