using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Back.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/student-payments")]
[Authorize]
[Tags("Student Payments")]
public class StudentPaymentController : ControllerBase
{
    private readonly IStudentPaymentServices _paymentServices;
    private readonly IStudentServices _studentServices;

    public StudentPaymentController(IStudentPaymentServices paymentServices, IStudentServices studentServices)
    {
        _paymentServices = paymentServices;
        _studentServices = studentServices;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<StudentPaymentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1)
    {
        var pagenumber = Math.Max(pageNumber, 1);
        var pagesize = Math.Clamp(pageSize, 1, 100);
        var payments = await _paymentServices.GetAllStudentPaymentsAsync(pagesize, pagenumber);

        return Ok(payments);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(StudentPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var payment = await _paymentServices.GetStudentPaymentByIdAsync(id);

        if (payment is null)
            return Problem(
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"السجل برقم {id} غير موجود"
            );

        return Ok(payment);
    }

    [HttpGet("by-student/{studentId:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<StudentPaymentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStudent(int studentId)
    {
        var payments = await _paymentServices.GetPaymentsByStudentIdAsync(studentId);
        return Ok(payments);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Student")]
    [EndpointName("GetMyStudentPayments")]
    [EndpointSummary("Get my payment records")]
    [EndpointDescription("Returns all payment records of the currently authenticated student")]
    [ProducesResponseType(typeof(IEnumerable<StudentPaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyPayments()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var student = await _studentServices.GetStudentByUserIdAsync(userId!);

        if (student is null)
            return Problem(title: "غير موجود", statusCode: StatusCodes.Status404NotFound,
                detail: "لم يتم العثور على بيانات الطالب");

        var payments = await _paymentServices.GetPaymentsByStudentIdAsync(student.Id);
        return Ok(payments);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(StudentPaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateStudentPaymentRequest request)
    {
        try
        {
            var payment = await _paymentServices.CreateStudentPaymentAsync(request);

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
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(StudentPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentPaymentRequest request)
    {
        try
        {
            var payment = await _paymentServices.UpdateStudentPaymentAsync(id, request);

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
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _paymentServices.DeleteStudentPaymentAsync(id);

        if (!result)
            return Problem(
                title: "غير موجود",
                statusCode: StatusCodes.Status404NotFound,
                detail: $"السجل برقم {id} غير موجود"
            );

        return NoContent();
    }
}
