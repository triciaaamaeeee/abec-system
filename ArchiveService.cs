using ABEC_System.Data;
using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using ABEC_System.Models;
using Microsoft.EntityFrameworkCore;

namespace ABEC_System.Services;

public class ArchiveService : IArchiveService
{
    private readonly ApplicationDbContext _db;

    public ArchiveService(ApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<IReadOnlyList<ArchiveDto>>> GetAllAsync(string? reason = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Archives.AsNoTracking()
            .Include(a => a.Student)!.ThenInclude(s => s!.Applicant)!.ThenInclude(ap => ap!.Course)
            .Include(a => a.Student)!.ThenInclude(s => s!.Batch)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(reason) && !string.Equals(reason, "all", StringComparison.OrdinalIgnoreCase))
            query = query.Where(a => a.ArchiveReason.ToLower().Contains(reason.ToLower()));

        var list = await query.OrderByDescending(a => a.ArchiveDate).ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<ArchiveDto>>.Ok(list.Select(Map).ToList());
    }

    public async Task<ApiResponse<ArchiveDto>> CreateAsync(CreateArchiveDto dto, CancellationToken cancellationToken = default)
        => await ArchiveInternalAsync(dto.StudentId, dto.ArchiveReason, cancellationToken);

    public async Task<ApiResponse<object>> ArchiveStudentFromCourseAsync(int studentId, string reason, CancellationToken cancellationToken = default)
    {
        var result = await ArchiveInternalAsync(studentId, reason, cancellationToken);
        return result.Success
            ? ApiResponse<object>.Ok(result.Data!, result.Message)
            : ApiResponse<object>.Fail(result.Message);
    }

    private async Task<ApiResponse<ArchiveDto>> ArchiveInternalAsync(int studentId, string reason, CancellationToken cancellationToken)
    {
        var student = await _db.Students
            .Include(s => s.Applicant)!.ThenInclude(a => a!.Course)
            .Include(s => s.Batch)
            .FirstOrDefaultAsync(s => s.StudentId == studentId, cancellationToken);

        if (student is null)
            return ApiResponse<ArchiveDto>.Fail("Student not found.");

        var archive = new Archive
        {
            StudentId = studentId,
            ArchiveReason = reason.Trim(),
            ArchiveDate = DateTime.Now
        };

        _db.Archives.Add(archive);
        student.AccountStatus = Constants.SystemConstants.AccountStatuses.Inactive;
        await _db.SaveChangesAsync(cancellationToken);

        archive.Student = student;
        return ApiResponse<ArchiveDto>.Ok(Map(archive), "Student archived successfully.");
    }

    private static ArchiveDto Map(Archive a) => new()
    {
        ArchiveId = a.ArchiveId,
        StudentId = a.StudentId,
        StudentName = a.Student?.Applicant?.FullName ?? string.Empty,
        Email = a.Student?.Applicant?.Email ?? string.Empty,
        CourseName = a.Student?.Applicant?.Course?.CourseName ?? string.Empty,
        BatchName = a.Student?.Batch?.BatchName ?? string.Empty,
        ArchiveReason = a.ArchiveReason,
        ArchiveDate = a.ArchiveDate
    };
}
