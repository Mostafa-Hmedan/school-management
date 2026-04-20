using Back.Data;
using Back.Entities;
using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Back.Services;

public class EmployeePaymentServices : IEmployeePaymentServices
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmployeePaymentServices> _logger;
    private readonly IMemoryCache _cache;

    private const string AllEmployeePaymentsCacheKey = "all_employee_payments";
    private static string EmployeePaymentByIdCacheKey(int id) => $"employee_payment_{id}";

    public EmployeePaymentServices(AppDbContext context, ILogger<EmployeePaymentServices> logger, IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<EmployeePaymentResponse>> GetAllEmployeePaymentsAsync(int PageSize, int PageNumber)
    {
        var cacheKey = $"{AllEmployeePaymentsCacheKey}_p{PageNumber}_s{PageSize}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<EmployeePaymentResponse>? cached))
            return cached!;

        var payments = await _context.EmployeePayments
            .Include(p => p.Employee)
            .OrderByDescending(p => p.PaymentDate)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var result = payments.Select(p => p.ToDto());
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<EmployeePaymentResponse?> GetEmployeePaymentByIdAsync(int PaymentId)
    {
        var cacheKey = EmployeePaymentByIdCacheKey(PaymentId);

        if (_cache.TryGetValue(cacheKey, out EmployeePaymentResponse? cached))
            return cached!;

        var payment = await _context.EmployeePayments
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.Id == PaymentId);

        if (payment == null)
            return null;

        var result = payment.ToDto();
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<IEnumerable<EmployeePaymentResponse>> GetPaymentsByEmployeeIdAsync(int EmployeeId)
    {
        var payments = await _context.EmployeePayments
            .Include(p => p.Employee)
            .Where(p => p.EmployeeId == EmployeeId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return payments.Select(p => p.ToDto());
    }

    public async Task<EmployeePaymentResponse?> CreateEmployeePaymentAsync(CreateEmployeePaymentRequest request)
    {
        var payment = new EmployeePayment
        {
            EmployeeId = request.EmployeeId,
            TotalAmount = request.TotalAmount,
            PaidAmount = request.PaidAmount,
            RemainingAmount = request.TotalAmount - request.PaidAmount,
            PaymentDate = request.PaymentDate
        };

        _context.EmployeePayments.Add(payment);
        await _context.SaveChangesAsync();

        await _context.Entry(payment).Reference(p => p.Employee).LoadAsync();

        InvalidateCache();

        return payment.ToDto();
    }

    public async Task<EmployeePaymentResponse?> UpdateEmployeePaymentAsync(int PaymentId, UpdateEmployeePaymentRequest request)
    {
        var payment = await _context.EmployeePayments
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.Id == PaymentId);

        if (payment == null)
            return null;

        if (request.TotalAmount.HasValue) payment.TotalAmount = request.TotalAmount.Value;
        if (request.PaidAmount.HasValue) payment.PaidAmount = request.PaidAmount.Value;
        if (request.PaymentDate.HasValue) payment.PaymentDate = request.PaymentDate.Value;

        payment.RemainingAmount = payment.TotalAmount - payment.PaidAmount;

        _context.EmployeePayments.Update(payment);
        await _context.SaveChangesAsync();

        InvalidateCache(PaymentId);

        return payment.ToDto();
    }

    public async Task<bool> DeleteEmployeePaymentAsync(int PaymentId)
    {
        var payment = await _context.EmployeePayments.FindAsync(PaymentId);

        if (payment == null)
            return false;

        _context.EmployeePayments.Remove(payment);
        await _context.SaveChangesAsync();

        InvalidateCache(PaymentId);

        return true;
    }

    public async Task<int> EmployeePaymentCountAsync()
    {
        return await _context.EmployeePayments.CountAsync();
    }

    private void InvalidateCache(int? id = null)
    {
        if (id.HasValue)
            _cache.Remove(EmployeePaymentByIdCacheKey(id.Value));

        for (int page = 1; page <= 50; page++)
            for (int size = 1; size <= 50; size++)
                _cache.Remove($"{AllEmployeePaymentsCacheKey}_p{page}_s{size}");
    }
}
