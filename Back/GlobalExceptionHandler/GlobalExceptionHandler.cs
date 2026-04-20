// هاد الكلاس يلتقط كل Exception تحصل في أي مكان في التطبيق
// IProblemDetailsService = خدمة جاهزة تحول الخطأ لـ RFC 9457 format
// IExceptionHandler = الإنترفيس اللي يخليه يشتغل كـ Global Handler
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    // هاد الميثود يُستدعى تلقائياً كلما صار throw لأي Exception
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,       // الريكوست الحالي
        Exception exception,          // الخطأ اللي صار
        CancellationToken cancellationToken)
    {
        // نحدد الـ StatusCode بناءً على نوع الـ Exception
        // ValidationException  → 400 Bad Request  (خطأ من المستخدم)
        // أي Exception ثانية  → 500 Internal Server Error (خطأ من السيرفر)
        httpContext.Response.StatusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        // نرجع الـ Response بـ ProblemDetails format وفق RFC 9457
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,   // نعطيه الـ context الحالي
            Exception = exception,      // نعطيه الخطأ عشان يقدر يقرأ منه

            ProblemDetails = new ProblemDetails
            {
                Type = exception.GetType().Name, // اسم نوع الـ Exception مثلاً "ValidationException"
                Title = "Error has occured",      // رسالة عامة للـ Client
                Detail = exception.Message        // تفاصيل الخطأ من الـ Exception نفسها
            }
        });
    }
}
 