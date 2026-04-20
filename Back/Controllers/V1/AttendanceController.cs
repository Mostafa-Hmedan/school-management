using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Back.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/attendances")]
[Authorize]
[Tags("Attendances")]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceServices _attendanceServices;
    private readonly IStudentServices _studentServices;

    public AttendanceController(IAttendanceServices attendanceServices, IStudentServices studentServices)
    {
        _attendanceServices = attendanceServices;
        _studentServices = studentServices;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [EndpointName("GetAllAttendances")]
    [EndpointSummary("Get all attendances")]
    [EndpointDescription("Retrieve a paginated list of all attendance records")]
    [ProducesResponseType(typeof(IEnumerable<AttendanceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1)
    {
        var pagenumber = Math.Max(pageNumber, 1);
        var pagesize = Math.Clamp(pageSize, 1, 100);
        var attendances = await _attendanceServices.GetAllAttendancesAsync(pagesize, pagenumber);

        return Ok(attendances);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointName("GetAttendanceById")]
    [EndpointSummary("Get attendance by ID")]
    [EndpointDescription("Retrieve a specific attendance record by its unique identifier")]
    [ProducesResponseType(typeof(AttendanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var attendance = await _attendanceServices.GetAttendanceByIdAsync(id);

        if (attendance is null)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"سجل الحضور برقم {id} غير موجود"
            );

        return Ok(attendance);
    }

    [HttpGet("by-student/{studentId:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointName("GetAttendancesByStudent")]
    [EndpointSummary("Get attendances by student")]
    [EndpointDescription("Retrieve all attendance records for a specific student")]
    [ProducesResponseType(typeof(IEnumerable<AttendanceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStudent(int studentId)
    {
        var attendances = await _attendanceServices.GetAttendanceByStudentAsync(studentId);
        return Ok(attendances);
    }

    [HttpGet("by-date/{date}")]
    [Authorize(Roles = "Admin")]
    [EndpointName("GetAttendancesByDate")]
    [EndpointSummary("Get attendances by date")]
    [EndpointDescription("Retrieve all attendance records for a specific date")]
    [ProducesResponseType(typeof(IEnumerable<AttendanceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDate(DateOnly date)
    {
        var attendances = await _attendanceServices.GetAttendanceByDateAsync(date);
        return Ok(attendances);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Student")]
    [EndpointName("GetMyAttendance")]
    [EndpointSummary("Get my attendance records")]
    [EndpointDescription("Returns all attendance records of the currently authenticated student")]
    [ProducesResponseType(typeof(IEnumerable<AttendanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyAttendance()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var student = await _studentServices.GetStudentByUserIdAsync(userId!);

        if (student is null)
            return Problem(title: "غير موجود", statusCode: StatusCodes.Status404NotFound,
                detail: "لم يتم العثور على بيانات الطالب");

        var attendances = await _attendanceServices.GetAttendanceByStudentAsync(student.Id);
        return Ok(attendances);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EndpointName("CreateAttendance")]
    [EndpointSummary("Create a new attendance record")]
    [EndpointDescription("Record a new attendance entry for a student")]
    [ProducesResponseType(typeof(AttendanceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAttendanceRequest request)
    {
        var attendance = await _attendanceServices.CreateAttendanceAsync(request);

        if (attendance is null)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                title: "فشل الإنشاء",
                statusCode: StatusCodes.Status400BadRequest,
                detail: "لم يتم إنشاء سجل الحضور، تحقق من البيانات"
            );

        return CreatedAtAction(nameof(GetById), new { id = attendance.AttendanceId }, attendance);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointName("UpdateAttendance")]
    [EndpointSummary("Update an attendance record")]
    [EndpointDescription("Update an existing attendance record")]
    [ProducesResponseType(typeof(AttendanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAttendanceRequest request)
    {
        var attendance = await _attendanceServices.UpdateAttendanceAsync(id, request);

        if (attendance is null)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"سجل الحضور برقم {id} غير موجود"
            );

        return Ok(attendance);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointName("DeleteAttendance")]
    [EndpointSummary("Delete an attendance record")]
    [EndpointDescription("Permanently delete an attendance record from the system")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _attendanceServices.DeleteAttendanceAsync(id);

        if (!result)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"سجل الحضور برقم {id} غير موجود"
            );

        return NoContent();
    }
}
