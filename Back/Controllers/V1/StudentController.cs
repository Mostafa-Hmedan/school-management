using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Back.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/students")]
[Authorize(Roles = "Admin")]
[Tags("Students")]
public class StudentController : ControllerBase
{
    private readonly IStudentServices _studentServices;

    public StudentController(IStudentServices studentServices)
    {
        _studentServices = studentServices;
    }

    [HttpGet]
    [EndpointName("GetAllStudents")]
    [EndpointSummary("Get all students")]
    [EndpointDescription("Retrieve a paginated list of all students")]
    [ProducesResponseType(typeof(PageResult<StudentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1)
    {
        var pagenumber = Math.Max(pageNumber, 1);
        var pagesize = Math.Clamp(pageSize, 1, 100);
        var students = await _studentServices.GetAllStudentAsync(pageSize, pageNumber);
        var count = await _studentServices.StudentCount();
        var data = PageResult<StudentResponse>.Create(students, count, pagenumber, pagesize);

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    [EndpointName("GetStudentById")]
    [EndpointSummary("Get student by ID")]
    [EndpointDescription("Retrieve a specific student by their unique identifier")]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var student = await _studentServices.GetStudentByIdAsync(id);

        if (student is null)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"الطالب برقم {id} غير موجود"
            );

        return Ok(student);
    }

    [HttpGet("by-name/{name}")]
    [EndpointName("GetStudentsByName")]
    [EndpointSummary("Search students by name")]
    [EndpointDescription("Retrieve students matching the given name")]
    [ProducesResponseType(typeof(IEnumerable<StudentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByName(string name)
    {
        var students = await _studentServices.GetStudentByNameAsync(name);
        return Ok(students);
    }

    [HttpGet("by-class/{className}")]
    [EndpointName("GetStudentsByClass")]
    [EndpointSummary("Get students by class")]
    [EndpointDescription("Retrieve all students belonging to a specific class")]
    [ProducesResponseType(typeof(IEnumerable<StudentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByClass(string className)
    {
        var students = await _studentServices.GetStudentByClassAsync(className);
        return Ok(students);
    }

    [HttpGet("count")]
    [EndpointName("GetStudentCount")]
    [EndpointSummary("Get total student count")]
    [EndpointDescription("Retrieve the total number of students in the system")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount()
    {
        var count = await _studentServices.StudentCount();
        return Ok(new { count });
    }


    [HttpPost]
    [EndpointName("CreateStudent")]
    [EndpointSummary("Create a new student")]
    [EndpointDescription("Create a new student with their profile information and user account")]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromForm] CreateStudentRequest request)
    {
        try
        {
            var student = await _studentServices.CreateStudentAsync(request);

            if (student is null)
                return Problem(
                    type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    title: "فشل الإنشاء",
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: "لم يتم إنشاء الطالب، تحقق من البيانات"
                );

            return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                title: "بيانات غير صالحة",
                statusCode: StatusCodes.Status400BadRequest,
                detail: ex.Message
            );
        }
    }

    [HttpPut("{id:int}")]
    [EndpointName("UpdateStudent")]
    [EndpointSummary("Update a student")]
    [EndpointDescription("Update an existing student's profile information")]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateStudentRequest request)
    {
        var student = await _studentServices.UpdateStudentAsync(id, request);

        if (student is null)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"الطالب برقم {id} غير موجود"
            );

        return Ok(student);
    }

    [HttpDelete("{id:int}")]
    [EndpointName("DeleteStudent")]
    [EndpointSummary("Delete a student")]
    [EndpointDescription("Permanently delete a student and their associated user account")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _studentServices.DeleteStudentAsync(id);

        if (!result)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"الطالب برقم {id} غير موجود"
            );

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize(Roles = "Student")]
    [EndpointName("GetMyStudentProfile")]
    [EndpointSummary("Get current student profile")]
    [EndpointDescription("Returns the profile of the currently authenticated student")]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var student = await _studentServices.GetStudentByUserIdAsync(userId!);

        if (student is null)
            return Problem(
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: "لم يتم العثور على بيانات الطالب"
            );

        return Ok(student);
    }
}
