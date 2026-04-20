using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Back.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/grades")]
[Authorize]
[Tags("Grades")]
public class GradeController : ControllerBase
{
    private readonly IGradeServices _gradeServices;
    private readonly IStudentServices _studentServices;
    private readonly ITeacherServices _teacherServices;

    public GradeController(IGradeServices gradeServices, IStudentServices studentServices, ITeacherServices teacherServices)
    {
        _gradeServices = gradeServices;
        _studentServices = studentServices;
        _teacherServices = teacherServices;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [EndpointName("GetAllGrades")]
    [EndpointSummary("Get all grades")]
    [EndpointDescription("Retrieve a paginated list of all grade records")]
    [ProducesResponseType(typeof(IEnumerable<GradeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1)
    {
        var pagenumber = Math.Max(pageNumber, 1);
        var pagesize = Math.Clamp(pageSize, 1, 100);
        var grades = await _gradeServices.GetAllGradesAsync(pagesize, pagenumber);

        return Ok(grades);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointName("GetGradeById")]
    [EndpointSummary("Get grade by ID")]
    [EndpointDescription("Retrieve a specific grade record by its unique identifier")]
    [ProducesResponseType(typeof(GradeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var grade = await _gradeServices.GetGradeByIdAsync(id);

        if (grade is null)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"الدرجة برقم {id} غير موجودة"
            );

        return Ok(grade);
    }

    [HttpGet("by-student/{studentId:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointName("GetGradesByStudent")]
    [EndpointSummary("Get grades by student")]
    [EndpointDescription("Retrieve all grade records for a specific student")]
    [ProducesResponseType(typeof(IEnumerable<GradeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStudent(int studentId)
    {
        var grades = await _gradeServices.GetGradesByStudentAsync(studentId);
        return Ok(grades);
    }

    [HttpGet("by-subject/{subjectId:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointName("GetGradesBySubject")]
    [EndpointSummary("Get grades by subject")]
    [EndpointDescription("Retrieve all grade records for a specific subject")]
    [ProducesResponseType(typeof(IEnumerable<GradeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySubject(int subjectId)
    {
        var grades = await _gradeServices.GetGradesBySubjectAsync(subjectId);
        return Ok(grades);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Student")]
    [EndpointName("GetMyGrades")]
    [EndpointSummary("Get my grades")]
    [EndpointDescription("Returns all grades of the currently authenticated student")]
    [ProducesResponseType(typeof(IEnumerable<GradeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyGrades()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var student = await _studentServices.GetStudentByUserIdAsync(userId!);

        if (student is null)
            return Problem(title: "غير موجود", statusCode: StatusCodes.Status404NotFound,
                detail: "لم يتم العثور على بيانات الطالب");

        var grades = await _gradeServices.GetGradesByStudentAsync(student.Id);
        return Ok(grades);
    }

    [HttpGet("by-teacher/{teacherId:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointName("GetGradesByTeacher")]
    [EndpointSummary("Get grades by teacher")]
    [EndpointDescription("Retrieve all grade records submitted by a specific teacher")]
    [ProducesResponseType(typeof(IEnumerable<GradeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTeacher(int teacherId)
    {
        var grades = await _gradeServices.GetGradesByTeacherAsync(teacherId);
        return Ok(grades);
    }

    [HttpGet("my-given")]
    [Authorize(Roles = "Teacher")]
    [EndpointName("GetMyGivenGrades")]
    [EndpointSummary("Get grades given by me (Teacher)")]
    [EndpointDescription("Returns all grades recorded by the currently authenticated teacher")]
    [ProducesResponseType(typeof(IEnumerable<GradeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyGivenGrades()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var teacher = await _teacherServices.GetTeacherByUserIdAsync(userId!);

        if (teacher is null)
            return Problem(title: "غير موجود", statusCode: StatusCodes.Status404NotFound,
                detail: "لم يتم العثور على بيانات الأستاذ");

        var grades = await _gradeServices.GetGradesByTeacherAsync(teacher.Id);
        return Ok(grades);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EndpointName("CreateGrade")]
    [EndpointSummary("Create a new grade")]
    [EndpointDescription("Record a new grade for a student in a specific subject")]
    [ProducesResponseType(typeof(GradeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateGradeRequest request)
    {
        var grade = await _gradeServices.CreateGradeAsync(request);

        if (grade is null)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                title: "فشل الإنشاء",
                statusCode: StatusCodes.Status400BadRequest,
                detail: "لم يتم إنشاء الدرجة، تحقق من البيانات"
            );

        return CreatedAtAction(nameof(GetById), new { id = grade.Id }, grade);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointName("UpdateGrade")]
    [EndpointSummary("Update a grade")]
    [EndpointDescription("Update an existing grade record")]
    [ProducesResponseType(typeof(GradeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGradeRequest request)
    {
        var grade = await _gradeServices.UpdateGradeAsync(id, request);

        if (grade is null)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"الدرجة برقم {id} غير موجودة"
            );

        return Ok(grade);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EndpointName("DeleteGrade")]
    [EndpointSummary("Delete a grade")]
    [EndpointDescription("Permanently delete a grade record from the system")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _gradeServices.DeleteGradeAsync(id);

        if (!result)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"الدرجة برقم {id} غير موجودة"
            );

        return NoContent();
    }
}
