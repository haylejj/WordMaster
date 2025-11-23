using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Dto.Folder;
using WordMaster.Application.Dto.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.Practice;
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

        ServiceResult<FolderDto> folderResult = await folderService.GetUserFolderAsync(folderId, userId);
        if (!folderResult.IsSuccess || folderResult.Data == null)
        {
            TempData["ErrorMessage"] = folderResult.ErrorMessage() ?? "Klasör bulunamadı.";
            return RedirectToAction("Index", "Folder");
        }

        ServiceResult<List<WordDto>> wordsResult = await folderService.GetWordsInFolderAsync(folderId, userId);
        if (!wordsResult.IsSuccess || wordsResult.Data == null)
        {
            TempData["ErrorMessage"] = wordsResult.ErrorMessage() ?? "Bu klasörde pratik yapılacak kelime yok.";
            return RedirectToAction("Detail", "Folder", new { id = folderId });
        }

        List<PracticeFolderWordViewModel> words = wordsResult.Data
            .Where(w => w != null && !string.IsNullOrWhiteSpace(w.EnglishWord))
            .OrderBy(w => w!.EnglishWord)
            .Select(w => new PracticeFolderWordViewModel
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

        PracticeFolderViewModel viewModel = new()
        {
            FolderId = folderId,
            FolderName = folderResult.Data.Name ?? string.Empty,
            Words = words
        };

        return View(viewModel);
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
        ServiceResult<bool> result = await wordService.CheckTranslationAndUpdateAsync(userId, turkishWord, request.EnglishWord);

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

