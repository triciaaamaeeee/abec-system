using ABEC_System.Constants;
using ABEC_System.Data;
using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using ABEC_System.Models;
using Microsoft.EntityFrameworkCore;

namespace ABEC_System.Services;

public class ApplicantService : IApplicantService
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationService _notifications;

    public ApplicantService(ApplicationDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var stats = new DashboardStatsDto
        {
            TotalApplicants = await _db.Applicants.CountAsync(cancellationToken),
            Pending = await _db.Applicants.CountAsync(a => a.ApplicationStatus == SystemConstants.ApplicationStatuses.Pending, cancellationToken),
            Waitlisted = await _db.Applicants.CountAsync(a => a.ApplicationStatus == SystemConstants.ApplicationStatuses.Waitlisted, cancellationToken),
            Approved = await _db.Applicants.CountAsync(a => a.ApplicationStatus == SystemConstants.ApplicationStatuses.Approved, cancellationToken),
            Rejected = await _db.Applicants.CountAsync(a => a.ApplicationStatus == SystemConstants.ApplicationStatuses.Rejected, cancellationToken)
        };
        return ApiResponse<DashboardStatsDto>.Ok(stats);
    }

    public async Task<ApiResponse<IReadOnlyList<ApplicantDto>>> GetApplicantsAsync(
        string? status = null, int? courseId = null, string? search = null, int? take = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Applicants.AsNoTracking().Include(a => a.Course).Include(a => a.Student).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
            query = query.Where(a => a.ApplicationStatus == status);

        if (courseId.HasValue)
            query = query.Where(a => a.CourseId == courseId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(a =>
                a.FirstName.ToLower().Contains(term) ||
                a.LastName.ToLower().Contains(term) ||
                a.Email.ToLower().Contains(term));
        }

        query = query.OrderByDescending(a => a.ApplicationDate);
        if (take.HasValue) query = query.Take(take.Value);

        var list = await query.ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<ApplicantDto>>.Ok(list.Select(Map).ToList());
    }

    public async Task<ApiResponse<ApplicantDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var applicant = await _db.Applicants.AsNoTracking()
            .Include(a => a.Course)
            .Include(a => a.Student)
            .FirstOrDefaultAsync(a => a.ApplicantId == id, cancellationToken);

        return applicant is null
            ? ApiResponse<ApplicantDto>.Fail("Applicant not found.")
            : ApiResponse<ApplicantDto>.Ok(Map(applicant));
    }

    public async Task<ApiResponse<ApplicantDto>> EnrollAsync(EnrollApplicantDto dto, CancellationToken cancellationToken = default)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == dto.CourseId, cancellationToken);
        if (course is null)
            return ApiResponse<ApplicantDto>.Fail("Selected course was not found.");

        var email = dto.Email.Trim().ToLowerInvariant();
        var exists = await _db.Applicants.AnyAsync(a => a.Email.ToLower() == email, cancellationToken);
        if (exists)
            return ApiResponse<ApplicantDto>.Fail("An application with this email already exists.");

        var names = SplitFullName(dto.FullName);
        var applicant = new Applicant
        {
            FirstName = names.first,
            MiddleName = names.middle,
            LastName = names.last,
            Gender = string.IsNullOrWhiteSpace(dto.Gender) ? "Not Specified" : dto.Gender,
            Birthdate = dto.Birthdate ?? DateTime.UtcNow.Date.AddYears(-18),
            ContactNumber = dto.ContactNumber.Trim(),
            Email = dto.Email.Trim(),
            Address = dto.Address.Trim(),
            CourseId = dto.CourseId,
            ApplicationStatus = SystemConstants.ApplicationStatuses.Pending,
            ApplicationDate = DateTime.Now
        };

        _db.Applicants.Add(applicant);
        await _db.SaveChangesAsync(cancellationToken);

        await _notifications.CreateAsync(
            "Application Received",
            "Your enrollment application has been submitted and is pending review.",
            applicantId: applicant.ApplicantId,
            cancellationToken: cancellationToken);

        await _db.Entry(applicant).Reference(a => a.Course).LoadAsync(cancellationToken);
        return ApiResponse<ApplicantDto>.Ok(Map(applicant), "Application submitted successfully.");
    }

    public async Task<ApiResponse<ApplicantDto>> UpdateStatusAsync(int id, string status, CancellationToken cancellationToken = default)
    {
        if (!IsValidStatus(status))
            return ApiResponse<ApplicantDto>.Fail("Invalid application status.");

        var applicant = await _db.Applicants.Include(a => a.Course).Include(a => a.Student)
            .FirstOrDefaultAsync(a => a.ApplicantId == id, cancellationToken);
        if (applicant is null)
            return ApiResponse<ApplicantDto>.Fail("Applicant not found.");

        applicant.ApplicationStatus = status;
        await _db.SaveChangesAsync(cancellationToken);

        await _notifications.CreateAsync(
            "Application Status Updated",
            $"Your application status is now: {status}.",
            applicantId: applicant.ApplicantId,
            cancellationToken: cancellationToken);

        return ApiResponse<ApplicantDto>.Ok(Map(applicant), "Applicant status updated.");
    }

    public async Task<ApiResponse<object>> BulkUpdateStatusAsync(BulkStatusDto dto, CancellationToken cancellationToken = default)
    {
        if (!IsValidStatus(dto.Status))
            return ApiResponse<object>.Fail("Invalid application status.");

        var applicants = await _db.Applicants.Where(a => dto.Ids.Contains(a.ApplicantId)).ToListAsync(cancellationToken);
        foreach (var applicant in applicants)
            applicant.ApplicationStatus = dto.Status;

        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { updated = applicants.Count }, "Bulk status update completed.");
    }

    public async Task<ApiResponse<StudentDto>> AddToSlotAsync(int applicantId, CancellationToken cancellationToken = default)
    {
        var applicant = await _db.Applicants
            .Include(a => a.Course)
            .Include(a => a.Student)
            .FirstOrDefaultAsync(a => a.ApplicantId == applicantId, cancellationToken);

        if (applicant is null)
            return ApiResponse<StudentDto>.Fail("Applicant not found.");
        if (applicant.ApplicationStatus != SystemConstants.ApplicationStatuses.Approved)
            return ApiResponse<StudentDto>.Fail("Only approved applicants can be added to a course slot.");
        if (applicant.Student is not null)
            return ApiResponse<StudentDto>.Fail("Applicant already has a student account.");

        var activeBatch = await _db.Batches.FirstOrDefaultAsync(b => b.Status == SystemConstants.BatchStatuses.Active, cancellationToken);
        if (activeBatch is null)
            return ApiResponse<StudentDto>.Fail("No active batch is available.");

        var filled = await _db.Students.Include(s => s.Applicant)
            .CountAsync(s => s.BatchId == activeBatch.BatchId && s.Applicant!.CourseId == applicant.CourseId, cancellationToken);

        if (filled >= SystemConstants.DefaultCourseCapacity)
            return ApiResponse<StudentDto>.Fail("Course slot capacity has been reached.");

        var pin = await GenerateUniquePinAsync(cancellationToken);
        var student = new Student
        {
            ApplicantId = applicant.ApplicantId,
            BatchId = activeBatch.BatchId,
            SecurityPin = pin,
            AccountStatus = SystemConstants.AccountStatuses.Inactive,
            CreatedAt = DateTime.Now
        };

        _db.Students.Add(student);
        await _db.SaveChangesAsync(cancellationToken);

        await _notifications.CreateAsync(
            "Slot Assigned",
            $"You have been assigned to {applicant.Course?.CourseName}. Your security PIN will be provided by the registrar.",
            applicantId: applicant.ApplicantId,
            cancellationToken: cancellationToken);

        student.Applicant = applicant;
        student.Batch = activeBatch;
        return ApiResponse<StudentDto>.Ok(MapStudent(student), "Applicant added to course slot.");
    }

    private async Task<string> GenerateUniquePinAsync(CancellationToken cancellationToken)
    {
        string pin;
        do
        {
            pin = $"ABEC-{Random.Shared.Next(100000, 999999)}";
        } while (await _db.Students.AnyAsync(s => s.SecurityPin == pin, cancellationToken));
        return pin;
    }

    private static bool IsValidStatus(string status) =>
        status is SystemConstants.ApplicationStatuses.Pending
            or SystemConstants.ApplicationStatuses.Approved
            or SystemConstants.ApplicationStatuses.Rejected
            or SystemConstants.ApplicationStatuses.Waitlisted;

    private static (string first, string? middle, string last) SplitFullName(string fullName)
    {
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => ("Unknown", null, "Applicant"),
            1 => (parts[0], null, parts[0]),
            2 => (parts[0], null, parts[1]),
            _ => (parts[0], string.Join(' ', parts[1..^1]), parts[^1])
        };
    }

    private static ApplicantDto Map(Applicant a) => new()
    {
        ApplicantId = a.ApplicantId,
        FullName = a.FullName,
        FirstName = a.FirstName,
        MiddleName = a.MiddleName,
        LastName = a.LastName,
        Email = a.Email,
        ContactNumber = a.ContactNumber,
        Address = a.Address,
        Gender = a.Gender,
        Birthdate = a.Birthdate,
        CourseId = a.CourseId,
        CourseName = a.Course?.CourseName ?? string.Empty,
        ApplicationStatus = a.ApplicationStatus,
        ApplicationDate = a.ApplicationDate,
        HasStudentAccount = a.Student is not null
    };

    private static StudentDto MapStudent(Student s) => new()
    {
        StudentId = s.StudentId,
        ApplicantId = s.ApplicantId,
        FullName = s.Applicant?.FullName ?? string.Empty,
        Email = s.Applicant?.Email ?? string.Empty,
        ContactNumber = s.Applicant?.ContactNumber ?? string.Empty,
        CourseName = s.Applicant?.Course?.CourseName ?? string.Empty,
        CourseId = s.Applicant?.CourseId ?? 0,
        BatchId = s.BatchId,
        BatchName = s.Batch?.BatchName ?? string.Empty,
        SecurityPin = s.SecurityPin,
        AccountStatus = s.AccountStatus,
        CreatedAt = s.CreatedAt
    };
}
