using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Responses.Admin;
using WordMaster.Application.Services.Abstract;
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
        ServiceResult<AdminDashboardResponse> result = await adminDashboardService.GetDashboardAsync();
        return !result.IsSuccess || result.Data == null ? View(new AdminDashboardResponse()) : View(result.Data);
    }
}
