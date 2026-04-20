using Back.Data;
using Back.Entities;
using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Back.Services;

public class AttendanceServices : IAttendanceServices
{
    private readonly AppDbContext _context;
    private readonly ILogger<AttendanceServices> _logger;
    private readonly IMemoryCache _cache;

    private const string AllAttendancesCacheKey = "all_attendances";
    private static string AttendanceByIdCacheKey(int id) => $"attendance_{id}";

    public AttendanceServices(AppDbContext context, ILogger<AttendanceServices> logger, IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<AttendanceResponse>> GetAllAttendancesAsync(int PageSize, int PageNumber)
    {
        var cacheKey = $"{AllAttendancesCacheKey}_p{PageNumber}_s{PageSize}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<AttendanceResponse>? cached))
            return cached!;

        var attendances = await _context.Attendances
            .Include(a => a.student)
            .Include(a => a.teacher)
            .OrderBy(a => a.AttendanceId)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var result = attendances.Select(a => a.ToDto());
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<AttendanceResponse> GetAttendanceByIdAsync(int AttendanceId)
    {
        var cacheKey = AttendanceByIdCacheKey(AttendanceId);

        if (_cache.TryGetValue(cacheKey, out AttendanceResponse? cached))
            return cached!;

        var attendance = await _context.Attendances
            .Include(a => a.student)
            .Include(a => a.teacher)
            .FirstOrDefaultAsync(a => a.AttendanceId == AttendanceId);

        if (attendance == null)
        {
            _logger.LogWarning("Attendance with ID {AttendanceId} not found", AttendanceId);
            return null!;
        }

        var result = attendance.ToDto();
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<IEnumerable<AttendanceResponse>> GetAttendanceByStudentAsync(int StudentId)
    {
        var attendances = await _context.Attendances
            .Include(a => a.student)
            .Include(a => a.teacher)
            .Where(a => a.StudentId == StudentId)
            .ToListAsync();

        return attendances.Select(a => a.ToDto());
    }

    public async Task<IEnumerable<AttendanceResponse>> GetAttendanceByDateAsync(DateOnly Date)
    {
        var attendances = await _context.Attendances
            .Include(a => a.student)
            .Include(a => a.teacher)
            .Where(a => a.Date == Date)
            .ToListAsync();

        return attendances.Select(a => a.ToDto());
    }

    public async Task<AttendanceResponse> CreateAttendanceAsync(CreateAttendanceRequest request)
    {
        try
        {
            var attendance = new Attendance
            {
                Date = request.Date,
                IsPresent = request.IsPresent,
                Notes = request.Notes,
                StudentId = request.StudentId,
                TeacherId = request.TeacherId
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            await _context.Entry(attendance).Reference(a => a.student).LoadAsync();
            await _context.Entry(attendance).Reference(a => a.teacher).LoadAsync();

            _logger.LogInformation("Attendance created for Student {StudentId} on {Date}", request.StudentId, request.Date);

            InvalidateCache();

            return attendance.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating attendance: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<AttendanceResponse> UpdateAttendanceAsync(int AttendanceId, UpdateAttendanceRequest request)
    {
        try
        {
            var attendance = await _context.Attendances
                .Include(a => a.student)
                .Include(a => a.teacher)
                .FirstOrDefaultAsync(a => a.AttendanceId == AttendanceId);

            if (attendance == null)
            {
                _logger.LogWarning("Attendance with ID {AttendanceId} not found", AttendanceId);
                return null!;
            }

            if (request.IsPresent.HasValue) attendance.IsPresent = request.IsPresent.Value;
            if (request.Notes != null) attendance.Notes = request.Notes;

            _context.Attendances.Update(attendance);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Attendance {AttendanceId} updated successfully", AttendanceId);

            InvalidateCache(AttendanceId);

            return attendance.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error updating attendance: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteAttendanceAsync(int AttendanceId)
    {
        try
        {
            var attendance = await _context.Attendances.FindAsync(AttendanceId);

            if (attendance == null)
            {
                _logger.LogWarning("Attendance with ID {AttendanceId} not found", AttendanceId);
                return false;
            }

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Attendance {AttendanceId} deleted successfully", AttendanceId);

            InvalidateCache(AttendanceId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error deleting attendance: {Message}", ex.Message);
            throw;
        }
    }

    private void InvalidateCache(int? id = null)
    {
        if (id.HasValue)
            _cache.Remove(AttendanceByIdCacheKey(id.Value));

        for (int page = 1; page <= 100; page++)
            for (int size = 1; size <= 100; size++)
                _cache.Remove($"{AllAttendancesCacheKey}_p{page}_s{size}");
    }
}
