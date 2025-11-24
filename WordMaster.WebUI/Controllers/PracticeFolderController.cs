using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Folder;
using WordMaster.Application.Responses.Practice;
using WordMaster.Application.Responses.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Controllers;

[Authorize]
[Route("/PracticeFolder")]
public class PracticeFolderController(IFolderService folderService, IWordService wordService) : Controller
{
    [HttpGet("", Name = "PracticeFolderIndex")]
    public async Task<IActionResult> Index(long folderId)
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return RedirectToAction("LogIn", "Login");
        }

        if (folderId <= 0)
        {
            TempData["ErrorMessage"] = "Klasör bulunamadı.";
            return RedirectToAction("Index", "Folder");
        }

        ServiceResult<FolderResponse> folderResult = await folderService.GetUserFolderAsync(folderId, userId);
        if (!folderResult.IsSuccess || folderResult.Data == null)
        {
            TempData["ErrorMessage"] = folderResult.ErrorMessage() ?? "Klasör bulunamadı.";
            return RedirectToAction("Index", "Folder");
        }

        ServiceResult<List<WordResponse>> wordsResult = await folderService.GetWordsInFolderAsync(folderId, userId);
        if (!wordsResult.IsSuccess || wordsResult.Data == null)
        {
            TempData["ErrorMessage"] = wordsResult.ErrorMessage() ?? "Bu klasörde pratik yapılacak kelime yok.";
            return RedirectToAction("Detail", "Folder", new { id = folderId });
        }

        List<PracticeFolderWordResponse> words = wordsResult.Data
            .Where(w => w != null && !string.IsNullOrWhiteSpace(w.EnglishWord))
            .OrderBy(w => w!.EnglishWord)
            .Select(w => new PracticeFolderWordResponse
            {
                WordId = w!.Id,
                EnglishWord = w.EnglishWord ?? string.Empty,
                TurkishWord = w.TurkishWord ?? string.Empty
            })
            .ToList();

        if (words.Count == 0)
        {
            TempData["ErrorMessage"] = "Bu klasörde pratik yapılacak kelime yok.";
            return RedirectToAction("Detail", "Folder", new { id = folderId });
        }

        PracticeFolderResponse response = new()
        {
            FolderId = folderId,
            FolderName = folderResult.Data.Name ?? string.Empty,
            Words = words
        };

        return View(response);
    }

    public record PracticeFolderCheckRequest(string? TurkishWord, string? EnglishWord);

    [HttpPost("CheckTranslation")]
    public async Task<IActionResult> CheckTranslation([FromBody] PracticeFolderCheckRequest? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.EnglishWord))
        {
            return BadRequest(new { success = false, errorMessage = "Geçersiz kelime bilgisi." });
        }

        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { success = false, errorMessage = "Oturum bulunamadı." });
        }

        string turkishWord = request.TurkishWord ?? string.Empty;
        ServiceResult<bool> result = await wordService.CheckTranslationAndUpdateAsync(userId, new CheckTranslationRequest { EnglishWord=request.EnglishWord, TurkishWord=request.TurkishWord! });

        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                success = false,
                errorMessage = result.ErrorMessage() ?? "Kontrol işlemi sırasında bir hata oluştu."
            });
        }

        return Ok(new { success = true, isCorrect = result.Data });
    }
}

