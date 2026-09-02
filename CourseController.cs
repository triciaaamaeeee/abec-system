using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ABEC_System.Controllers;

[ApiController]
[Route("api/courses")]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CourseController(ICourseService courseService) => _courseService = courseService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CourseDto>>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _courseService.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CourseDto>>> GetById(int id, CancellationToken cancellationToken)
        => Ok(await _courseService.GetByIdAsync(id, cancellationToken));

    [HttpGet("{id:int}/students")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentDto>>>> GetStudents(int id, [FromQuery] string? search, CancellationToken cancellationToken)
        => Ok(await _courseService.GetEnrolledStudentsAsync(id, search, cancellationToken));
}
