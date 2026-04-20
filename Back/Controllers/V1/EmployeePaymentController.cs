using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/employee-payments")]
[Authorize(Roles = "Admin")]
[Tags("Employee Payments")]
public class EmployeePaymentController : ControllerBase
{
    private readonly IEmployeePaymentServices _paymentServices;

    public EmployeePaymentController(IEmployeePaymentServices paymentServices)
    {
        _paymentServices = paymentServices;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmployeePaymentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1)
    {
        var pagenumber = Math.Max(pageNumber, 1);
        var pagesize = Math.Clamp(pageSize, 1, 100);
        var payments = await _paymentServices.GetAllEmployeePaymentsAsync(pagesize, pagenumber);

        return Ok(payments);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EmployeePaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var payment = await _paymentServices.GetEmployeePaymentByIdAsync(id);

        if (payment is null)
            return Problem(
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"السجل برقم {id} غير موجود"
            );

        return Ok(payment);
    }

    [HttpGet("by-employee/{employeeId:int}")]
    [ProducesResponseType(typeof(IEnumerable<EmployeePaymentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEmployee(int employeeId)
    {
        var payments = await _paymentServices.GetPaymentsByEmployeeIdAsync(employeeId);
        return Ok(payments);
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmployeePaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeePaymentRequest request)
    {
        try
        {
            var payment = await _paymentServices.CreateEmployeePaymentAsync(request);

            if (payment is null)
                return Problem(
                    title: "فشل الإنشاء",
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: "لم يتم إنشاء السجل، تحقق من البيانات"
                );

            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
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
    [ProducesResponseType(typeof(EmployeePaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeePaymentRequest request)
    {
        try
        {
            var payment = await _paymentServices.UpdateEmployeePaymentAsync(id, request);

            if (payment is null)
                return Problem(
                    title: "غير موجود",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"السجل برقم {id} غير موجود"
                );

            return Ok(payment);
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
        var result = await _paymentServices.DeleteEmployeePaymentAsync(id);

        if (!result)
            return Problem(
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"السجل برقم {id} غير موجود"
            );

        return NoContent();
    }
}
