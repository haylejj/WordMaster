using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WordMaster.Application.Constants;
using WordMaster.Application.Responses.Version;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers;

/// <summary>
/// Uygulama versiyon bilgilerini döndüren controller.
/// </summary>
[Route("api/v{version:apiVersion}/version")]
public class VersionController : BaseController
{
    /// <summary>
    /// Uygulama versiyon bilgilerini döndürür.
    /// </summary>
    /// <returns>Versiyon bilgileri.</returns>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetVersion()
    {
        VersionResponse versionInfo = new()
        {
            AppName = AppInfo.Name,
            AppVersion = AppInfo.Version,
            ApiVersion = "1.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        };

        return CreateResult(ServiceResult<VersionResponse>.Success(versionInfo, HttpStatusCode.OK));
    }
}

