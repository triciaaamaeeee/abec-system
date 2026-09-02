using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ABEC_System.Controllers;

[ApiController]
[Route("api/student")]
public class StudentApiController : ControllerBase
{
    private readonly IStudentPortalService _studentPortal;

    public StudentApiController(IStudentPortalService studentPortal) => _studentPortal = studentPortal;

    [HttpGet("{studentId:int}/dashboard")]
    public async Task<ActionResult<ApiResponse<StudentDashboardDto>>> Dashboard(int studentId, CancellationToken cancellationToken)
        => Ok(await _studentPortal.GetDashboardAsync(studentId, cancellationToken));

    [HttpGet("{studentId:int}/profile")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> Profile(int studentId, CancellationToken cancellationToken)
        => Ok(await _studentPortal.GetProfileAsync(studentId, cancellationToken));

    [HttpPut("{studentId:int}/profile")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> UpdateProfile(int studentId, [FromBody] UpdateStudentProfileDto dto, CancellationToken cancellationToken)
        => Ok(await _studentPortal.UpdateProfileAsync(studentId, dto, cancellationToken));

    [HttpGet("{studentId:int}/schedule")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CourseScheduleDto>>>> Schedule(int studentId, CancellationToken cancellationToken)
        => Ok(await _studentPortal.GetScheduleAsync(studentId, cancellationToken));
}
