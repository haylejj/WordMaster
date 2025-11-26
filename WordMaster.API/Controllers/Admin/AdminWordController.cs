using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers.Admin;

[Authorize(Roles = "admin")]
[Route("api/admin/words")]
public class AdminWordController(IWordService wordService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetPagedWords([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        ServiceResult<PagedResult<AdminWordResponse>> result = await wordService.GetAdminPagedWordsAsync(search, page, pageSize);
        return CreateResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWord(long id, [FromBody] UpdateWordRequest request)
    {
        ServiceResult<AdminWordResponse> result = await wordService.AdminUpdateWordAsync(id, request);
        return CreateResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWord(long id)
    {
        ServiceResult result = await wordService.AdminDeleteWordAsync(id);
        return CreateResult(result);
    }
}
