namespace ABEC_System.DTOs;

public class DashboardStatsDto
{
    public int TotalApplicants { get; set; }
    public int Pending { get; set; }
    public int Waitlisted { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
}

public class ApplicantDto
{
    public int ApplicantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime Birthdate { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string ApplicationStatus { get; set; } = string.Empty;
    public DateTime ApplicationDate { get; set; }
    public bool HasStudentAccount { get; set; }
}

public class BatchDto
{
    public int BatchId { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public string AcademicYear => $"{StartDate:yyyy}–{EndDate:yyyy}";
}

public class CourseDto
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string? CourseDescription { get; set; }
    public string Duration { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int FilledSlots { get; set; }
    public int RemainingSlots => Math.Max(0, Capacity - FilledSlots);
}

public class StudentDto
{
    public int StudentId { get; set; }
    public int ApplicantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public int BatchId { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public string SecurityPin { get; set; } = string.Empty;
    public string AccountStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ActivationSummaryDto
{
    public int TotalApprovedStudents { get; set; }
    public int PendingActivation { get; set; }
    public int ActivatedAccounts { get; set; }
    public int DeactivatedAccounts { get; set; }
}

public class CourseScheduleDto
{
    public int ScheduleId { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int BatchId { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public DateTime ScheduleDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Room { get; set; } = string.Empty;
    public string DayOfWeek => ScheduleDate.DayOfWeek.ToString();
}

public class DocumentRequestDto
{
    public int RequestId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string RequestStatus { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public DateTime? ReleaseDate { get; set; }
}

public class NotificationDto
{
    public int NotificationId { get; set; }
    public int? StudentId { get; set; }
    public int? ApplicantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ArchiveDto
{
    public int ArchiveId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string BatchName { get; set; } = string.Empty;
    public string ArchiveReason { get; set; } = string.Empty;
    public DateTime ArchiveDate { get; set; }
}

public class AdminProfileDto
{
    public int AdminId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class StudentDashboardDto
{
    public StudentDto Profile { get; set; } = new();
    public IReadOnlyList<CourseScheduleDto> UpcomingClasses { get; set; } = Array.Empty<CourseScheduleDto>();
    public IReadOnlyList<DocumentRequestDto> RecentDocumentRequests { get; set; } = Array.Empty<DocumentRequestDto>();
    public IReadOnlyList<NotificationDto> RecentNotifications { get; set; } = Array.Empty<NotificationDto>();
}
