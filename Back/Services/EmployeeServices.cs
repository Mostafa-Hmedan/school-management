using Back.Data;
using Back.Entities;
using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Back.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Back.Services;

public class EmployeeServices : IEmployeeServices
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmployeeServices> _logger;
    private readonly IImageService _imageService;
    private readonly IMemoryCache _cache;

    private const string AllEmployeesCacheKey = "all_employees";
    private static string EmployeeByIdCacheKey(int id) => $"employee_{id}";

    public EmployeeServices(
        AppDbContext context,
        ILogger<EmployeeServices> logger,
        IImageService imageService,
        IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _imageService = imageService;
        _cache = cache;
    }

    public async Task<IEnumerable<EmployeeResponse>> GetAllEmployeesAsync(int PageSize, int PageNumber)
    {
        var cacheKey = $"{AllEmployeesCacheKey}_p{PageNumber}_s{PageSize}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<EmployeeResponse>? cached))
            return cached!;

        var employees = await _context.Employees
            .OrderBy(e => e.Id)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var result = employees.Select(e => e.ToDto());
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }

    public async Task<EmployeeResponse?> GetEmployeeByIdAsync(int EmployeeId)
    {
        var cacheKey = EmployeeByIdCacheKey(EmployeeId);

        if (_cache.TryGetValue(cacheKey, out EmployeeResponse? cached))
            return cached!;

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == EmployeeId);

        if (employee == null)
        {
            _logger.LogWarning("Employee with ID {EmployeeId} not found", EmployeeId);
            return null;
        }

        var result = employee.ToDto();
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }

    public async Task<EmployeeResponse?> CreateEmployeeAsync(CreateEmployeeRequest request, IFormFile? image)
    {
        var employee = new Employee
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            City = request.City,
            JobTitle = request.JobTitle,
            Salary = request.Salary
        };

        if (image != null)
            employee.ImagePath = await _imageService.SaveImageAsync(image, "employees");

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        InvalidateCache();

        return employee.ToDto();
    }

    public async Task<EmployeeResponse?> UpdateEmployeeAsync(int EmployeeId, UpdateEmployeeRequest request, IFormFile? image)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == EmployeeId);

        if (employee == null)
            return null;

        if (request.FirstName != null) employee.FirstName = request.FirstName;
        if (request.LastName != null) employee.LastName = request.LastName;
        if (request.Phone != null) employee.Phone = request.Phone;
        if (request.City != null) employee.City = request.City;
        if (request.JobTitle != null) employee.JobTitle = request.JobTitle;
        if (request.Salary.HasValue) employee.Salary = request.Salary;

        if (image != null)
            employee.ImagePath = await _imageService.SaveImageAsync(image, "employees");

        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();

        InvalidateCache(EmployeeId);

        return employee.ToDto();
    }

    public async Task<bool> DeleteEmployeeAsync(int EmployeeId)
    {
        var employee = await _context.Employees.FindAsync(EmployeeId);

        if (employee == null)
            return false;

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        InvalidateCache(EmployeeId);

        return true;
    }

    public async Task<int> EmployeeCountAsync()
    {
        return await _context.Employees.CountAsync();
    }

    private void InvalidateCache(int? id = null)
    {
        if (id.HasValue)
            _cache.Remove(EmployeeByIdCacheKey(id.Value));

        for (int page = 1; page <= 50; page++)
            for (int size = 1; size <= 50; size++)
                _cache.Remove($"{AllEmployeesCacheKey}_p{page}_s{size}");
    }
}
