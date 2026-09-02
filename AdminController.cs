using ABEC_System.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ABEC_System.Controllers;

public class AdminController : Controller
{
    private readonly IApplicantService _applicantService;
    private readonly IBatchService _batchService;

    public AdminController(IApplicantService applicantService, IBatchService batchService)
    {
        _applicantService = applicantService;
        _batchService = batchService;
    }

    public IActionResult Dashboard() => View();

    public IActionResult Applicants() => View();

    public IActionResult AccountActivation() => View();

    public IActionResult DocumentManagement() => View();

    public IActionResult CourseManagement() => View();

    public IActionResult StudentArchive() => View();

    public IActionResult UserSettings() => View();

    [HttpGet]
    public async Task<IActionResult> DashboardData(CancellationToken cancellationToken)
    {
        var stats = await _applicantService.GetDashboardStatsAsync(cancellationToken);
        var activeBatch = await _batchService.GetActiveAsync(cancellationToken);
        var recentApplicants = await _applicantService.GetApplicantsAsync(take: 10, cancellationToken: cancellationToken);
        return Json(new
        {
            stats = stats.Data,
            activeBatch = activeBatch.Data,
            recentApplicants = recentApplicants.Data
        });
    }
}
