using ABEC_System.Data;
using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ABEC_System.Services;

public class StudentPortalService : IStudentPortalService
{
    private readonly ApplicationDbContext _db;
    private readonly IDocumentRequestService _documents;
    private readonly INotificationService _notifications;
    private readonly IScheduleService _schedules;

    public StudentPortalService(
        ApplicationDbContext db,
        IDocumentRequestService documents,
        INotificationService notifications,
        IScheduleService schedules)
    {
        _db = db;
        _documents = documents;
        _notifications = notifications;
        _schedules = schedules;
    }

    public async Task<ApiResponse<StudentDashboardDto>> GetDashboardAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var profile = await GetProfileAsync(studentId, cancellationToken);
        if (!profile.Success || profile.Data is null)
            return ApiResponse<StudentDashboardDto>.Fail(profile.Message);

        var schedule = await GetScheduleAsync(studentId, cancellationToken);
        var docs = await _documents.GetByStudentAsync(studentId, cancellationToken);
        var notifs = await _notifications.GetForStudentAsync(studentId, cancellationToken: cancellationToken);

        return ApiResponse<StudentDashboardDto>.Ok(new StudentDashboardDto
        {
            Profile = profile.Data,
            UpcomingClasses = schedule.Data?.Take(5).ToList() ?? new List<CourseScheduleDto>(),
            RecentDocumentRequests = docs.Data?.Take(5).ToList() ?? new List<DocumentRequestDto>(),
            RecentNotifications = notifs.Data?.Take(5).ToList() ?? new List<NotificationDto>()
        });
    }

    public async Task<ApiResponse<StudentDto>> GetProfileAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var student = await _db.Students.AsNoTracking()
            .Include(s => s.Applicant)!.ThenInclude(a => a!.Course)
            .Include(s => s.Batch)
            .FirstOrDefaultAsync(s => s.StudentId == studentId, cancellationToken);

        if (student is null)
            return ApiResponse<StudentDto>.Fail("Student not found.");

        return ApiResponse<StudentDto>.Ok(new StudentDto
        {
            StudentId = student.StudentId,
            ApplicantId = student.ApplicantId,
            FullName = student.Applicant?.FullName ?? string.Empty,
            Email = student.Applicant?.Email ?? string.Empty,
            ContactNumber = student.Applicant?.ContactNumber ?? string.Empty,
            CourseName = student.Applicant?.Course?.CourseName ?? string.Empty,
            CourseId = student.Applicant?.CourseId ?? 0,
            BatchId = student.BatchId,
            BatchName = student.Batch?.BatchName ?? string.Empty,
            SecurityPin = student.SecurityPin,
            AccountStatus = student.AccountStatus,
            CreatedAt = student.CreatedAt
        });
    }

    public async Task<ApiResponse<StudentDto>> UpdateProfileAsync(int studentId, UpdateStudentProfileDto dto, CancellationToken cancellationToken = default)
    {
        var student = await _db.Students
            .Include(s => s.Applicant)!.ThenInclude(a => a!.Course)
            .Include(s => s.Batch)
            .FirstOrDefaultAsync(s => s.StudentId == studentId, cancellationToken);

        if (student?.Applicant is null)
            return ApiResponse<StudentDto>.Fail("Student not found.");

        student.Applicant.FirstName = dto.FirstName.Trim();
        student.Applicant.MiddleName = dto.MiddleName?.Trim();
        student.Applicant.LastName = dto.LastName.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Gender)) student.Applicant.Gender = dto.Gender.Trim();
        if (dto.Birthdate.HasValue) student.Applicant.Birthdate = dto.Birthdate.Value.Date;
        if (!string.IsNullOrWhiteSpace(dto.Email)) student.Applicant.Email = dto.Email.Trim();
        if (!string.IsNullOrWhiteSpace(dto.ContactNumber)) student.Applicant.ContactNumber = dto.ContactNumber.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Address)) student.Applicant.Address = dto.Address.Trim();

        await _db.SaveChangesAsync(cancellationToken);
        return await GetProfileAsync(studentId, cancellationToken);
    }

    public async Task<ApiResponse<IReadOnlyList<CourseScheduleDto>>> GetScheduleAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var student = await _db.Students.AsNoTracking()
            .Include(s => s.Applicant)
            .FirstOrDefaultAsync(s => s.StudentId == studentId, cancellationToken);

        if (student?.Applicant is null)
            return ApiResponse<IReadOnlyList<CourseScheduleDto>>.Fail("Student not found.");

        return await _schedules.GetAllAsync(student.Applicant.CourseId, student.BatchId, cancellationToken);
    }
}
