using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/classes")]
[Authorize(Roles = "Admin")]
[Tags("Classes")]
public class ClassController : ControllerBase
{
    private readonly IClassServices _classServices;

    public ClassController(IClassServices classServices)
    {
        _classServices = classServices;
    }

    [HttpGet]
    [EndpointName("GetAllClasses")]
    [EndpointSummary("Get all classes")]
    [EndpointDescription("Retrieve a paginated list of all classes")]
    [ProducesResponseType(typeof(PageResult<ClassResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1)
    {
        var pagenumber = Math.Max(pageNumber, 1);
        var pagesize = Math.Clamp(pageSize, 1, 100);
        var classes = await _classServices.GetAllClassesAsync(pagesize, pagenumber);
        var count = await _classServices.ClassCountAsync();

        var data = PageResult<ClassResponse>.Create(classes, count, pagenumber, pagesize);

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    [EndpointName("GetClassById")]
    [EndpointSummary("Get class by ID")]
    [EndpointDescription("Retrieve a specific class by its unique identifier")]
    [ProducesResponseType(typeof(ClassResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var classItem = await _classServices.GetClassByIdAsync(id);

        if (classItem is null)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"الصف برقم {id} غير موجود"
            );

        return Ok(classItem);
    }

    [HttpGet("by-number/{classNumber}")]
    [EndpointName("GetClassByNumber")]
    [EndpointSummary("Get class by number")]
    [EndpointDescription("Retrieve a specific class by its class number")]
    [ProducesResponseType(typeof(ClassResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByNumber(string classNumber)
    {
        var classItem = await _classServices.GetClassByNumberAsync(classNumber);

        if (classItem is null)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"الصف '{classNumber}' غير موجود"
            );

        return Ok(classItem);
    }

    [HttpGet("count")]
    [EndpointName("GetClassCount")]
    [EndpointSummary("Get total class count")]
    [EndpointDescription("Retrieve the total number of classes in the system")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount()
    {
        var count = await _classServices.ClassCountAsync();
        return Ok(new { count });
    }

    [HttpPost]
    [EndpointName("CreateClass")]
    [EndpointSummary("Create a new class")]
    [EndpointDescription("Create a new class with its number and section")]
    [ProducesResponseType(typeof(ClassResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateClassRequest request)
    {
        var classItem = await _classServices.CreateClassAsync(request);

        if (classItem is null)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                title: "فشل الإنشاء",
                statusCode: StatusCodes.Status400BadRequest,
                detail: "لم يتم إنشاء الصف، تحقق من البيانات"
            );

        return CreatedAtAction(nameof(GetById), new { id = classItem.Id }, classItem);
    }

    [HttpPut("{id:int}")]
    [EndpointName("UpdateClass")]
    [EndpointSummary("Update a class")]
    [EndpointDescription("Update an existing class information")]
    [ProducesResponseType(typeof(ClassResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassRequest request)
    {
        var classItem = await _classServices.UpdateClassAsync(id, request);

        if (classItem is null)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"الصف برقم {id} غير موجود"
            );

        return Ok(classItem);
    }

    [HttpDelete("{id:int}")]
    [EndpointName("DeleteClass")]
    [EndpointSummary("Delete a class")]
    [EndpointDescription("Permanently delete a class from the system")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _classServices.DeleteClassAsync(id);

        if (!result)
            return Problem(
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"الصف برقم {id} غير موجود"
            );

        return NoContent();
    }
}
