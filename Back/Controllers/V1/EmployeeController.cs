using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/employees")]
[Authorize(Roles = "Admin")]
[Tags("Employees")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeServices _employeeServices;

    public EmployeeController(IEmployeeServices employeeServices)
    {
        _employeeServices = employeeServices;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PageResult<EmployeeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1)
    {
        var pagenumber = Math.Max(pageNumber, 1);
        var pagesize = Math.Clamp(pageSize, 1, 100);
        var employees = await _employeeServices.GetAllEmployeesAsync(pagesize, pagenumber);
        var count = await _employeeServices.EmployeeCountAsync();

        var data = PageResult<EmployeeResponse>.Create(employees, count, pagenumber, pagesize);

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _employeeServices.GetEmployeeByIdAsync(id);

        if (employee is null)
            return Problem(
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"الموظف برقم {id} غير موجود"
            );

        return Ok(employee);
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromForm] CreateEmployeeRequest request, IFormFile? image)
    {
        try
        {
            var employee = await _employeeServices.CreateEmployeeAsync(request, image);

            if (employee is null)
                return Problem(
                    title: "فشل الإنشاء",
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: "لم يتم إنشاء الموظف، تحقق من البيانات"
                );

            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
        }
        catch (Exception ex)
        {
            return Problem(
                title: "بيانات غير صالحة",
                statusCode: StatusCodes.Status400BadRequest,
                detail: ex.Message
            );
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateEmployeeRequest request, IFormFile? image)
    {
        try
        {
            var employee = await _employeeServices.UpdateEmployeeAsync(id, request, image);

            if (employee is null)
                return Problem(
                    title: "غير موجود",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"الموظف برقم {id} غير موجود"
                );

            return Ok(employee);
        }
        catch (Exception ex)
        {
            return Problem(
                title: "خطأ داخلي",
                statusCode: StatusCodes.Status500InternalServerError,
                detail: ex.Message
            );
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _employeeServices.DeleteEmployeeAsync(id);

        if (!result)
            return Problem(
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"الموظف برقم {id} غير موجود"
            );

        return NoContent();
    }
}
