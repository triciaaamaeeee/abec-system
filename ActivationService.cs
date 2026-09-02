using ABEC_System.Constants;
using ABEC_System.Data;
using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ABEC_System.Services;

public class ActivationService : IActivationService
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationService _notifications;

    public ActivationService(ApplicationDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<ApiResponse<ActivationSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var approvedApplicants = await _db.Applicants.CountAsync(a => a.ApplicationStatus == SystemConstants.ApplicationStatuses.Approved, cancellationToken);
        var students = await _db.Students.AsNoTracking().ToListAsync(cancellationToken);
        var summary = new ActivationSummaryDto
        {
            TotalApprovedStudents = approvedApplicants,
            PendingActivation = students.Count(s => s.AccountStatus == SystemConstants.AccountStatuses.Inactive),
            ActivatedAccounts = students.Count(s => s.AccountStatus == SystemConstants.AccountStatuses.Active),
            DeactivatedAccounts = students.Count(s => s.AccountStatus == SystemConstants.AccountStatuses.Inactive && !string.IsNullOrWhiteSpace(s.SecurityPin))
        };
        return ApiResponse<ActivationSummaryDto>.Ok(summary);
    }

    public async Task<ApiResponse<IReadOnlyList<StudentDto>>> GetAccountsAsync(
        string? status = null, int? courseId = null, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Students.AsNoTracking()
            .Include(s => s.Applicant)!.ThenInclude(a => a!.Course)
            .Include(s => s.Batch)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(status, "pending", StringComparison.OrdinalIgnoreCase))
                query = query.Where(s => s.AccountStatus == SystemConstants.AccountStatuses.Inactive);
            else if (string.Equals(status, "activated", StringComparison.OrdinalIgnoreCase))
                query = query.Where(s => s.AccountStatus == SystemConstants.AccountStatuses.Active);
            else if (string.Equals(status, "deactivated", StringComparison.OrdinalIgnoreCase))
                query = query.Where(s => s.AccountStatus == SystemConstants.AccountStatuses.Inactive);
            else
                query = query.Where(s => s.AccountStatus == status);
        }

        if (courseId.HasValue)
            query = query.Where(s => s.Applicant != null && s.Applicant.CourseId == courseId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s =>
                s.Applicant!.FirstName.ToLower().Contains(term) ||
                s.Applicant.LastName.ToLower().Contains(term) ||
                s.Applicant.Email.ToLower().Contains(term));
        }

        var list = await query.OrderByDescending(s => s.CreatedAt).ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<StudentDto>>.Ok(list.Select(Map).ToList());
    }

    public async Task<ApiResponse<StudentDto>> GeneratePinAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var student = await LoadStudentAsync(studentId, cancellationToken);
        if (student is null)
            return ApiResponse<StudentDto>.Fail("Student account not found.");

        string pin;
        do
        {
            pin = $"ABEC-{Random.Shared.Next(100000, 999999)}";
        } while (await _db.Students.AnyAsync(s => s.SecurityPin == pin && s.StudentId != studentId, cancellationToken));

        student.SecurityPin = pin;
        student.AccountStatus = SystemConstants.AccountStatuses.Inactive;
        await _db.SaveChangesAsync(cancellationToken);

        await _notifications.CreateAsync(
            "Security PIN Generated",
            "Your security PIN has been generated. Please collect it from the registrar to activate your account.",
            studentId: student.StudentId,
            cancellationToken: cancellationToken);

        return ApiResponse<StudentDto>.Ok(Map(student), "Security PIN generated.");
    }

    public async Task<ApiResponse<StudentDto>> ActivateAsync(int studentId, CancellationToken cancellationToken = default)
        => await SetStatusAsync(studentId, SystemConstants.AccountStatuses.Active, "Account activated.", cancellationToken);

    public async Task<ApiResponse<StudentDto>> DeactivateAsync(int studentId, CancellationToken cancellationToken = default)
        => await SetStatusAsync(studentId, SystemConstants.AccountStatuses.Inactive, "Account deactivated.", cancellationToken);

    public async Task<ApiResponse<StudentDto>> ReactivateAsync(int studentId, CancellationToken cancellationToken = default)
        => await SetStatusAsync(studentId, SystemConstants.AccountStatuses.Active, "Account reactivated.", cancellationToken);

    public async Task<ApiResponse<object>> BulkUpdateStatusAsync(BulkStatusDto dto, CancellationToken cancellationToken = default)
    {
        var status = NormalizeStatus(dto.Status);
        if (status is null)
            return ApiResponse<object>.Fail("Invalid account status.");

        var students = await _db.Students.Where(s => dto.Ids.Contains(s.StudentId)).ToListAsync(cancellationToken);
        foreach (var student in students)
            student.AccountStatus = status;

        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { updated = students.Count }, "Bulk activation update completed.");
    }

    private async Task<ApiResponse<StudentDto>> SetStatusAsync(int studentId, string status, string message, CancellationToken cancellationToken)
    {
        var student = await LoadStudentAsync(studentId, cancellationToken);
        if (student is null)
            return ApiResponse<StudentDto>.Fail("Student account not found.");

        student.AccountStatus = status;
        await _db.SaveChangesAsync(cancellationToken);
        await _notifications.CreateAsync("Account Status Updated", message, studentId: student.StudentId, cancellationToken: cancellationToken);
        return ApiResponse<StudentDto>.Ok(Map(student), message);
    }

    private async Task<Models.Student?> LoadStudentAsync(int studentId, CancellationToken cancellationToken)
        => await _db.Students
            .Include(s => s.Applicant)!.ThenInclude(a => a!.Course)
            .Include(s => s.Batch)
            .FirstOrDefaultAsync(s => s.StudentId == studentId, cancellationToken);

    private static string? NormalizeStatus(string status) => status.ToLowerInvariant() switch
    {
        "active" or "activated" => SystemConstants.AccountStatuses.Active,
        "inactive" or "deactivated" or "pending" => SystemConstants.AccountStatuses.Inactive,
        _ => null
    };

    private static StudentDto Map(Models.Student s) => new()
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
