using Microsoft.AspNetCore.Mvc;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers;

[ApiController]
public class BaseController : ControllerBase
{
    [NonAction]
    protected IActionResult CreateResult<T>(ServiceResult<T> result)
    {
        return new ObjectResult(result) { StatusCode = (int)result.StatusCode };
    }

    [NonAction]
    protected IActionResult CreateResult(ServiceResult result)
    {
        return new ObjectResult(result) { StatusCode = (int)result.StatusCode };
    }
}
