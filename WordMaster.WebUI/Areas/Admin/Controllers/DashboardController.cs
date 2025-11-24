using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.Admin;

namespace WordMaster.WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin")]
[Route("[area]/[controller]")]
public class DashboardController(IAdminDashboardService adminDashboardService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        AdminDashboardViewModel model = await adminDashboardService.GetDashboardAsync();
        return View(model);
    }
}

