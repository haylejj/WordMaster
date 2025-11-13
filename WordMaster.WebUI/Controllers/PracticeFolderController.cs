using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using WordMaster.Application.Services.Abstract;
using WordMaster.WebUI.Extensions;
using WordMaster.WebUI.Models;

namespace WordMaster.WebUI.Controllers;

[Authorize]
[Route("/PracticeFolder")]
public class PracticeFolderController(IFolderService folderService, IWordService wordService) : Controller
{
    [HttpGet("", Name = "PracticeFolderIndex")]
    public async Task<IActionResult> Index(int folderId)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("LogIn", "Login");
        }

        if (folderId <= 0)
        {
            TempData["ErrorMessage"] = "Klasör bulunamadı.";
            return RedirectToAction("Index", "Folder");
        }

        var folderResult = await folderService.GetUserFolderAsync(folderId, userId);
        if (!folderResult.IsSuccess || folderResult.Data == null)
        {
            TempData["ErrorMessage"] = folderResult.ErrorMessage ?? "Klasör bulunamadı.";
            return RedirectToAction("Index", "Folder");
        }

        var wordsResult = await folderService.GetWordsInFolderAsync(folderId, userId);
        if (!wordsResult.IsSuccess || wordsResult.Data == null)
        {
            TempData["ErrorMessage"] = wordsResult.ErrorMessage ?? "Bu klasörde pratik yapılacak kelime yok.";
            return RedirectToAction("Detail", "Folder", new { id = folderId });
        }

        var words = wordsResult.Data
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

        var viewModel = new PracticeFolderViewModel
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

        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { success = false, errorMessage = "Oturum bulunamadı." });
        }

        var turkishWord = request.TurkishWord ?? string.Empty;
        var result = await wordService.CheckTranslationAndUpdateAsync(userId, turkishWord, request.EnglishWord);

        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                success = false,
                errorMessage = result.ErrorMessage ?? "Kontrol işlemi sırasında bir hata oluştu."
            });
        }

        return Ok(new { success = true, isCorrect = result.Data });
    }
}

