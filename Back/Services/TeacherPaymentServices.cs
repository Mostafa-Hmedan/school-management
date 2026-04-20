using Back.Data;
using Back.Entities;
using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Back.Services;

public class TeacherPaymentServices : ITeacherPaymentServices
{
    private readonly AppDbContext _context;
    private readonly ILogger<TeacherPaymentServices> _logger;
    private readonly IMemoryCache _cache;

    private const string AllTeacherPaymentsCacheKey = "all_teacher_payments";
    private static string TeacherPaymentByIdCacheKey(int id) => $"teacher_payment_{id}";

    public TeacherPaymentServices(AppDbContext context, ILogger<TeacherPaymentServices> logger, IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<TeacherPaymentResponse>> GetAllTeacherPaymentsAsync(int PageSize, int PageNumber)
    {
        var cacheKey = $"{AllTeacherPaymentsCacheKey}_p{PageNumber}_s{PageSize}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<TeacherPaymentResponse>? cached))
            return cached!;

        var payments = await _context.TeacherPayments
            .Include(p => p.Teacher)
            .OrderByDescending(p => p.PaymentDate) // الأحدث أولاً
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var result = payments.Select(p => p.ToDto());
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<TeacherPaymentResponse> GetTeacherPaymentByIdAsync(int PaymentId)
    {
        var cacheKey = TeacherPaymentByIdCacheKey(PaymentId);

        if (_cache.TryGetValue(cacheKey, out TeacherPaymentResponse? cached))
            return cached!;

        var payment = await _context.TeacherPayments
            .Include(p => p.Teacher)
            .FirstOrDefaultAsync(p => p.Id == PaymentId);

        if (payment == null)
            return null!;

        var result = payment.ToDto();
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<IEnumerable<TeacherPaymentResponse>> GetPaymentsByTeacherIdAsync(int TeacherId)
    {
        var payments = await _context.TeacherPayments
            .Include(p => p.Teacher)
            .Where(p => p.TeacherId == TeacherId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return payments.Select(p => p.ToDto());
    }

    public async Task<TeacherPaymentResponse> CreateTeacherPaymentAsync(CreateTeacherPaymentRequest request)
    {
        var payment = new TeacherPayment
        {
            TeacherId = request.TeacherId,
            TotalAmount = request.TotalAmount,
            PaidAmount = request.PaidAmount,
            RemainingAmount = request.TotalAmount - request.PaidAmount,
            PaymentDate = request.PaymentDate
        };

        _context.TeacherPayments.Add(payment);
        await _context.SaveChangesAsync();

        await _context.Entry(payment).Reference(p => p.Teacher).LoadAsync();

        InvalidateCache();

        return payment.ToDto();
    }

    public async Task<TeacherPaymentResponse> UpdateTeacherPaymentAsync(int PaymentId, UpdateTeacherPaymentRequest request)
    {
        var payment = await _context.TeacherPayments
            .Include(p => p.Teacher)
            .FirstOrDefaultAsync(p => p.Id == PaymentId);

        if (payment == null)
            return null!;

        if (request.TotalAmount.HasValue) payment.TotalAmount = request.TotalAmount.Value;
        if (request.PaidAmount.HasValue) payment.PaidAmount = request.PaidAmount.Value;
        if (request.PaymentDate.HasValue) payment.PaymentDate = request.PaymentDate.Value;

        // Recalculate remaining
        payment.RemainingAmount = payment.TotalAmount - payment.PaidAmount;

        _context.TeacherPayments.Update(payment);
        await _context.SaveChangesAsync();

        InvalidateCache(PaymentId);

        return payment.ToDto();
    }

    public async Task<bool> DeleteTeacherPaymentAsync(int PaymentId)
    {
        var payment = await _context.TeacherPayments.FindAsync(PaymentId);

        if (payment == null)
            return false;

        _context.TeacherPayments.Remove(payment);
        await _context.SaveChangesAsync();

        InvalidateCache(PaymentId);

        return true;
    }

    public async Task<int> TeacherPaymentCountAsync()
    {
        return await _context.TeacherPayments.CountAsync();
    }

    private void InvalidateCache(int? id = null)
    {
        if (id.HasValue)
            _cache.Remove(TeacherPaymentByIdCacheKey(id.Value));

        for (int page = 1; page <= 50; page++)
            for (int size = 1; size <= 50; size++)
                _cache.Remove($"{AllTeacherPaymentsCacheKey}_p{page}_s{size}");
    }
}
