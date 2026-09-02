using System.Diagnostics;
using ABEC_System.Models;
using Microsoft.AspNetCore.Mvc;

namespace ABEC_System.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment _environment;

    public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    private IActionResult RenderHtmlPage(string fileName)
    {
        var path = Path.Combine(_environment.ContentRootPath, "Views", "Home", fileName);
        return File(path, "text/html");
    }

    public IActionResult Index() => RedirectToAction(nameof(Landing));

    public IActionResult Landing() => RenderHtmlPage("LandingPage.html");

    public IActionResult Login() => RenderHtmlPage("LoginPage.html");

    public IActionResult SignOutPage() => RenderHtmlPage("SignOutPage.html");

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
