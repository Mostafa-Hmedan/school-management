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
[Route("api/v{version:apiVersion}/timetable")]
[Authorize] // Authenticated users can view, but only Admin can edit
[Tags("Timetable")]
public class TimetableController : ControllerBase
{
    private readonly IClassScheduleServices _scheduleServices;
    private readonly IStudentServices _studentServices;
    private readonly ITeacherServices _teacherServices;

    public TimetableController(
        IClassScheduleServices scheduleServices,
        IStudentServices studentServices,
        ITeacherServices teacherServices)
    {
        _scheduleServices = scheduleServices;
        _studentServices = studentServices;
        _teacherServices = teacherServices;
    }

    [HttpGet("class/{classId:int}")]
    [EndpointSummary("Get all schedule for a specific class")]
    [ProducesResponseType(typeof(PagedResponse<ClassScheduleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByClass(int classId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
    {
        var result = await _scheduleServices.GetSchedulesByClass(classId, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("teacher/{teacherId:int}")]
    [EndpointSummary("Get schedule for a specific teacher")]
    [ProducesResponseType(typeof(PagedResponse<ClassScheduleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTeacher(int teacherId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
    {
        var result = await _scheduleServices.GetSchedulesByTeacher(teacherId, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("Get a specific schedule record by ID")]
    [ProducesResponseType(typeof(ClassScheduleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _scheduleServices.GetSchedule(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Student: جدول الفصل / Teacher: جدوله الخاص</summary>
    [HttpGet("me")]
    [Authorize(Roles = "Student,Teacher")]
    [EndpointName("GetMyTimetable")]
    [EndpointSummary("Get my timetable")]
    [EndpointDescription("Student gets class schedule. Teacher gets their own teaching schedule.")]
    [ProducesResponseType(typeof(PagedResponse<ClassScheduleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyTimetable(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        if (roles.Contains("Student"))
        {
            var student = await _studentServices.GetStudentByUserIdAsync(userId!);
            if (student is null)
                return Problem(title: "غير موجود", statusCode: StatusCodes.Status404NotFound,
                    detail: "لم يتم العثور على بيانات الطالب");

            if (student.ClassId == 0)
                return Problem(title: "غير مسجل", statusCode: StatusCodes.Status404NotFound,
                    detail: "الطالب غير مسجل في أي فصل");

            var result = await _scheduleServices.GetSchedulesByClass(
                student.ClassId, pageNumber, pageSize);
            return Ok(result);
        }
        else // Teacher
        {
            var teacher = await _teacherServices.GetTeacherByUserIdAsync(userId!);
            if (teacher is null)
                return Problem(title: "غير موجود", statusCode: StatusCodes.Status404NotFound,
                    detail: "لم يتم العثور على بيانات الأستاذ");

            var result = await _scheduleServices.GetSchedulesByTeacher(
                teacher.Id, pageNumber, pageSize);
            return Ok(result);
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Add a new session/lesson to a class schedule")]
    [ProducesResponseType(typeof(ClassScheduleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateClassScheduleRequest request)
    {
        try
        {
            var result = await _scheduleServices.CreateSchedule(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                title: "تضارب في الجدول",
                statusCode: StatusCodes.Status400BadRequest,
                detail: ex.Message
            );
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Update a schedule record")]
    [ProducesResponseType(typeof(ClassScheduleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateClassScheduleRequest request)
    {
        try
        {
            var result = await _scheduleServices.UpdateSchedule(id, request);
            if (result == null) return NotFound();
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                title: "تضارب في الجدول",
                statusCode: StatusCodes.Status400BadRequest,
                detail: ex.Message
            );
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Delete a schedule record")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _scheduleServices.DeleteSchedule(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
