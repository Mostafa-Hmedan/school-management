using Back.Data;
using Back.Entities;
using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Back.Services;

public class ClassServices : IClassServices
{
    private readonly AppDbContext _context;
    private readonly ILogger<ClassServices> _logger;
    private readonly IMemoryCache _cache;

    private const string AllClassesCacheKey = "all_classes";
    private static string ClassByIdCacheKey(int id) => $"class_{id}";

    public ClassServices(AppDbContext context, ILogger<ClassServices> logger, IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<ClassResponse>> GetAllClassesAsync(int PageSize, int PageNumber)
    {
        var cacheKey = $"{AllClassesCacheKey}_p{PageNumber}_s{PageSize}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<ClassResponse>? cached))
            return cached!;

        var classes = await _context.Classes
            .Include(c => c.Students)
            .Include(c => c.teachers)
            .OrderBy(c => c.Id)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var result = classes.Select(c => c.ToDto());
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }

    public async Task<ClassResponse> GetClassByIdAsync(int ClassId)
    {
        var cacheKey = ClassByIdCacheKey(ClassId);

        if (_cache.TryGetValue(cacheKey, out ClassResponse? cached))
            return cached!;

        var cls = await _context.Classes
            .Include(c => c.Students)
            .Include(c => c.teachers)
            .FirstOrDefaultAsync(c => c.Id == ClassId);

        if (cls == null)
        {
            _logger.LogWarning("Class with ID {ClassId} not found", ClassId);
            return null!;
        }

        var result = cls.ToDto();
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }

    public async Task<ClassResponse> GetClassByNumberAsync(string ClassNumber)
    {
        var cls = await _context.Classes
            .Include(c => c.Students)
            .Include(c => c.teachers)
            .FirstOrDefaultAsync(c => c.ClassNumber == ClassNumber);

        if (cls == null)
        {
            _logger.LogWarning("Class '{ClassNumber}' not found", ClassNumber);
            return null!;
        }

        return cls.ToDto();
    }

    public async Task<ClassResponse> CreateClassAsync(CreateClassRequest request)
    {
        try
        {
            var exists = await _context.Classes.AnyAsync(c => c.ClassNumber == request.ClassNumber);
            if (exists)
                throw new InvalidOperationException("Class number already exists");

            var cls = new Class
            {
                ClassNumber = request.ClassNumber,
                StudentStep = request.StudentStep
            };

            _context.Classes.Add(cls);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Class '{ClassNumber}' created successfully", cls.ClassNumber);

            InvalidateCache();

            return cls.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating class: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<ClassResponse> UpdateClassAsync(int ClassId, UpdateClassRequest request)
    {
        try
        {
            var cls = await _context.Classes
                .Include(c => c.Students)
                .Include(c => c.teachers)
                .FirstOrDefaultAsync(c => c.Id == ClassId);

            if (cls == null)
            {
                _logger.LogWarning("Class with ID {ClassId} not found", ClassId);
                return null!;
            }

            if (request.ClassNumber != null) cls.ClassNumber = request.ClassNumber;
            if (request.StudentStep.HasValue) cls.StudentStep = request.StudentStep.Value;

            _context.Classes.Update(cls);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Class {ClassId} updated successfully", ClassId);

            InvalidateCache(ClassId);

            return cls.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error updating class: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteClassAsync(int ClassId)
    {
        try
        {
            var cls = await _context.Classes.FindAsync(ClassId);

            if (cls == null)
            {
                _logger.LogWarning("Class with ID {ClassId} not found", ClassId);
                return false;
            }

            _context.Classes.Remove(cls);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Class {ClassId} deleted successfully", ClassId);

            InvalidateCache(ClassId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error deleting class: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<int> ClassCountAsync()
    {
        return await _context.Classes.CountAsync();
    }

    private void InvalidateCache(int? id = null)
    {
        if (id.HasValue)
            _cache.Remove(ClassByIdCacheKey(id.Value));

        for (int page = 1; page <= 100; page++)
            for (int size = 1; size <= 100; size++)
                _cache.Remove($"{AllClassesCacheKey}_p{page}_s{size}");
    }
}
