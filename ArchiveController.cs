using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ABEC_System.Controllers;

[ApiController]
[Route("api/archives")]
public class ArchiveController : ControllerBase
{
    private readonly IArchiveService _archiveService;

    public ArchiveController(IArchiveService archiveService) => _archiveService = archiveService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ArchiveDto>>>> GetAll([FromQuery] string? reason, CancellationToken cancellationToken)
        => Ok(await _archiveService.GetAllAsync(reason, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ArchiveDto>>> Create([FromBody] CreateArchiveDto dto, CancellationToken cancellationToken)
        => Ok(await _archiveService.CreateAsync(dto, cancellationToken));

    [HttpPost("students/{studentId:int}")]
    public async Task<ActionResult<ApiResponse<object>>> ArchiveStudent(int studentId, [FromBody] CreateArchiveDto dto, CancellationToken cancellationToken)
        => Ok(await _archiveService.ArchiveStudentFromCourseAsync(studentId, dto.ArchiveReason, cancellationToken));
}
