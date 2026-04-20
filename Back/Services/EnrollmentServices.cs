using Back.Data;
using Back.Entities;
using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Back.Services;

public class EnrollmentServices : IEnrollmentServices
{
    private readonly AppDbContext _context;
    private readonly ILogger<EnrollmentServices> _logger;
    private readonly IMemoryCache _cache;

    private const string AllEnrollmentsCacheKey = "all_enrollments";
    private static string EnrollmentByIdCacheKey(int id) => $"enrollment_{id}";

    public EnrollmentServices(AppDbContext context, ILogger<EnrollmentServices> logger, IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<EnrollmentResponse>> GetAllEnrollmentsAsync(int PageSize, int PageNumber)
    {
        var cacheKey = $"{AllEnrollmentsCacheKey}_p{PageNumber}_s{PageSize}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<EnrollmentResponse>? cached))
            return cached!;

        var enrollments = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Subject)
            .Include(e => e.Class)
            .OrderBy(e => e.Id)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var result = enrollments.Select(e => e.ToDto());
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }

    public async Task<EnrollmentResponse> GetEnrollmentByIdAsync(int EnrollmentId)
    {
        var cacheKey = EnrollmentByIdCacheKey(EnrollmentId);

        if (_cache.TryGetValue(cacheKey, out EnrollmentResponse? cached))
            return cached!;

        var enrollment = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Subject)
            .Include(e => e.Class)
            .FirstOrDefaultAsync(e => e.Id == EnrollmentId);

        if (enrollment == null)
        {
            _logger.LogWarning("Enrollment with ID {EnrollmentId} not found", EnrollmentId);
            return null!;
        }

        var result = enrollment.ToDto();
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }

    public async Task<IEnumerable<EnrollmentResponse>> GetEnrollmentsByStudentAsync(int StudentId)
    {
        var enrollments = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Subject)
            .Include(e => e.Class)
            .Where(e => e.StudentId == StudentId)
            .ToListAsync();

        return enrollments.Select(e => e.ToDto());
    }

    public async Task<IEnumerable<EnrollmentResponse>> GetEnrollmentsByClassAsync(int ClassId)
    {
        var enrollments = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Subject)
            .Include(e => e.Class)
            .Where(e => e.ClassId == ClassId)
            .ToListAsync();

        return enrollments.Select(e => e.ToDto());
    }

    public async Task<IEnumerable<EnrollmentResponse>> GetEnrollmentsBySubjectAsync(int SubjectId)
    {
        var enrollments = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Subject)
            .Include(e => e.Class)
            .Where(e => e.SubjectId == SubjectId)
            .ToListAsync();

        return enrollments.Select(e => e.ToDto());
    }

    public async Task<EnrollmentResponse> CreateEnrollmentAsync(CreateEnrollmentRequest request)
    {
        try
        {
            var exists = await _context.Enrollments.AnyAsync(e =>
                e.StudentId == request.StudentId && e.SubjectId == request.SubjectId);

            if (exists)
                throw new InvalidOperationException("Student already enrolled in this subject");

            var enrollment = new Enrollment
            {
                StudentId = request.StudentId,
                SubjectId = request.SubjectId,
                ClassId = request.ClassId,
                EnrollmentDate = DateTime.UtcNow
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            await _context.Entry(enrollment).Reference(e => e.Student).LoadAsync();
            await _context.Entry(enrollment).Reference(e => e.Subject).LoadAsync();
            await _context.Entry(enrollment).Reference(e => e.Class).LoadAsync();

            _logger.LogInformation("Student {StudentId} enrolled in Subject {SubjectId}", request.StudentId, request.SubjectId);

            InvalidateCache();

            return enrollment.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating enrollment: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteEnrollmentAsync(int EnrollmentId)
    {
        try
        {
            var enrollment = await _context.Enrollments.FindAsync(EnrollmentId);

            if (enrollment == null)
            {
                _logger.LogWarning("Enrollment with ID {EnrollmentId} not found", EnrollmentId);
                return false;
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Enrollment {EnrollmentId} deleted successfully", EnrollmentId);

            InvalidateCache(EnrollmentId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error deleting enrollment: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<int> EnrollmentCountAsync()
    {
        return await _context.Enrollments.CountAsync();
    }

    private void InvalidateCache(int? id = null)
    {
        if (id.HasValue)
            _cache.Remove(EnrollmentByIdCacheKey(id.Value));

        for (int page = 1; page <= 100; page++)
            for (int size = 1; size <= 100; size++)
                _cache.Remove($"{AllEnrollmentsCacheKey}_p{page}_s{size}");
    }
}
