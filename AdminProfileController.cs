using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ABEC_System.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminProfileController : ControllerBase
{
    private readonly IAdminProfileService _profileService;
    private readonly IApplicantService _applicantService;

    public AdminProfileController(IAdminProfileService profileService, IApplicantService applicantService)
    {
        _profileService = profileService;
        _applicantService = applicantService;
    }

    [HttpGet("dashboard/stats")]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> DashboardStats(CancellationToken cancellationToken)
        => Ok(await _applicantService.GetDashboardStatsAsync(cancellationToken));

    [HttpGet("profile/{adminId:int}")]
    public async Task<ActionResult<ApiResponse<AdminProfileDto>>> GetProfile(int adminId, CancellationToken cancellationToken)
        => Ok(await _profileService.GetProfileAsync(adminId, cancellationToken));

    [HttpPut("profile/{adminId:int}")]
    public async Task<ActionResult<ApiResponse<AdminProfileDto>>> UpdateProfile(int adminId, [FromBody] UpdateAdminProfileDto dto, CancellationToken cancellationToken)
        => Ok(await _profileService.UpdateProfileAsync(adminId, dto, cancellationToken));

    [HttpPost("profile/{adminId:int}/change-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(int adminId, [FromBody] ChangePasswordDto dto, CancellationToken cancellationToken)
        => Ok(await _profileService.ChangePasswordAsync(adminId, dto, cancellationToken));
}
