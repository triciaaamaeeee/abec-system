using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ABEC_System.Controllers;

[ApiController]
[Route("api/applicants")]
public class ApplicantController : ControllerBase
{
    private readonly IApplicantService _applicantService;

    public ApplicantController(IApplicantService applicantService) => _applicantService = applicantService;

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> Stats(CancellationToken cancellationToken)
        => Ok(await _applicantService.GetDashboardStatsAsync(cancellationToken));

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ApplicantDto>>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] int? courseId,
        [FromQuery] string? search,
        [FromQuery] int? take,
        CancellationToken cancellationToken)
        => Ok(await _applicantService.GetApplicantsAsync(status, courseId, search, take, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ApplicantDto>>> GetById(int id, CancellationToken cancellationToken)
        => Ok(await _applicantService.GetByIdAsync(id, cancellationToken));

    [HttpPost("enroll")]
    public async Task<ActionResult<ApiResponse<ApplicantDto>>> Enroll([FromBody] EnrollApplicantDto dto, CancellationToken cancellationToken)
        => Ok(await _applicantService.EnrollAsync(dto, cancellationToken));

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<ApiResponse<ApplicantDto>>> UpdateStatus(int id, [FromBody] UpdateApplicantStatusDto dto, CancellationToken cancellationToken)
        => Ok(await _applicantService.UpdateStatusAsync(id, dto.Status, cancellationToken));

    [HttpPost("bulk-status")]
    public async Task<ActionResult<ApiResponse<object>>> BulkStatus([FromBody] BulkStatusDto dto, CancellationToken cancellationToken)
        => Ok(await _applicantService.BulkUpdateStatusAsync(dto, cancellationToken));

    [HttpPost("{id:int}/add-to-slot")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> AddToSlot(int id, CancellationToken cancellationToken)
        => Ok(await _applicantService.AddToSlotAsync(id, cancellationToken));
}
