using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Attributes;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers.Admin;

/// <summary>
/// Controller for managing words by administrators.
/// </summary>
[Route("api/admin/words")]
public class AdminWordController(IWordService wordService) : BaseController
{
    /// <summary>
    /// Retrieves a paged list of words for administration.
    /// </summary>
    /// <param name="search">Optional search term to filter words.</param>
    /// <param name="page">The page number to retrieve (default is 1).</param>
    /// <param name="pageSize">The number of items per page (default is 10).</param>
    /// <returns>A paged result of admin word responses.</returns>
    [HttpGet]
    [RequirePermission("Admin", "AdminWords", "GetPagedWords", "GET", "Yönetici kelime listesi")]
    public async Task<IActionResult> GetPagedWords([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        ServiceResult<PagedResult<AdminWordResponse>> result = await wordService.GetAdminPagedWordsAsync(search, page, pageSize);
        return CreateResult(result);
    }

    /// <summary>
    /// Updates an existing word.
    /// </summary>
    /// <param name="id">The unique identifier of the word to update.</param>
    /// <param name="request">The update request containing the new word details.</param>
    /// <returns>The updated word response.</returns>
    [HttpPut("{id}")]
    [RequirePermission("Admin", "AdminWords", "UpdateWord", "PUT", "Yönetici kelime güncelle")]
    public async Task<IActionResult> UpdateWord(long id, [FromBody] UpdateWordRequest request)
    {
        ServiceResult<AdminWordResponse> result = await wordService.AdminUpdateWordAsync(id, request);
        return CreateResult(result);
    }

    /// <summary>
    /// Deletes a word by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the word to delete.</param>
    /// <returns>A result indicating the outcome of the operation.</returns>
    [HttpDelete("{id}")]
    [RequirePermission("Admin", "AdminWords", "DeleteWord", "DELETE", "Yönetici kelime sil")]
    public async Task<IActionResult> DeleteWord(long id)
    {
        ServiceResult result = await wordService.AdminDeleteWordAsync(id);
        return CreateResult(result);
    }
}
