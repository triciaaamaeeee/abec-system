using System.ComponentModel.DataAnnotations;

namespace ABEC_System.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message)
        => new() { Success = false, Message = message };
}

public class AdminLoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool RememberDevice { get; set; }
}

public class StudentLoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string SecurityPin { get; set; } = string.Empty;

    public bool RememberDevice { get; set; }
}

public class StudentRegisterDto
{
    [Required]
    public string Surname { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    public string? MiddleInitial { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string SecurityPin { get; set; } = string.Empty;
}

public class AuthResultDto
{
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string EmailOrUsername { get; set; } = string.Empty;
}

public class EnrollApplicantDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required]
    public int CourseId { get; set; }

    [Required]
    public string ContactNumber { get; set; } = string.Empty;

    public string Gender { get; set; } = "Not Specified";

    public DateTime? Birthdate { get; set; }
}

public class UpdateApplicantStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}

public class BulkStatusDto
{
    [Required]
    public List<int> Ids { get; set; } = new();

    [Required]
    public string Status { get; set; } = string.Empty;
}

public class CreateBatchDto
{
    [Required]
    public string BatchName { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}

public class CreateScheduleDto
{
    [Required]
    public int CourseId { get; set; }

    [Required]
    public int BatchId { get; set; }

    [Required]
    public DateTime ScheduleDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    public string Room { get; set; } = string.Empty;
}

public class UpdateScheduleDto
{
    public DateTime? ScheduleDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string? Room { get; set; }
    public int? BatchId { get; set; }
}

public class CreateDocumentRequestDto
{
    [Required]
    public string DocumentType { get; set; } = string.Empty;

    [Required]
    public string Reason { get; set; } = string.Empty;
}

public class UpdateDocumentStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}

public class UpdateAdminProfileDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string Username { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}

public class UpdateStudentProfileDto
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }

    [Required]
    public string LastName { get; set; } = string.Empty;

    public string? Gender { get; set; }

    public DateTime? Birthdate { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public string? ContactNumber { get; set; }

    public string? Address { get; set; }
}

public class CreateArchiveDto
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public string ArchiveReason { get; set; } = string.Empty;
}
