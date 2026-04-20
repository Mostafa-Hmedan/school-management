using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Utilities;

namespace Back.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/teacher-availability")]
[Authorize]
[Tags("Teacher Availability")]
public class TeacherAvailabilityController : ControllerBase
{
    private readonly ITeacherAvailabilityServices _availabilityServices;
    private readonly ITeacherServices _teacherServices;

    public TeacherAvailabilityController(
        ITeacherAvailabilityServices availabilityServices,
        ITeacherServices teacherServices)
    {
        _availabilityServices = availabilityServices;
        _teacherServices = teacherServices;
    }

    [HttpGet("{teacherId:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Get availability for a specific teacher")]
    [ProducesResponseType(typeof(PagedResponse<TeacherAvailabilityResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeacherAvailabilities(int teacherId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _availabilityServices.GetTeacherAvailabilities(teacherId, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Teacher")]
    [EndpointName("GetMyAvailability")]
    [EndpointSummary("Get my availability schedule")]
    [EndpointDescription("Returns the availability schedule of the currently authenticated teacher")]
    [ProducesResponseType(typeof(PagedResponse<TeacherAvailabilityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyAvailability([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var teacher = await _teacherServices.GetTeacherByUserIdAsync(userId!);

        if (teacher is null)
            return Problem(title: "غير موجود", statusCode: StatusCodes.Status404NotFound,
                detail: "لم يتم العثور على بيانات الأستاذ");

        var result = await _availabilityServices.GetTeacherAvailabilities(teacher.Id, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("detail/{id:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Get a specific availability record by ID")]
    [ProducesResponseType(typeof(TeacherAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _availabilityServices.GetTeacherAvailability(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Create a new availability record for a teacher")]
    [ProducesResponseType(typeof(TeacherAvailabilityResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateTeacherAvailabilityRequest request)
    {
        var result = await _availabilityServices.CreateTeacherAvailability(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Update an existing availability record")]
    [ProducesResponseType(typeof(TeacherAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateTeacherAvailabilityRequest request)
    {
        var result = await _availabilityServices.UpdateTeacherAvailability(id, request);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Delete an availability record")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _availabilityServices.DeleteTeacherAvailability(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
