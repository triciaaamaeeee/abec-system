using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ABEC_System.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService) => _notificationService = notificationService;

    [HttpGet("student/{studentId:int}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationDto>>>> GetForStudent(
        int studentId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
        => Ok(await _notificationService.GetForStudentAsync(studentId, status, cancellationToken));

    [HttpPut("{id:int}/read")]
    public async Task<ActionResult<ApiResponse<object>>> MarkRead(int id, CancellationToken cancellationToken)
        => Ok(await _notificationService.MarkReadAsync(id, cancellationToken));

    [HttpPost("student/{studentId:int}/mark-all-read")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAllRead(int studentId, CancellationToken cancellationToken)
        => Ok(await _notificationService.MarkAllReadAsync(studentId, cancellationToken));
}
