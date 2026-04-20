using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/subjects")]
[Authorize(Roles = "Admin")]
[Tags("Subjects")]
public class SubjectController : ControllerBase
{
    private readonly ISubjectServices _subjectServices;

    public SubjectController(ISubjectServices subjectServices)
    {
        _subjectServices = subjectServices;
    }

    [HttpGet]
    [EndpointName("GetAllSubjects")]
    [EndpointSummary("Get all subjects")]
    [EndpointDescription("Retrieve a paginated list of all subjects")]
    [ProducesResponseType(typeof(PageResult<SubjectResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1)
    {
        var pagenumber = Math.Max(pageNumber, 1);
        var pagesize = Math.Clamp(pageSize, 1, 100);
        var subjects = await _subjectServices.GetAllSubjectsAsync(pagesize, pagenumber);
        var count = await _subjectServices.SubjectCountAsync();

        var data = PageResult<SubjectResponse>.Create(subjects, count, pagenumber, pagesize);

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    [EndpointName("GetSubjectById")]
    [EndpointSummary("Get subject by ID")]
    [EndpointDescription("Retrieve a specific subject by its unique identifier")]
    [ProducesResponseType(typeof(SubjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var subject = await _subjectServices.GetSubjectByIdAsync(id);

        if (subject is null)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"المادة برقم {id} غير موجودة"
            );

        return Ok(subject);
    }

    [HttpGet("by-name/{name}")]
    [EndpointName("GetSubjectByName")]
    [EndpointSummary("Get subject by name")]
    [EndpointDescription("Retrieve a specific subject by its name")]
    [ProducesResponseType(typeof(SubjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByName(string name)
    {
        var subject = await _subjectServices.GetSubjectByNameAsync(name);

        if (subject is null)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"المادة '{name}' غير موجودة"
            );

        return Ok(subject);
    }

    [HttpGet("count")]
    [EndpointName("GetSubjectCount")]
    [EndpointSummary("Get total subject count")]
    [EndpointDescription("Retrieve the total number of subjects in the system")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount()
    {
        var count = await _subjectServices.SubjectCountAsync();
        return Ok(new { count });
    }

    [HttpPost]
    [EndpointName("CreateSubject")]
    [EndpointSummary("Create a new subject")]
    [EndpointDescription("Create a new subject with its name")]
    [ProducesResponseType(typeof(SubjectResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSubjectRequest request)
    {
        var subject = await _subjectServices.CreateSubjectAsync(request);

        if (subject is null)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                title: "فشل الإنشاء",
                statusCode: StatusCodes.Status400BadRequest,
                detail: "لم يتم إنشاء المادة، تحقق من البيانات"
            );

        return CreatedAtAction(nameof(GetById), new { id = subject.Id }, subject);
    }

    [HttpPut("{id:int}")]
    [EndpointName("UpdateSubject")]
    [EndpointSummary("Update a subject")]
    [EndpointDescription("Update an existing subject's information")]
    [ProducesResponseType(typeof(SubjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSubjectRequest request)
    {
        var subject = await _subjectServices.UpdateSubjectAsync(id, request);

        if (subject is null)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"المادة برقم {id} غير موجودة"
            );

        return Ok(subject);
    }

    [HttpDelete("{id:int}")]
    [EndpointName("DeleteSubject")]
    [EndpointSummary("Delete a subject")]
    [EndpointDescription("Permanently delete a subject from the system")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _subjectServices.DeleteSubjectAsync(id);

        if (!result)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"المادة برقم {id} غير موجودة"
            );

        return NoContent();
    }
}
