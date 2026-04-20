using Back.Data;
using Back.Entities;
using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Back.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Back.Services;

public class TeacherServices : ITeacherServices
{
    private readonly AppDbContext _context;
    private readonly ILogger<TeacherServices> _logger;
    private readonly IImageService _imageService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMemoryCache _cache;

    private const string AllTeachersCacheKey = "all_teachers";
    private static string TeacherByIdCacheKey(int id) => $"teacher_{id}";

    public TeacherServices(
        AppDbContext context,
        ILogger<TeacherServices> logger,
        IImageService imageService,
        UserManager<AppUser> userManager,
        IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _imageService = imageService;
        _userManager = userManager;
        _cache = cache;
    }

    public async Task<IEnumerable<TeacherResponse>> GetAllTeacherAsync(int PageSize, int PageNumber)
    {
        var cacheKey = $"{AllTeachersCacheKey}_p{PageNumber}_s{PageSize}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<TeacherResponse>? cached))
            return cached!;

        var teachers = await _context.Teachers
            .Include(t => t.Class)
            .Include(t => t.Subject)
            .OrderBy(t => t.Id)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var result = teachers.Select(t => t.ToDto());
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }

    public async Task<TeacherResponse> GetTeacherByIdAsync(int TeacherId)
    {
        var cacheKey = TeacherByIdCacheKey(TeacherId);

        if (_cache.TryGetValue(cacheKey, out TeacherResponse? cached))
            return cached!;

        var teacher = await _context.Teachers
            .Include(t => t.Class)
            .Include(t => t.Subject)
            .FirstOrDefaultAsync(t => t.Id == TeacherId);

        if (teacher == null)
        {
            _logger.LogWarning("Teacher with ID {TeacherId} not found", TeacherId);
            return null!;
        }

        var result = teacher.ToDto();
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }

    public async Task<IEnumerable<TeacherResponse>> GetTeacherByNameAsync(string TeacherName)
    {
        var teachers = await _context.Teachers
            .Include(t => t.Class)
            .Include(t => t.Subject)
            .Where(t => t.FirstName.Contains(TeacherName) || t.LastName.Contains(TeacherName))
            .ToListAsync();

        return teachers.Select(t => t.ToDto());
    }

    public async Task<IEnumerable<TeacherResponse>> GetTeacherByClassAsync(string ClassName)
    {
        var teachers = await _context.Teachers
            .Include(t => t.Class)
            .Include(t => t.Subject)
            .Where(t => t.Class.ClassNumber == ClassName)
            .ToListAsync();

        return teachers.Select(t => t.ToDto());
    }

    public async Task<IEnumerable<TeacherResponse>> GetTeacherBySubjectAsync(string SubjectName)
    {
        var teachers = await _context.Teachers
            .Include(t => t.Class)
            .Include(t => t.Subject)
            .Where(t => t.Subject.SubjectName == SubjectName)
            .ToListAsync();

        return teachers.Select(t => t.ToDto());
    }

    public async Task<TeacherResponse> CreateTeacherAsync(CreateTeacherRequest request)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Email already exists");

            var appUser = new AppUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            var result = await _userManager.CreateAsync(appUser, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            await _userManager.AddToRoleAsync(appUser, "Teacher");

            var teacher = new Teacher
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                City = request.City,
                Phone = request.Phone,
                SubjectId = request.SubjectId,
                ClassId = request.ClassId,
                AppUserId = appUser.Id
            };

            if (request.Image != null)
                teacher.ImagePath = await _imageService.SaveImageAsync(request.Image, "teachers");

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            await _context.Entry(teacher).Reference(t => t.Class).LoadAsync();
            await _context.Entry(teacher).Reference(t => t.Subject).LoadAsync();

            _logger.LogInformation("Teacher {Name} created successfully", teacher.FirstName);

            InvalidateCache();

            return teacher.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating teacher: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<TeacherResponse> UpdateTeacherAsync(int TeacherId, UpdateTeacherRequest request)
    {
        try
        {
            var teacher = await _context.Teachers
                .Include(t => t.Class)
                .Include(t => t.Subject)
                .FirstOrDefaultAsync(t => t.Id == TeacherId);

            if (teacher == null)
            {
                _logger.LogWarning("Teacher with ID {TeacherId} not found", TeacherId);
                return null!;
            }

            if (request.FirstName != null) teacher.FirstName = request.FirstName;
            if (request.LastName != null) teacher.LastName = request.LastName;
            if (request.City != null) teacher.City = request.City;
            if (request.Phone != null) teacher.Phone = request.Phone;
            if (request.SubjectId.HasValue) teacher.SubjectId = request.SubjectId.Value;
            if (request.ClassId.HasValue) teacher.ClassId = request.ClassId.Value;

            if (request.Image != null)
            {
                if (!string.IsNullOrEmpty(teacher.ImagePath))
                    _imageService.DeleteImage(teacher.ImagePath);

                teacher.ImagePath = await _imageService.SaveImageAsync(request.Image, "teachers");
            }

            _context.Teachers.Update(teacher);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Teacher {TeacherId} updated successfully", TeacherId);

            InvalidateCache(TeacherId);

            return teacher.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error updating teacher: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteTeacherAsync(int TeacherId)
    {
        try
        {
            var teacher = await _context.Teachers.FindAsync(TeacherId);

            if (teacher == null)
            {
                _logger.LogWarning("Teacher with ID {TeacherId} not found", TeacherId);
                return false;
            }

            if (!string.IsNullOrEmpty(teacher.ImagePath))
                _imageService.DeleteImage(teacher.ImagePath);

            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(teacher.AppUserId))
            {
                var appUser = await _userManager.FindByIdAsync(teacher.AppUserId);
                if (appUser != null)
                    await _userManager.DeleteAsync(appUser);
            }

            _logger.LogInformation("Teacher {TeacherId} deleted successfully", TeacherId);

            InvalidateCache(TeacherId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error deleting teacher: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<int> TeacherCountAsync()
    {
        return await _context.Teachers.CountAsync();
    }

    public async Task<TeacherResponse> GetTeacherByUserIdAsync(string userId)
    {
        var teacher = await _context.Teachers
            .Include(t => t.Class)
            .Include(t => t.Subject)
            .FirstOrDefaultAsync(t => t.AppUserId == userId);

        return teacher?.ToDto()!;
    }

    private void InvalidateCache(int? id = null)
    {
        if (id.HasValue)
            _cache.Remove(TeacherByIdCacheKey(id.Value));

        for (int page = 1; page <= 100; page++)
            for (int size = 1; size <= 100; size++)
                _cache.Remove($"{AllTeachersCacheKey}_p{page}_s{size}");
    }
}
