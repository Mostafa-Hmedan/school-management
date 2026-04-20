using Back.Data;
using Back.Entities;
using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Back.Services;

public class StudentPaymentServices : IStudentPaymentServices
{
    private readonly AppDbContext _context;
    private readonly ILogger<StudentPaymentServices> _logger;
    private readonly IMemoryCache _cache;

    private const string AllStudentPaymentsCacheKey = "all_student_payments";
    private static string StudentPaymentByIdCacheKey(int id) => $"student_payment_{id}";

    public StudentPaymentServices(AppDbContext context, ILogger<StudentPaymentServices> logger, IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<StudentPaymentResponse>> GetAllStudentPaymentsAsync(int PageSize, int PageNumber)
    {
        var cacheKey = $"{AllStudentPaymentsCacheKey}_p{PageNumber}_s{PageSize}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<StudentPaymentResponse>? cached))
            return cached!;

        var payments = await _context.StudentPayments
            .Include(p => p.Student)
            .OrderByDescending(p => p.PaymentDate) // الأحدث أولاً
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var result = payments.Select(p => p.ToDto());
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<StudentPaymentResponse> GetStudentPaymentByIdAsync(int PaymentId)
    {
        var cacheKey = StudentPaymentByIdCacheKey(PaymentId);

        if (_cache.TryGetValue(cacheKey, out StudentPaymentResponse? cached))
            return cached!;

        var payment = await _context.StudentPayments
            .Include(p => p.Student)
            .FirstOrDefaultAsync(p => p.Id == PaymentId);

        if (payment == null)
            return null!;

        var result = payment.ToDto();
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<IEnumerable<StudentPaymentResponse>> GetPaymentsByStudentIdAsync(int StudentId)
    {
        var payments = await _context.StudentPayments
            .Include(p => p.Student)
            .Where(p => p.StudentId == StudentId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return payments.Select(p => p.ToDto());
    }

    public async Task<StudentPaymentResponse> CreateStudentPaymentAsync(CreateStudentPaymentRequest request)
    {
        var payment = new StudentPayment
        {
            StudentId = request.StudentId,
            TotalAmount = request.TotalAmount,
            PaidAmount = request.PaidAmount,
            RemainingAmount = request.TotalAmount - request.PaidAmount,
            PaymentDate = request.PaymentDate
        };

        _context.StudentPayments.Add(payment);
        await _context.SaveChangesAsync();

        await _context.Entry(payment).Reference(p => p.Student).LoadAsync();

        InvalidateCache();

        return payment.ToDto();
    }

    public async Task<StudentPaymentResponse> UpdateStudentPaymentAsync(int PaymentId, UpdateStudentPaymentRequest request)
    {
        var payment = await _context.StudentPayments
            .Include(p => p.Student)
            .FirstOrDefaultAsync(p => p.Id == PaymentId);

        if (payment == null)
            return null!;

        if (request.TotalAmount.HasValue) payment.TotalAmount = request.TotalAmount.Value;
        if (request.PaidAmount.HasValue) payment.PaidAmount = request.PaidAmount.Value;
        if (request.PaymentDate.HasValue) payment.PaymentDate = request.PaymentDate.Value;

        // Recalculate remaining
        payment.RemainingAmount = payment.TotalAmount - payment.PaidAmount;

        _context.StudentPayments.Update(payment);
        await _context.SaveChangesAsync();

        InvalidateCache(PaymentId);

        return payment.ToDto();
    }

    public async Task<bool> DeleteStudentPaymentAsync(int PaymentId)
    {
        var payment = await _context.StudentPayments.FindAsync(PaymentId);

        if (payment == null)
            return false;

        _context.StudentPayments.Remove(payment);
        await _context.SaveChangesAsync();

        InvalidateCache(PaymentId);

        return true;
    }

    public async Task<int> StudentPaymentCountAsync()
    {
        return await _context.StudentPayments.CountAsync();
    }

    private void InvalidateCache(int? id = null)
    {
        if (id.HasValue)
            _cache.Remove(StudentPaymentByIdCacheKey(id.Value));

        for (int page = 1; page <= 50; page++)
            for (int size = 1; size <= 50; size++)
                _cache.Remove($"{AllStudentPaymentsCacheKey}_p{page}_s{size}");
    }
}
