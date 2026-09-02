using Microsoft.AspNetCore.Mvc;

namespace ABEC_System.Controllers;

public class StudentController : Controller
{
    public IActionResult Dashboard() => View();
    public IActionResult ClassSchedule() => View();
    public IActionResult DocumentRequest() => View();
    public IActionResult Notifications() => View();
    public IActionResult UserSettings() => View();
}
