using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ABEC_System.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("admin-login")]
    public async Task<ActionResult<ApiResponse<AuthResultDto>>> AdminLogin([FromBody] AdminLoginDto dto, CancellationToken cancellationToken)
        => Ok(await _authService.AdminLoginAsync(dto, cancellationToken));

    [HttpPost("student-login")]
    public async Task<ActionResult<ApiResponse<AuthResultDto>>> StudentLogin([FromBody] StudentLoginDto dto, CancellationToken cancellationToken)
        => Ok(await _authService.StudentLoginAsync(dto, cancellationToken));

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResultDto>>> Register([FromBody] StudentRegisterDto dto, CancellationToken cancellationToken)
        => Ok(await _authService.RegisterStudentAsync(dto, cancellationToken));
}
