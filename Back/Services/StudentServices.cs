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

public class StudentServices : IStudentServices
{
    private readonly AppDbContext _context;
    private readonly ILogger<StudentServices> _logger;
    private readonly IImageService _imageService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMemoryCache _cache;

    private const string AllStudentsCacheKey = "all_students";
    private static string StudentByIdCacheKey(int id) => $"student_{id}";

    public StudentServices(
        AppDbContext context,
        ILogger<StudentServices> logger,
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

    public async Task<IEnumerable<StudentResponse>> GetAllStudentAsync(int PageSize, int PageNumber)
    {
        var cacheKey = $"{AllStudentsCacheKey}_p{PageNumber}_s{PageSize}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<StudentResponse>? cached))
            return cached!;

        var students = await _context.Students
            .Include(s => s.Class)
            .OrderBy(s => s.Id)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var result = StudentResponse.ToDto(students);
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }

    public async Task<StudentResponse> GetStudentByIdAsync(int StudentId)
    {
        var cacheKey = StudentByIdCacheKey(StudentId);

        if (_cache.TryGetValue(cacheKey, out StudentResponse? cached))
            return cached!;

        var student = await _context.Students
            .Include(s => s.Class)
            .FirstOrDefaultAsync(s => s.Id == StudentId);

        if (student == null)
        {
            _logger.LogWarning("Student with ID {StudentId} not found", StudentId);
            return null!;
        }

        var result = StudentResponse.ToDto(student);
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }

    public async Task<IEnumerable<StudentResponse>> GetStudentByNameAsync(string StudentName)
    {
        var students = await _context.Students
            .Include(s => s.Class)
            .Where(s => s.FirstName.Contains(StudentName) || s.LastName.Contains(StudentName))
            .ToListAsync();

        return StudentResponse.ToDto(students);
    }

    public async Task<IEnumerable<StudentResponse>> GetStudentByClassAsync(string ClassName)
    {
        var students = await _context.Students
            .Include(s => s.Class)
            .Where(s => s.Class.ClassNumber == ClassName)
            .ToListAsync();

        return StudentResponse.ToDto(students);
    }

    public async Task<StudentResponse> CreateStudentAsync(CreateStudentRequest request)
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

            await _userManager.AddToRoleAsync(appUser, "Student");

            var student = new Student
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                City = request.City,
                Phone = request.Phone,
                BirthDay = request.BirthDay,
                ClassId = request.ClassId,
                AppUserId = appUser.Id
            };

            if (request.Image != null)
                student.ImagePath = await _imageService.SaveImageAsync(request.Image, "students");

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            await _context.Entry(student).Reference(s => s.Class).LoadAsync();

            _logger.LogInformation("Student {Name} created successfully", student.FirstName);

            InvalidateCache();

            return StudentResponse.ToDto(student);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating student: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<StudentResponse> UpdateStudentAsync(int StudentId, UpdateStudentRequest request)
    {
        try
        {
            var student = await _context.Students.FindAsync(StudentId);

            if (student == null)
            {
                _logger.LogWarning("Student with ID {StudentId} not found", StudentId);
                return null!;
            }

            student.FirstName = request.FirstName;
            student.LastName = request.LastName;
            student.City = request.City;
            student.Phone = request.Phone;
            student.BirthDay = request.BirthDay;
            student.ClassId = request.ClassId;

            if (request.Image != null)
            {
                if (!string.IsNullOrEmpty(student.ImagePath))
                    _imageService.DeleteImage(student.ImagePath);

                student.ImagePath = await _imageService.SaveImageAsync(request.Image, "students");
            }

            _context.Students.Update(student);
            await _context.SaveChangesAsync();

            await _context.Entry(student).Reference(s => s.Class).LoadAsync();

            _logger.LogInformation("Student {StudentId} updated successfully", StudentId);

            InvalidateCache(StudentId);

            return StudentResponse.ToDto(student);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error updating student: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteStudentAsync(int studentId)
    {
        try
        {
            var student = await _context.Students.FindAsync(studentId);

            if (student == null)
            {
                _logger.LogWarning("Student with ID {StudentId} not found", studentId);
                return false;
            }

            if (!string.IsNullOrEmpty(student.ImagePath))
                _imageService.DeleteImage(student.ImagePath);

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(student.AppUserId))
            {
                var appUser = await _userManager.FindByIdAsync(student.AppUserId);
                if (appUser != null)
                    await _userManager.DeleteAsync(appUser);
            }

            _logger.LogInformation("Student {StudentId} deleted successfully", studentId);

            InvalidateCache(studentId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error deleting student: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<int> StudentCount()
    {
        return await _context.Students.CountAsync();
    }

    public async Task<StudentResponse> GetStudentByUserIdAsync(string userId)
    {
        var student = await _context.Students
            .Include(s => s.Class)
            .FirstOrDefaultAsync(s => s.AppUserId == userId);

        return StudentResponse.ToDto(student!);
    }

    private void InvalidateCache(int? id = null)
    {
        if (id.HasValue)
            _cache.Remove(StudentByIdCacheKey(id.Value));

        for (int page = 1; page <= 100; page++)
            for (int size = 1; size <= 100; size++)
                _cache.Remove($"{AllStudentsCacheKey}_p{page}_s{size}");
    }
}
