using ABEC_System.DTOs;

namespace ABEC_System.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResultDto>> AdminLoginAsync(AdminLoginDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<AuthResultDto>> StudentLoginAsync(StudentLoginDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<AuthResultDto>> RegisterStudentAsync(StudentRegisterDto dto, CancellationToken cancellationToken = default);
}

public interface IApplicantService
{
    Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<ApplicantDto>>> GetApplicantsAsync(string? status = null, int? courseId = null, string? search = null, int? take = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<ApplicantDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<ApplicantDto>> EnrollAsync(EnrollApplicantDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<ApplicantDto>> UpdateStatusAsync(int id, string status, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> BulkUpdateStatusAsync(BulkStatusDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<StudentDto>> AddToSlotAsync(int applicantId, CancellationToken cancellationToken = default);
}

public interface IBatchService
{
    Task<ApiResponse<IReadOnlyList<BatchDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<BatchDto?>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<BatchDto>>> GetHistoryAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<BatchDto>> CreateAsync(CreateBatchDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<BatchDto>> ActivateAsync(int batchId, CancellationToken cancellationToken = default);
    Task<ApiResponse<BatchDto>> CompleteAsync(int batchId, CancellationToken cancellationToken = default);
}

public interface ICourseService
{
    Task<ApiResponse<IReadOnlyList<CourseDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CourseDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<StudentDto>>> GetEnrolledStudentsAsync(int courseId, string? search = null, CancellationToken cancellationToken = default);
}

public interface IActivationService
{
    Task<ApiResponse<ActivationSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<StudentDto>>> GetAccountsAsync(string? status = null, int? courseId = null, string? search = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<StudentDto>> GeneratePinAsync(int studentId, CancellationToken cancellationToken = default);
    Task<ApiResponse<StudentDto>> ActivateAsync(int studentId, CancellationToken cancellationToken = default);
    Task<ApiResponse<StudentDto>> DeactivateAsync(int studentId, CancellationToken cancellationToken = default);
    Task<ApiResponse<StudentDto>> ReactivateAsync(int studentId, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> BulkUpdateStatusAsync(BulkStatusDto dto, CancellationToken cancellationToken = default);
}

public interface IScheduleService
{
    Task<ApiResponse<IReadOnlyList<CourseScheduleDto>>> GetAllAsync(int? courseId = null, int? batchId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<CourseScheduleDto>> CreateAsync(CreateScheduleDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CourseScheduleDto>> UpdateAsync(int id, UpdateScheduleDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> HasConflictAsync(CreateScheduleDto dto, int? excludeScheduleId = null, CancellationToken cancellationToken = default);
}

public interface IDocumentRequestService
{
    Task<ApiResponse<IReadOnlyList<DocumentRequestDto>>> GetAllAsync(string? status = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<DocumentRequestDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<DocumentRequestDto>>> GetByStudentAsync(int studentId, CancellationToken cancellationToken = default);
    Task<ApiResponse<DocumentRequestDto>> CreateAsync(int studentId, CreateDocumentRequestDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<DocumentRequestDto>> UpdateStatusAsync(int id, string status, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> BulkUpdateStatusAsync(BulkStatusDto dto, CancellationToken cancellationToken = default);
}

public interface INotificationService
{
    Task<ApiResponse<IReadOnlyList<NotificationDto>>> GetForStudentAsync(int studentId, string? status = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> MarkReadAsync(int notificationId, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> MarkAllReadAsync(int studentId, CancellationToken cancellationToken = default);
    Task CreateAsync(string title, string message, int? studentId = null, int? applicantId = null, CancellationToken cancellationToken = default);
}

public interface IArchiveService
{
    Task<ApiResponse<IReadOnlyList<ArchiveDto>>> GetAllAsync(string? reason = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<ArchiveDto>> CreateAsync(CreateArchiveDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> ArchiveStudentFromCourseAsync(int studentId, string reason, CancellationToken cancellationToken = default);
}

public interface IAdminProfileService
{
    Task<ApiResponse<AdminProfileDto>> GetProfileAsync(int adminId, CancellationToken cancellationToken = default);
    Task<ApiResponse<AdminProfileDto>> UpdateProfileAsync(int adminId, UpdateAdminProfileDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> ChangePasswordAsync(int adminId, ChangePasswordDto dto, CancellationToken cancellationToken = default);
}

public interface IStudentPortalService
{
    Task<ApiResponse<StudentDashboardDto>> GetDashboardAsync(int studentId, CancellationToken cancellationToken = default);
    Task<ApiResponse<StudentDto>> GetProfileAsync(int studentId, CancellationToken cancellationToken = default);
    Task<ApiResponse<StudentDto>> UpdateProfileAsync(int studentId, UpdateStudentProfileDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<CourseScheduleDto>>> GetScheduleAsync(int studentId, CancellationToken cancellationToken = default);
}
