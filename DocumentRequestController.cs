using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ABEC_System.Controllers;

[ApiController]
[Route("api/document-requests")]
public class DocumentRequestController : ControllerBase
{
    private readonly IDocumentRequestService _documentService;

    public DocumentRequestController(IDocumentRequestService documentService) => _documentService = documentService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DocumentRequestDto>>>> GetAll([FromQuery] string? status, CancellationToken cancellationToken)
        => Ok(await _documentService.GetAllAsync(status, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<DocumentRequestDto>>> GetById(int id, CancellationToken cancellationToken)
        => Ok(await _documentService.GetByIdAsync(id, cancellationToken));

    [HttpGet("student/{studentId:int}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DocumentRequestDto>>>> GetByStudent(int studentId, CancellationToken cancellationToken)
        => Ok(await _documentService.GetByStudentAsync(studentId, cancellationToken));

    [HttpPost("student/{studentId:int}")]
    public async Task<ActionResult<ApiResponse<DocumentRequestDto>>> Create(int studentId, [FromBody] CreateDocumentRequestDto dto, CancellationToken cancellationToken)
        => Ok(await _documentService.CreateAsync(studentId, dto, cancellationToken));

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<ApiResponse<DocumentRequestDto>>> UpdateStatus(int id, [FromBody] UpdateDocumentStatusDto dto, CancellationToken cancellationToken)
        => Ok(await _documentService.UpdateStatusAsync(id, dto.Status, cancellationToken));

    [HttpPost("bulk-status")]
    public async Task<ActionResult<ApiResponse<object>>> BulkStatus([FromBody] BulkStatusDto dto, CancellationToken cancellationToken)
        => Ok(await _documentService.BulkUpdateStatusAsync(dto, cancellationToken));
}
