using Back.Data;
using Back.Entities;
using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Back.Services;

public class GradeServices : IGradeServices
{
    private readonly AppDbContext _context;
    private readonly ILogger<GradeServices> _logger;
    private readonly IMemoryCache _cache;

    private const string AllGradesCacheKey = "all_grades";
    private static string GradeByIdCacheKey(int id) => $"grade_{id}";

    public GradeServices(AppDbContext context, ILogger<GradeServices> logger, IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<GradeResponse>> GetAllGradesAsync(int PageSize, int PageNumber)
    {
        var cacheKey = $"{AllGradesCacheKey}_p{PageNumber}_s{PageSize}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<GradeResponse>? cached))
            return cached!;

        var grades = await _context.StudentGrades
            .Include(g => g.Student)
            .Include(g => g.Teacher)
            .Include(g => g.Subject)
            .OrderBy(g => g.Id)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var result = grades.Select(g => g.ToDto());
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<GradeResponse> GetGradeByIdAsync(int GradeId)
    {
        var cacheKey = GradeByIdCacheKey(GradeId);

        if (_cache.TryGetValue(cacheKey, out GradeResponse? cached))
            return cached!;

        var grade = await _context.StudentGrades
            .Include(g => g.Student)
            .Include(g => g.Teacher)
            .Include(g => g.Subject)
            .FirstOrDefaultAsync(g => g.Id == GradeId);

        if (grade == null)
        {
            _logger.LogWarning("Grade with ID {GradeId} not found", GradeId);
            return null!;
        }

        var result = grade.ToDto();
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<IEnumerable<GradeResponse>> GetGradesByStudentAsync(int StudentId)
    {
        var grades = await _context.StudentGrades
            .Include(g => g.Student)
            .Include(g => g.Teacher)
            .Include(g => g.Subject)
            .Where(g => g.StudentId == StudentId)
            .ToListAsync();

        return grades.Select(g => g.ToDto());
    }

    public async Task<IEnumerable<GradeResponse>> GetGradesBySubjectAsync(int SubjectId)
    {
        var grades = await _context.StudentGrades
            .Include(g => g.Student)
            .Include(g => g.Teacher)
            .Include(g => g.Subject)
            .Where(g => g.SubjectId == SubjectId)
            .ToListAsync();

        return grades.Select(g => g.ToDto());
    }

    public async Task<IEnumerable<GradeResponse>> GetGradesByTeacherAsync(int TeacherId)
    {
        var grades = await _context.StudentGrades
            .Include(g => g.Student)
            .Include(g => g.Teacher)
            .Include(g => g.Subject)
            .Where(g => g.TeacherId == TeacherId)
            .OrderByDescending(g => g.DateGiven)
            .ToListAsync();

        return grades.Select(g => g.ToDto());
    }

    public async Task<GradeResponse> CreateGradeAsync(CreateGradeRequest request)
    {
        try
        {
            var grade = new StudentGrade
            {
                Score = request.Score,
                GradeType = request.GradeType,
                DateGiven = DateTime.UtcNow,
                StudentId = request.StudentId,
                TeacherId = request.TeacherId,
                SubjectId = request.SubjectId
            };

            _context.StudentGrades.Add(grade);
            await _context.SaveChangesAsync();

            await _context.Entry(grade).Reference(g => g.Student).LoadAsync();
            await _context.Entry(grade).Reference(g => g.Teacher).LoadAsync();
            await _context.Entry(grade).Reference(g => g.Subject).LoadAsync();

            _logger.LogInformation("Grade created for Student {StudentId}", request.StudentId);

            InvalidateCache();

            return grade.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating grade: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<GradeResponse> UpdateGradeAsync(int GradeId, UpdateGradeRequest request)
    {
        try
        {
            var grade = await _context.StudentGrades
                .Include(g => g.Student)
                .Include(g => g.Teacher)
                .Include(g => g.Subject)
                .FirstOrDefaultAsync(g => g.Id == GradeId);

            if (grade == null)
            {
                _logger.LogWarning("Grade with ID {GradeId} not found", GradeId);
                return null!;
            }

            if (request.Score.HasValue) grade.Score = request.Score.Value;
            if (request.GradeType != null) grade.GradeType = request.GradeType;

            _context.StudentGrades.Update(grade);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Grade {GradeId} updated successfully", GradeId);

            InvalidateCache(GradeId);

            return grade.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error updating grade: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteGradeAsync(int GradeId)
    {
        try
        {
            var grade = await _context.StudentGrades.FindAsync(GradeId);

            if (grade == null)
            {
                _logger.LogWarning("Grade with ID {GradeId} not found", GradeId);
                return false;
            }

            _context.StudentGrades.Remove(grade);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Grade {GradeId} deleted successfully", GradeId);

            InvalidateCache(GradeId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error deleting grade: {Message}", ex.Message);
            throw;
        }
    }

    private void InvalidateCache(int? id = null)
    {
        if (id.HasValue)
            _cache.Remove(GradeByIdCacheKey(id.Value));

        for (int page = 1; page <= 100; page++)
            for (int size = 1; size <= 100; size++)
                _cache.Remove($"{AllGradesCacheKey}_p{page}_s{size}");
    }
}
