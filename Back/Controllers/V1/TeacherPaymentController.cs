using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/teacher-payments")]
[Authorize(Roles = "Admin")]
[Tags("Teacher Payments")]
public class TeacherPaymentController : ControllerBase
{
    private readonly ITeacherPaymentServices _paymentServices;

    public TeacherPaymentController(ITeacherPaymentServices paymentServices)
    {
        _paymentServices = paymentServices;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TeacherPaymentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1)
    {
        var pagenumber = Math.Max(pageNumber, 1);
        var pagesize = Math.Clamp(pageSize, 1, 100);
        var payments = await _paymentServices.GetAllTeacherPaymentsAsync(pagesize, pagenumber);

        return Ok(payments);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TeacherPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var payment = await _paymentServices.GetTeacherPaymentByIdAsync(id);

        if (payment is null)
            return Problem(
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"السجل برقم {id} غير موجود"
            );

        return Ok(payment);
    }

    [HttpGet("by-teacher/{teacherId:int}")]
    [ProducesResponseType(typeof(IEnumerable<TeacherPaymentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTeacher(int teacherId)
    {
        var payments = await _paymentServices.GetPaymentsByTeacherIdAsync(teacherId);
        return Ok(payments);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TeacherPaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTeacherPaymentRequest request)
    {
        try
        {
            var payment = await _paymentServices.CreateTeacherPaymentAsync(request);

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
    [ProducesResponseType(typeof(TeacherPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTeacherPaymentRequest request)
    {
        try
        {
            var payment = await _paymentServices.UpdateTeacherPaymentAsync(id, request);

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
        var result = await _paymentServices.DeleteTeacherPaymentAsync(id);

        if (!result)
            return Problem(
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"السجل برقم {id} غير موجود"
            );

        return NoContent();
    }
}
