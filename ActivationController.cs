using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ABEC_System.Controllers;

[ApiController]
[Route("api/activations")]
public class ActivationController : ControllerBase
{
    private readonly IActivationService _activationService;

    public ActivationController(IActivationService activationService) => _activationService = activationService;

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<ActivationSummaryDto>>> Summary(CancellationToken cancellationToken)
        => Ok(await _activationService.GetSummaryAsync(cancellationToken));

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentDto>>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] int? courseId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
        => Ok(await _activationService.GetAccountsAsync(status, courseId, search, cancellationToken));

    [HttpPost("{id:int}/generate-pin")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> GeneratePin(int id, CancellationToken cancellationToken)
        => Ok(await _activationService.GeneratePinAsync(id, cancellationToken));

    [HttpPost("{id:int}/activate")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> Activate(int id, CancellationToken cancellationToken)
        => Ok(await _activationService.ActivateAsync(id, cancellationToken));

    [HttpPost("{id:int}/deactivate")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> Deactivate(int id, CancellationToken cancellationToken)
        => Ok(await _activationService.DeactivateAsync(id, cancellationToken));

    [HttpPost("{id:int}/reactivate")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> Reactivate(int id, CancellationToken cancellationToken)
        => Ok(await _activationService.ReactivateAsync(id, cancellationToken));

    [HttpPost("bulk-status")]
    public async Task<ActionResult<ApiResponse<object>>> BulkStatus([FromBody] BulkStatusDto dto, CancellationToken cancellationToken)
        => Ok(await _activationService.BulkUpdateStatusAsync(dto, cancellationToken));
}
