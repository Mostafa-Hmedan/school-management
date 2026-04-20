using Back.Data;
using Back.Entities;
using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Back.Services;

public class SubjectServices : ISubjectServices
{
    private readonly AppDbContext _context;
    private readonly ILogger<SubjectServices> _logger;
    private readonly IMemoryCache _cache;

    private const string AllSubjectsCacheKey = "all_subjects";
    private static string SubjectByIdCacheKey(int id) => $"subject_{id}";

    public SubjectServices(AppDbContext context, ILogger<SubjectServices> logger, IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<SubjectResponse>> GetAllSubjectsAsync(int PageSize, int PageNumber)
    {
        var cacheKey = $"{AllSubjectsCacheKey}_p{PageNumber}_s{PageSize}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<SubjectResponse>? cached))
            return cached!;

        var subjects = await _context.Subjects
            .Include(s => s.TeacherName)
            .OrderBy(s => s.Id)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var result = subjects.Select(s => s.ToDto());

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }

    public async Task<SubjectResponse> GetSubjectByIdAsync(int SubjectId)
    {
        var cacheKey = SubjectByIdCacheKey(SubjectId);

        if (_cache.TryGetValue(cacheKey, out SubjectResponse? cached))
            return cached!;

        var subject = await _context.Subjects
            .Include(s => s.TeacherName)
            .FirstOrDefaultAsync(s => s.Id == SubjectId);

        if (subject == null)
        {
            _logger.LogWarning("Subject with ID {SubjectId} not found", SubjectId);
            return null!;
        }

        var result = subject.ToDto();
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }

    public async Task<SubjectResponse> GetSubjectByNameAsync(string SubjectName)
    {
        var subject = await _context.Subjects
            .Include(s => s.TeacherName)
            .FirstOrDefaultAsync(s => s.SubjectName == SubjectName);

        if (subject == null)
        {
            _logger.LogWarning("Subject '{SubjectName}' not found", SubjectName);
            return null!;
        }

        return subject.ToDto();
    }

    public async Task<SubjectResponse> CreateSubjectAsync(CreateSubjectRequest request)
    {
        try
        {
            var exists = await _context.Subjects.AnyAsync(s => s.SubjectName == request.SubjectName);
            if (exists)
                throw new InvalidOperationException("Subject already exists");

            var subject = new Subject { SubjectName = request.SubjectName };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Subject '{SubjectName}' created successfully", subject.SubjectName);

            // مسح كل الـ cache عند الإضافة
            InvalidateCache();

            return subject.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating subject: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<SubjectResponse> UpdateSubjectAsync(int SubjectId, UpdateSubjectRequest request)
    {
        try
        {
            var subject = await _context.Subjects
                .Include(s => s.TeacherName)
                .FirstOrDefaultAsync(s => s.Id == SubjectId);

            if (subject == null)
            {
                _logger.LogWarning("Subject with ID {SubjectId} not found", SubjectId);
                return null!;
            }

            if (request.SubjectName != null) subject.SubjectName = request.SubjectName;

            _context.Subjects.Update(subject);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Subject {SubjectId} updated successfully", SubjectId);

            // مسح الـ cache الخاص بهذا العنصر
            InvalidateCache(SubjectId);

            return subject.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error updating subject: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteSubjectAsync(int SubjectId)
    {
        try
        {
            var subject = await _context.Subjects.FindAsync(SubjectId);

            if (subject == null)
            {
                _logger.LogWarning("Subject with ID {SubjectId} not found", SubjectId);
                return false;
            }

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Subject {SubjectId} deleted successfully", SubjectId);

            // مسح الـ cache
            InvalidateCache(SubjectId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error deleting subject: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<int> SubjectCountAsync()
    {
        return await _context.Subjects.CountAsync();
    }

    private void InvalidateCache(int? id = null)
    {
        if (id.HasValue)
            _cache.Remove(SubjectByIdCacheKey(id.Value));

        // مسح كل صفحات القائمة
        for (int page = 1; page <= 100; page++)
            for (int size = 1; size <= 100; size++)
                _cache.Remove($"{AllSubjectsCacheKey}_p{page}_s{size}");
    }
}
