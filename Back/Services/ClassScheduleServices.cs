using Back.Data;
using Back.Entities;
using Back.Interfaces;
using Back.Requestes;
using Back.Responses;
using Microsoft.EntityFrameworkCore;
using Utilities;

namespace Back.Services;

public class ClassScheduleServices : IClassScheduleServices
{
    private readonly AppDbContext _context;

    public ClassScheduleServices(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<ClassScheduleResponse>> GetSchedulesByClass(int classId, int pageNumber, int pageSize)
    {
        var query = _context.ClassSchedules
            .Include(s => s.Class)
            .Include(s => s.Subject)
            .Include(s => s.Teacher)
            .Where(s => s.ClassId == classId);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => s.ToDto())
            .ToListAsync();

        return new PagedResponse<ClassScheduleResponse>(items, pageNumber, pageSize, totalCount);
    }

    public async Task<PagedResponse<ClassScheduleResponse>> GetSchedulesByTeacher(int teacherId, int pageNumber, int pageSize)
    {
        var query = _context.ClassSchedules
            .Include(s => s.Class)
            .Include(s => s.Subject)
            .Include(s => s.Teacher)
            .Where(s => s.TeacherId == teacherId);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => s.ToDto())
            .ToListAsync();

        return new PagedResponse<ClassScheduleResponse>(items, pageNumber, pageSize, totalCount);
    }

    public async Task<ClassScheduleResponse> GetSchedule(int id)
    {
        var schedule = await _context.ClassSchedules
            .Include(s => s.Class)
            .Include(s => s.Subject)
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync(s => s.Id == id);

        return schedule?.ToDto()!;
    }

    public async Task<ClassScheduleResponse> CreateSchedule(CreateClassScheduleRequest request)
    {
        await ValidateScheduleConflict(request.TeacherId, request.ClassId, request.DayOfWeek, request.StartTime, request.EndTime);

        var schedule = new ClassSchedule
        {
            ClassId = request.ClassId,
            SubjectId = request.SubjectId,
            TeacherId = request.TeacherId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        _context.ClassSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        // Load relations for DTO
        await _context.Entry(schedule).Reference(s => s.Class).LoadAsync();
        await _context.Entry(schedule).Reference(s => s.Subject).LoadAsync();
        await _context.Entry(schedule).Reference(s => s.Teacher).LoadAsync();

        return schedule.ToDto();
    }

    public async Task<ClassScheduleResponse> UpdateSchedule(int id, UpdateClassScheduleRequest request)
    {
        var schedule = await _context.ClassSchedules.FindAsync(id);
        if (schedule == null) return null!;

        await ValidateScheduleConflict(request.TeacherId, request.ClassId, request.DayOfWeek, request.StartTime, request.EndTime, id);

        schedule.ClassId = request.ClassId;
        schedule.SubjectId = request.SubjectId;
        schedule.TeacherId = request.TeacherId;
        schedule.DayOfWeek = request.DayOfWeek;
        schedule.StartTime = request.StartTime;
        schedule.EndTime = request.EndTime;

        await _context.SaveChangesAsync();

        await _context.Entry(schedule).Reference(s => s.Class).LoadAsync();
        await _context.Entry(schedule).Reference(s => s.Subject).LoadAsync();
        await _context.Entry(schedule).Reference(s => s.Teacher).LoadAsync();

        return schedule.ToDto();
    }

    public async Task<bool> DeleteSchedule(int id)
    {
        var schedule = await _context.ClassSchedules.FindAsync(id);
        if (schedule == null) return false;

        _context.ClassSchedules.Remove(schedule);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task ValidateScheduleConflict(int teacherId, int classId, DayOfWeek day, TimeSpan start, TimeSpan end, int? excludeId = null)
    {
        // 1. Check Teacher conflict (Teacher can't be in two places at once)
        var teacherConflict = await _context.ClassSchedules
            .AnyAsync(s => s.TeacherId == teacherId 
                        && s.DayOfWeek == day 
                        && s.Id != excludeId
                        && s.StartTime < end 
                        && s.EndTime > start);

        if (teacherConflict)
            throw new InvalidOperationException("الأستاذ لديه حصة أخرى في نفس هذا الوقت.");

        // 2. Check Class conflict (Class group can't have two sessions at once)
        var classConflict = await _context.ClassSchedules
            .AnyAsync(s => s.ClassId == classId 
                        && s.DayOfWeek == day 
                        && s.Id != excludeId
                        && s.StartTime < end 
                        && s.EndTime > start);

        if (classConflict)
            throw new InvalidOperationException("هذا الصف لديه حصة أخرى مسجلة في نفس هذا الوقت.");
            
        // 3. Optional: Verify against Teacher Availability (Admin override check)
        var isAvailable = await _context.TeacherAvailabilities
            .AnyAsync(a => a.TeacherId == teacherId 
                        && a.DayOfWeek == day 
                        && a.StartTime <= start 
                        && a.EndTime >= end);
        
        // Note: We might want to allow this but show a warning on frontend. 
        // For now, let's keep it as a soft validation or just skip to allow Admin flexibility.
        // If you want to force availability, uncomment below:
        // if (!isAvailable) throw new InvalidOperationException("الأستاذ غير متوفر في هذا الوقت بحسب جدول فراغه.");
    }
}
