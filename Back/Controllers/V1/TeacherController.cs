using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Back.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/teachers")]
[Authorize(Roles = "Admin")]
[Tags("Teachers")]
public class TeacherController : ControllerBase
{
    private readonly ITeacherServices _teacherServices;

    public TeacherController(ITeacherServices teacherServices)
    {
        _teacherServices = teacherServices;
    }

    [HttpGet]
    [EndpointName("GetAllTeachers")]
    [EndpointSummary("Get all teachers")]
    [EndpointDescription("Retrieve a paginated list of all teachers")]
    [ProducesResponseType(typeof(PageResult<TeacherResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1)
    {
        var pagenumber = Math.Max(pageNumber, 1);
        var pagesize = Math.Clamp(pageSize, 1, 100);
        var teachers = await _teacherServices.GetAllTeacherAsync(pagesize, pagenumber);
        var count = await _teacherServices.TeacherCountAsync();

        var data = PageResult<TeacherResponse>.Create(teachers, count, pagenumber, pagesize);

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    [EndpointName("GetTeacherById")]
    [EndpointSummary("Get teacher by ID")]
    [EndpointDescription("Retrieve a specific teacher by their unique identifier")]
    [ProducesResponseType(typeof(TeacherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var teacher = await _teacherServices.GetTeacherByIdAsync(id);

        if (teacher is null)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"الأستاذ برقم {id} غير موجود"
            );

        return Ok(teacher);
    }

    [HttpGet("by-name/{name}")]
    [EndpointName("GetTeachersByName")]
    [EndpointSummary("Search teachers by name")]
    [EndpointDescription("Retrieve teachers matching the given name")]
    [ProducesResponseType(typeof(IEnumerable<TeacherResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByName(string name)
    {
        var teachers = await _teacherServices.GetTeacherByNameAsync(name);
        return Ok(teachers);
    }

    [HttpGet("by-class/{className}")]
    [EndpointName("GetTeachersByClass")]
    [EndpointSummary("Get teachers by class")]
    [EndpointDescription("Retrieve all teachers assigned to a specific class")]
    [ProducesResponseType(typeof(IEnumerable<TeacherResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByClass(string className)
    {
        var teachers = await _teacherServices.GetTeacherByClassAsync(className);
        return Ok(teachers);
    }

    [HttpGet("by-subject/{subjectName}")]
    [EndpointName("GetTeachersBySubject")]
    [EndpointSummary("Get teachers by subject")]
    [EndpointDescription("Retrieve all teachers teaching a specific subject")]
    [ProducesResponseType(typeof(IEnumerable<TeacherResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySubject(string subjectName)
    {
        var teachers = await _teacherServices.GetTeacherBySubjectAsync(subjectName);
        return Ok(teachers);
    }

    [HttpGet("count")]
    [EndpointName("GetTeacherCount")]
    [EndpointSummary("Get total teacher count")]
    [EndpointDescription("Retrieve the total number of teachers in the system")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount()
    {
        var count = await _teacherServices.TeacherCountAsync();
        return Ok(new { count });
    }




    [HttpPost]
    [EndpointName("CreateTeacher")]
    [EndpointSummary("Create a new teacher")]
    [EndpointDescription("Create a new teacher with their profile information and user account")]
    [ProducesResponseType(typeof(TeacherResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromForm] CreateTeacherRequest request)
    {
        try 
        {
            var teacher = await _teacherServices.CreateTeacherAsync(request);

            if (teacher is null)
                return Problem(
                    type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    title: "فشل الإنشاء",
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: "لم يتم إنشاء الأستاذ، تحقق من البيانات"
                );

            return CreatedAtAction(nameof(GetById), new { id = teacher.Id }, teacher);
        }
        catch (Exception ex)
        {
            return Problem(
                title: "خطأ داخلي في الخادم",
                statusCode: StatusCodes.Status500InternalServerError,
                detail: ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
            );
        }
    }

    [HttpPut("{id:int}")]
    [EndpointName("UpdateTeacher")]
    [EndpointSummary("Update a teacher")]
    [EndpointDescription("Update an existing teacher's profile information")]
    [ProducesResponseType(typeof(TeacherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateTeacherRequest request)
    {
        try {
            var teacher = await _teacherServices.UpdateTeacherAsync(id, request);

            if (teacher is null)
                return Problem(
                    type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    title: "غير موجود",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"الأستاذ برقم {id} غير موجود"
                );

            return Ok(teacher);
        } 
        catch (Exception ex)
        {
            return Problem(
                title: "خطأ داخلي",
                statusCode: StatusCodes.Status500InternalServerError,
                detail: ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : "")
            );
        }
    }

    [HttpDelete("{id:int}")]
    [EndpointName("DeleteTeacher")]
    [EndpointSummary("Delete a teacher")]
    [EndpointDescription("Permanently delete a teacher and their associated user account")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _teacherServices.DeleteTeacherAsync(id);

        if (!result)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"الأستاذ برقم {id} غير موجود"
            );

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize(Roles = "Teacher")]
    [EndpointName("GetMyTeacherProfile")]
    [EndpointSummary("Get current teacher profile")]
    [EndpointDescription("Returns the profile of the currently authenticated teacher")]
    [ProducesResponseType(typeof(TeacherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var teacher = await _teacherServices.GetTeacherByUserIdAsync(userId!);

        if (teacher is null)
            return Problem(
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: "لم يتم العثور على بيانات الأستاذ"
            );

        return Ok(teacher);
    }
}
