using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTO;
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

    [HttpPost]
    public async Task<IActionResult> PostAppoinment([FromBody] CreateAppointmentRequestDto request)
    {
        try
        {
            await service.AddAppointment(request);
            return Created();
        }
        catch (NotFoundExcpetion e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPut]
    [Route("{id:int}")]
    public async Task<IActionResult> PutAppointment([FromRoute] int id, [FromBody] UpdateAppointmentRequestDto request)
    {
        try
        {
            await service.UpdateAppointment(request);
            return Ok();
        }
        catch (NotFoundExcpetion)
        {
            return NotFound();
        }
    }

    [HttpDelete]
    [Route("{id:int}")]
    public async Task<IActionResult> DeleteAppointment([FromRoute] int id)
    {
        try
        {
            await service.DeleteAppointment(id);
            return Ok();
        }
        catch (NotFoundExcpetion e)
        {
            return NotFound(e.Message);
        }
    }
    
}