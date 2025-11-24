using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Responses.Admin;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers.Admin;

/// <summary>
/// Admin paneli dashboard işlemlerini yöneten controller.
/// </summary>
[Authorize(Roles = "admin")]
[Route("api/admin/dashboard")]
public class DashboardController(IAdminDashboardService adminDashboardService) : BaseController
{
    /// <summary>
    /// Admin dashboard istatistiklerini getirir.
    /// </summary>
    /// <returns>Dashboard verileri (kullanıcı sayıları, kelime sayıları vb.).</returns>
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        ServiceResult<AdminDashboardResponse> result = await adminDashboardService.GetDashboardAsync();
        return CreateResult(result);
    }
}

