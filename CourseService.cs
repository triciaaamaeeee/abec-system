using ABEC_System.Constants;
using ABEC_System.Data;
using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ABEC_System.Services;

public class CourseService : ICourseService
{
    private readonly ApplicationDbContext _db;

    public CourseService(ApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<IReadOnlyList<CourseDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var courses = await _db.Courses.AsNoTracking().OrderBy(c => c.CourseName).ToListAsync(cancellationToken);
        var result = new List<CourseDto>();
        foreach (var course in courses)
            result.Add(await MapAsync(course.CourseId, course.CourseName, course.CourseDescription, course.Duration, course.Status, cancellationToken));
        return ApiResponse<IReadOnlyList<CourseDto>>.Ok(result);
    }

    public async Task<ApiResponse<CourseDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var course = await _db.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.CourseId == id, cancellationToken);
        if (course is null)
            return ApiResponse<CourseDto>.Fail("Course not found.");

        return ApiResponse<CourseDto>.Ok(await MapAsync(course.CourseId, course.CourseName, course.CourseDescription, course.Duration, course.Status, cancellationToken));
    }

    public async Task<ApiResponse<IReadOnlyList<StudentDto>>> GetEnrolledStudentsAsync(int courseId, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Students.AsNoTracking()
            .Include(s => s.Applicant)!.ThenInclude(a => a!.Course)
            .Include(s => s.Batch)
            .Where(s => s.Applicant != null && s.Applicant.CourseId == courseId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s =>
                s.Applicant!.FirstName.ToLower().Contains(term) ||
                s.Applicant.LastName.ToLower().Contains(term) ||
                s.Applicant.Email.ToLower().Contains(term));
        }

        var students = await query.OrderBy(s => s.Applicant!.LastName).ToListAsync(cancellationToken);
        var list = students.Select(s => new StudentDto
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
        }).ToList();

        return ApiResponse<IReadOnlyList<StudentDto>>.Ok(list);
    }

    private async Task<CourseDto> MapAsync(int id, string name, string? description, string duration, string status, CancellationToken cancellationToken)
    {
        var filled = await _db.Students.Include(s => s.Applicant)
            .CountAsync(s => s.Applicant != null && s.Applicant.CourseId == id, cancellationToken);

        return new CourseDto
        {
            CourseId = id,
            CourseName = name,
            CourseDescription = description,
            Duration = duration,
            Status = status,
            Capacity = SystemConstants.DefaultCourseCapacity,
            FilledSlots = filled
        };
    }
}
