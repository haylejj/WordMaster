using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Services.Abstract;

namespace WordMaster.WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin")]
[Route("[area]/[controller]")]
public class DashboardController(IAdminDashboardService adminDashboardService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var model = await adminDashboardService.GetDashboardAsync();
        return View(model);
    }
}

