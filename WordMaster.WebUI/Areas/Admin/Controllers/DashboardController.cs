using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.Admin;
using WordMaster.Domain.Results;

namespace WordMaster.WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin")]
[Route("[area]/[controller]")]
public class DashboardController(IAdminDashboardService adminDashboardService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ServiceResult<AdminDashboardViewModel> result = await adminDashboardService.GetDashboardAsync();
        if (!result.IsSuccess || result.Data == null)
        {
            // Handle error, maybe redirect or show empty view
            return View(new AdminDashboardViewModel());
        }
        return View(result.Data);
    }
}
