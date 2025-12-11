using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WordMaster.API.Extensions;
using WordMaster.Application.Attributes;
using WordMaster.Application.Responses.Statistics;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers;

/// <summary>
/// Kullanıcı istatistikleri ve analizleri ile ilgili işlemleri yöneten kontrolcü.
/// </summary>
[Route("api/statistics")]
[EnableRateLimiting("GeneralPolicy")]
public class StatisticsController(IStatisticsService statisticsService) : BaseController
{
    /// <summary>
    /// Kullanıcının dashboard istatistiklerini getirir.
    /// </summary>
    /// <returns>Toplam kelime, başarı oranı, en iyiler/kötüler ve aktivite grafiği.</returns>
    [HttpGet("dashboard")]
    [RequirePermission("Public", "Statistics", "GetDashboardStatistics", "GET", "Dashboard istatistiklerini getir")]
    public async Task<IActionResult> GetDashboardStatistics()
    {
        Guid userId = User.GetUserId();
        ServiceResult<DashboardStatisticsResponse> result = await statisticsService.GetDashboardStatisticsAsync(userId);
        return CreateResult(result);
    }
}
