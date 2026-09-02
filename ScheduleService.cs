using ABEC_System.Data;
using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using ABEC_System.Models;
using Microsoft.EntityFrameworkCore;

namespace ABEC_System.Services;

public class ScheduleService : IScheduleService
{
    private readonly ApplicationDbContext _db;

    public ScheduleService(ApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<IReadOnlyList<CourseScheduleDto>>> GetAllAsync(int? courseId = null, int? batchId = null, CancellationToken cancellationToken = default)
    {
        var query = _db.CourseSchedules.AsNoTracking()
            .Include(s => s.Course)
            .Include(s => s.Batch)
            .AsQueryable();

        if (courseId.HasValue) query = query.Where(s => s.CourseId == courseId.Value);
        if (batchId.HasValue) query = query.Where(s => s.BatchId == batchId.Value);

        var list = await query.OrderBy(s => s.ScheduleDate).ThenBy(s => s.StartTime).ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<CourseScheduleDto>>.Ok(list.Select(Map).ToList());
    }

    public async Task<ApiResponse<CourseScheduleDto>> CreateAsync(CreateScheduleDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.EndTime <= dto.StartTime)
            return ApiResponse<CourseScheduleDto>.Fail("End time must be after start time.");

        var conflict = await HasConflictAsync(dto, null, cancellationToken);
        if (conflict.Success && conflict.Data)
            return ApiResponse<CourseScheduleDto>.Fail("Schedule conflict detected for the selected room or time slot.");

        var entity = new CourseSchedule
        {
            CourseId = dto.CourseId,
            BatchId = dto.BatchId,
            ScheduleDate = dto.ScheduleDate.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Room = dto.Room.Trim()
        };

        _db.CourseSchedules.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        await _db.Entry(entity).Reference(s => s.Course).LoadAsync(cancellationToken);
        await _db.Entry(entity).Reference(s => s.Batch).LoadAsync(cancellationToken);
        return ApiResponse<CourseScheduleDto>.Ok(Map(entity), "Schedule created.");
    }

    public async Task<ApiResponse<CourseScheduleDto>> UpdateAsync(int id, UpdateScheduleDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _db.CourseSchedules
            .Include(s => s.Course)
            .Include(s => s.Batch)
            .FirstOrDefaultAsync(s => s.ScheduleId == id, cancellationToken);
        if (entity is null)
            return ApiResponse<CourseScheduleDto>.Fail("Schedule not found.");

        if (dto.ScheduleDate.HasValue) entity.ScheduleDate = dto.ScheduleDate.Value.Date;
        if (dto.StartTime.HasValue) entity.StartTime = dto.StartTime.Value;
        if (dto.EndTime.HasValue) entity.EndTime = dto.EndTime.Value;
        if (!string.IsNullOrWhiteSpace(dto.Room)) entity.Room = dto.Room.Trim();
        if (dto.BatchId.HasValue) entity.BatchId = dto.BatchId.Value;

        if (entity.EndTime <= entity.StartTime)
            return ApiResponse<CourseScheduleDto>.Fail("End time must be after start time.");

        var conflictCheck = await HasConflictAsync(new CreateScheduleDto
        {
            CourseId = entity.CourseId,
            BatchId = entity.BatchId,
            ScheduleDate = entity.ScheduleDate,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            Room = entity.Room
        }, id, cancellationToken);

        if (conflictCheck.Success && conflictCheck.Data)
            return ApiResponse<CourseScheduleDto>.Fail("Schedule conflict detected for the selected room or time slot.");

        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<CourseScheduleDto>.Ok(Map(entity), "Schedule updated.");
    }

    public async Task<ApiResponse<object>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.CourseSchedules.FirstOrDefaultAsync(s => s.ScheduleId == id, cancellationToken);
        if (entity is null)
            return ApiResponse<object>.Fail("Schedule not found.");

        _db.CourseSchedules.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { deleted = true }, "Schedule deleted.");
    }

    public async Task<ApiResponse<bool>> HasConflictAsync(CreateScheduleDto dto, int? excludeScheduleId = null, CancellationToken cancellationToken = default)
    {
        var query = _db.CourseSchedules.AsNoTracking()
            .Where(s => s.ScheduleDate.Date == dto.ScheduleDate.Date && s.Room == dto.Room.Trim());

        if (excludeScheduleId.HasValue)
            query = query.Where(s => s.ScheduleId != excludeScheduleId.Value);

        var sameDay = await query.ToListAsync(cancellationToken);
        var conflict = sameDay.Any(s => TimesOverlap(s.StartTime, s.EndTime, dto.StartTime, dto.EndTime));
        return ApiResponse<bool>.Ok(conflict);
    }

    private static bool TimesOverlap(TimeSpan aStart, TimeSpan aEnd, TimeSpan bStart, TimeSpan bEnd)
        => aStart < bEnd && bStart < aEnd;

    private static CourseScheduleDto Map(CourseSchedule s) => new()
    {
        ScheduleId = s.ScheduleId,
        CourseId = s.CourseId,
        CourseName = s.Course?.CourseName ?? string.Empty,
        BatchId = s.BatchId,
        BatchName = s.Batch?.BatchName ?? string.Empty,
        ScheduleDate = s.ScheduleDate,
        StartTime = s.StartTime,
        EndTime = s.EndTime,
        Room = s.Room
    };
}
