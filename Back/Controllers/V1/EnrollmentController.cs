using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/enrollments")]
[Authorize(Roles = "Admin")]
[Tags("Enrollments")]
public class EnrollmentController : ControllerBase
{
    private readonly IEnrollmentServices _enrollmentServices;

    public EnrollmentController(IEnrollmentServices enrollmentServices)
    {
        _enrollmentServices = enrollmentServices;
    }

    [HttpGet]
    [EndpointName("GetAllEnrollments")]
    [EndpointSummary("Get all enrollments")]
    [EndpointDescription("Retrieve a paginated list of all student enrollments")]
    [ProducesResponseType(typeof(PageResult<EnrollmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1)
    {
        var pagenumber = Math.Max(pageNumber, 1);
        var pagesize = Math.Clamp(pageSize, 1, 100);
        var enrollments = await _enrollmentServices.GetAllEnrollmentsAsync(pagesize, pagenumber);
        var count = await _enrollmentServices.EnrollmentCountAsync();

        var data = PageResult<EnrollmentResponse>.Create(enrollments, count, pagenumber, pagesize);

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    [EndpointName("GetEnrollmentById")]
    [EndpointSummary("Get enrollment by ID")]
    [EndpointDescription("Retrieve a specific enrollment by its unique identifier")]
    [ProducesResponseType(typeof(EnrollmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var enrollment = await _enrollmentServices.GetEnrollmentByIdAsync(id);

        if (enrollment is null)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"التسجيل برقم {id} غير موجود"
            );

        return Ok(enrollment);
    }

    [HttpGet("by-student/{studentId:int}")]
    [EndpointName("GetEnrollmentsByStudent")]
    [EndpointSummary("Get enrollments by student")]
    [EndpointDescription("Retrieve all enrollments for a specific student")]
    [ProducesResponseType(typeof(IEnumerable<EnrollmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStudent(int studentId)
    {
        var enrollments = await _enrollmentServices.GetEnrollmentsByStudentAsync(studentId);
        return Ok(enrollments);
    }

    [HttpGet("by-class/{classId:int}")]
    [EndpointName("GetEnrollmentsByClass")]
    [EndpointSummary("Get enrollments by class")]
    [EndpointDescription("Retrieve all enrollments for a specific class")]
    [ProducesResponseType(typeof(IEnumerable<EnrollmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByClass(int classId)
    {
        var enrollments = await _enrollmentServices.GetEnrollmentsByClassAsync(classId);
        return Ok(enrollments);
    }

    [HttpGet("by-subject/{subjectId:int}")]
    [EndpointName("GetEnrollmentsBySubject")]
    [EndpointSummary("Get enrollments by subject")]
    [EndpointDescription("Retrieve all enrollments for a specific subject")]
    [ProducesResponseType(typeof(IEnumerable<EnrollmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySubject(int subjectId)
    {
        var enrollments = await _enrollmentServices.GetEnrollmentsBySubjectAsync(subjectId);
        return Ok(enrollments);
    }

    [HttpPost]
    [EndpointName("CreateEnrollment")]
    [EndpointSummary("Create a new enrollment")]
    [EndpointDescription("Enroll a student in a subject within a specific class")]
    [ProducesResponseType(typeof(EnrollmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        var enrollment = await _enrollmentServices.CreateEnrollmentAsync(request);

        if (enrollment is null)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                title: "فشل الإنشاء",
                statusCode: StatusCodes.Status400BadRequest,
                detail: "لم يتم إنشاء التسجيل، تحقق من البيانات"
            );

        return CreatedAtAction(nameof(GetById), new { id = enrollment.Id }, enrollment);
    }

    [HttpDelete("{id:int}")]
    [EndpointName("DeleteEnrollment")]
    [EndpointSummary("Delete an enrollment")]
    [EndpointDescription("Permanently remove a student's enrollment from a subject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _enrollmentServices.DeleteEnrollmentAsync(id);

        if (!result)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"التسجيل برقم {id} غير موجود"
            );

        return NoContent();
    }
}
