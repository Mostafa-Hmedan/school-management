using Back.Data;
using Back.Entities;
using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.EntityFrameworkCore;
using Utilities;

namespace Back.Services;

public class TeacherAvailabilityServices : ITeacherAvailabilityServices
{
    private readonly AppDbContext _context;

    public TeacherAvailabilityServices(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<TeacherAvailabilityResponse>> GetTeacherAvailabilities(int teacherId, int pageNumber, int pageSize)
    {
        var query = _context.TeacherAvailabilities
            .Include(a => a.Teacher)
            .Where(a => a.TeacherId == teacherId);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(a => a.DayOfWeek)
            .ThenBy(a => a.StartTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => a.ToDto())
            .ToListAsync();

        return new PagedResponse<TeacherAvailabilityResponse>(items, pageNumber, pageSize, totalCount);
    }

    public async Task<TeacherAvailabilityResponse> GetTeacherAvailability(int id)
    {
        var availability = await _context.TeacherAvailabilities
            .Include(a => a.Teacher)
            .FirstOrDefaultAsync(a => a.Id == id);

        return availability?.ToDto()!;
    }

    public async Task<TeacherAvailabilityResponse> CreateTeacherAvailability(CreateTeacherAvailabilityRequest request)
    {
        var availability = new TeacherAvailability
        {
            TeacherId = request.TeacherId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        _context.TeacherAvailabilities.Add(availability);
        await _context.SaveChangesAsync();

        // Reload to get Teacher details for DTO
        await _context.Entry(availability).Reference(a => a.Teacher).LoadAsync();

        return availability.ToDto();
    }

    public async Task<TeacherAvailabilityResponse> UpdateTeacherAvailability(int id, UpdateTeacherAvailabilityRequest request)
    {
        var availability = await _context.TeacherAvailabilities.FindAsync(id);
        if (availability == null) return null!;

        availability.DayOfWeek = request.DayOfWeek;
        availability.StartTime = request.StartTime;
        availability.EndTime = request.EndTime;

        await _context.SaveChangesAsync();
        
        await _context.Entry(availability).Reference(a => a.Teacher).LoadAsync();

        return availability.ToDto();
    }

    public async Task<bool> DeleteTeacherAvailability(int id)
    {
        var availability = await _context.TeacherAvailabilities.FindAsync(id);
        if (availability == null) return false;

        _context.TeacherAvailabilities.Remove(availability);
        await _context.SaveChangesAsync();
        return true;
    }
}
