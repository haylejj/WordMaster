using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.API.Extensions;
using WordMaster.Application.Requests.Database;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers.Admin;

/// <summary>
/// Controller for administrative database operations such as resetting tables.
/// Requires 'admin' role.
/// </summary>
[Authorize(Roles = "admin")]
[Route("api/admin/database")]
public class DatabaseController(IDatabaseService databaseService) : BaseController
{
    /// <summary>
    /// Resets the specified database table after verifying the user's password.
    /// This is a high-risk operation that deletes data.
    /// </summary>
    /// <param name="request">The reset request containing table name and admin password.</param>
    /// <returns>A result indicating success or failure.</returns>
    [HttpPost("reset")]
    public async Task<IActionResult> ResetTable([FromBody] ResetTableRequest request)
    {
        Guid userId = User.GetUserId();
        ServiceResult result = await databaseService.ResetTableAsync(request.TableName, request.Password, userId.ToString(), request.TargetUserId);
        return CreateResult(result);
    }
}
