using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers;

/// <summary>
/// Base controller that provides common functionality for all API controllers.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
public class BaseController : ControllerBase
{
    /// <summary>
    /// Creates an IActionResult based on the ServiceResult.
    /// </summary>
    /// <typeparam name="T">The type of the data returned in the result.</typeparam>
    /// <param name="result">The service result to convert.</param>
    /// <returns>An IActionResult representing the service result.</returns>
    [NonAction]
    protected IActionResult CreateResult<T>(ServiceResult<T> result)
    {
        return new ObjectResult(result) { StatusCode = (int)result.StatusCode };
    }

    /// <summary>
    /// Creates an IActionResult based on the ServiceResult.
    /// </summary>
    /// <param name="result">The service result to convert.</param>
    /// <returns>An IActionResult representing the service result.</returns>
    [NonAction]
    protected IActionResult CreateResult(ServiceResult result)
    {
        return new ObjectResult(result) { StatusCode = (int)result.StatusCode };
    }
}
