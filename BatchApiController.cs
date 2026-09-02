using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ABEC_System.Controllers;

[ApiController]
[Route("api/batches")]
public class BatchApiController : ControllerBase
{
    private readonly IBatchService _batchService;

    public BatchApiController(IBatchService batchService) => _batchService = batchService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BatchDto>>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _batchService.GetAllAsync(cancellationToken));

    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<BatchDto?>>> GetActive(CancellationToken cancellationToken)
        => Ok(await _batchService.GetActiveAsync(cancellationToken));

    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BatchDto>>>> GetHistory(CancellationToken cancellationToken)
        => Ok(await _batchService.GetHistoryAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BatchDto>>> Create([FromBody] CreateBatchDto dto, CancellationToken cancellationToken)
        => Ok(await _batchService.CreateAsync(dto, cancellationToken));

    [HttpPut("{id:int}/activate")]
    public async Task<ActionResult<ApiResponse<BatchDto>>> Activate(int id, CancellationToken cancellationToken)
        => Ok(await _batchService.ActivateAsync(id, cancellationToken));

    [HttpPost("{id:int}/complete")]
    public async Task<ActionResult<ApiResponse<BatchDto>>> Complete(int id, CancellationToken cancellationToken)
        => Ok(await _batchService.CompleteAsync(id, cancellationToken));
}
