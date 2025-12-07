using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Constants;
using WordMaster.Application.Requests.LogHistory;
using WordMaster.Application.Responses.LogHistory;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers.Admin;

/// <summary>
/// Sistem log geçmişi işlemlerini yöneten API kontrolcüsü.
/// </summary>
/// <param name="logHistoryService">Log geçmişi işlemlerini yürüten servis.</param>
[Route("api/admin/logHistory")]
[ApiController]
[Authorize(Roles = ApplicationRoles.Admin)]
public class LogHistoryController(ILogHistoryService logHistoryService) : BaseController
{
    /// <summary>
    /// Log geçmişini sayfalı ve filtrelenmiş şekilde listeler.
    /// </summary>
    /// <param name="request">Sayfalama ve filtreleme parametreleri</param>
    /// <returns>Log kayıtları listesi</returns>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetLogHistoryRequest request)
    {
        ServiceResult<PagedResult<LogHistoryResponse>> result = await logHistoryService.GetPagedLogHistoryAsync(request);
        return CreateResult(result);
    }
}
