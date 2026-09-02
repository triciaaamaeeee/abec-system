using ABEC_System.Constants;
using ABEC_System.Data;
using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using ABEC_System.Models;
using Microsoft.EntityFrameworkCore;

namespace ABEC_System.Services;

public class DocumentRequestService : IDocumentRequestService
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationService _notifications;

    public DocumentRequestService(ApplicationDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<ApiResponse<IReadOnlyList<DocumentRequestDto>>> GetAllAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        var query = BaseQuery();
        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
            query = query.Where(r => r.RequestStatus == NormalizeStatus(status));

        var list = await query.OrderByDescending(r => r.RequestDate).ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<DocumentRequestDto>>.Ok(list.Select(Map).ToList());
    }

    public async Task<ApiResponse<DocumentRequestDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await BaseQuery().FirstOrDefaultAsync(r => r.RequestId == id, cancellationToken);
        return entity is null
            ? ApiResponse<DocumentRequestDto>.Fail("Document request not found.")
            : ApiResponse<DocumentRequestDto>.Ok(Map(entity));
    }

    public async Task<ApiResponse<IReadOnlyList<DocumentRequestDto>>> GetByStudentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var list = await BaseQuery()
            .Where(r => r.StudentId == studentId)
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<DocumentRequestDto>>.Ok(list.Select(Map).ToList());
    }

    public async Task<ApiResponse<DocumentRequestDto>> CreateAsync(int studentId, CreateDocumentRequestDto dto, CancellationToken cancellationToken = default)
    {
        var studentExists = await _db.Students.AnyAsync(s => s.StudentId == studentId, cancellationToken);
        if (!studentExists)
            return ApiResponse<DocumentRequestDto>.Fail("Student not found.");

        var entity = new DocumentRequest
        {
            StudentId = studentId,
            DocumentType = dto.DocumentType.Trim(),
            Reason = dto.Reason.Trim(),
            RequestStatus = SystemConstants.DocumentStatuses.Pending,
            RequestDate = DateTime.Now
        };

        _db.DocumentRequests.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await _notifications.CreateAsync(
            "Document Request Submitted",
            $"Your request for {entity.DocumentType} has been submitted.",
            studentId: studentId,
            cancellationToken: cancellationToken);

        var loaded = await BaseQuery().FirstAsync(r => r.RequestId == entity.RequestId, cancellationToken);
        return ApiResponse<DocumentRequestDto>.Ok(Map(loaded), "Document request created.");
    }

    public async Task<ApiResponse<DocumentRequestDto>> UpdateStatusAsync(int id, string status, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeStatus(status);
        if (normalized is null)
            return ApiResponse<DocumentRequestDto>.Fail("Invalid document request status.");

        var entity = await _db.DocumentRequests
            .Include(r => r.Student)!.ThenInclude(s => s!.Applicant)!.ThenInclude(a => a!.Course)
            .FirstOrDefaultAsync(r => r.RequestId == id, cancellationToken);
        if (entity is null)
            return ApiResponse<DocumentRequestDto>.Fail("Document request not found.");

        entity.RequestStatus = normalized;
        if (normalized == SystemConstants.DocumentStatuses.Released)
            entity.ReleaseDate = DateTime.Now;

        await _db.SaveChangesAsync(cancellationToken);
        await _notifications.CreateAsync(
            "Document Request Updated",
            $"Your {entity.DocumentType} request is now: {normalized}.",
            studentId: entity.StudentId,
            cancellationToken: cancellationToken);

        return ApiResponse<DocumentRequestDto>.Ok(Map(entity), "Document request status updated.");
    }

    public async Task<ApiResponse<object>> BulkUpdateStatusAsync(BulkStatusDto dto, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeStatus(dto.Status);
        if (normalized is null)
            return ApiResponse<object>.Fail("Invalid document request status.");

        var requests = await _db.DocumentRequests.Where(r => dto.Ids.Contains(r.RequestId)).ToListAsync(cancellationToken);
        foreach (var request in requests)
        {
            request.RequestStatus = normalized;
            if (normalized == SystemConstants.DocumentStatuses.Released)
                request.ReleaseDate = DateTime.Now;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { updated = requests.Count }, "Bulk document status update completed.");
    }

    private IQueryable<DocumentRequest> BaseQuery()
        => _db.DocumentRequests.AsNoTracking()
            .Include(r => r.Student)!.ThenInclude(s => s!.Applicant)!.ThenInclude(a => a!.Course);

    private static string? NormalizeStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "pending" => SystemConstants.DocumentStatuses.Pending,
        "approved" or "processing" or "ready" => SystemConstants.DocumentStatuses.Approved,
        "rejected" => SystemConstants.DocumentStatuses.Rejected,
        "released" or "archived" => SystemConstants.DocumentStatuses.Released,
        _ when status is SystemConstants.DocumentStatuses.Pending
            or SystemConstants.DocumentStatuses.Approved
            or SystemConstants.DocumentStatuses.Rejected
            or SystemConstants.DocumentStatuses.Released => status,
        _ => null
    };

    private static DocumentRequestDto Map(DocumentRequest r) => new()
    {
        RequestId = r.RequestId,
        StudentId = r.StudentId,
        StudentName = r.Student?.Applicant?.FullName ?? string.Empty,
        Email = r.Student?.Applicant?.Email ?? string.Empty,
        ContactNumber = r.Student?.Applicant?.ContactNumber ?? string.Empty,
        CourseName = r.Student?.Applicant?.Course?.CourseName ?? string.Empty,
        DocumentType = r.DocumentType,
        Reason = r.Reason,
        RequestStatus = r.RequestStatus,
        RequestDate = r.RequestDate,
        ReleaseDate = r.ReleaseDate
    };
}
