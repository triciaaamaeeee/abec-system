using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ABEC_System.Controllers;

[ApiController]
[Route("api/schedules")]
public class ScheduleController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public ScheduleController(IScheduleService scheduleService) => _scheduleService = scheduleService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CourseScheduleDto>>>> GetAll(
        [FromQuery] int? courseId,
        [FromQuery] int? batchId,
        CancellationToken cancellationToken)
        => Ok(await _scheduleService.GetAllAsync(courseId, batchId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CourseScheduleDto>>> Create([FromBody] CreateScheduleDto dto, CancellationToken cancellationToken)
        => Ok(await _scheduleService.CreateAsync(dto, cancellationToken));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<CourseScheduleDto>>> Update(int id, [FromBody] UpdateScheduleDto dto, CancellationToken cancellationToken)
        => Ok(await _scheduleService.UpdateAsync(id, dto, cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
        => Ok(await _scheduleService.DeleteAsync(id, cancellationToken));

    [HttpPost("conflicts")]
    public async Task<ActionResult<ApiResponse<bool>>> Conflicts([FromBody] CreateScheduleDto dto, CancellationToken cancellationToken)
        => Ok(await _scheduleService.HasConflictAsync(dto, null, cancellationToken));
}
