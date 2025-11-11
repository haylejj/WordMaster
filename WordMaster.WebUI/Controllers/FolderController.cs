using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.WebUI.Extensions;
using WordMaster.WebUI.Models;

namespace WordMaster.WebUI.Controllers;

[Authorize]
[Route("/Folder")]
public class FolderController(IFolderService folderService) : Controller
{
    private readonly IFolderService _folderService = folderService;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("LogIn", "Login");
        }

        var foldersResult = await _folderService.GetUserFoldersAsync(userId);
        if (!foldersResult.IsSuccess)
        {
            TempData["ErrorMessage"] = foldersResult.ErrorMessage ?? "Klasörler getirilirken hata oluştu.";
            return View(new List<Folder>());
        }

        return View(foldersResult.Data ?? []);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("LogIn", "Login");
        }

        var folderResult = await _folderService.GetUserFolderAsync(id, userId);
        if (!folderResult.IsSuccess)
        {
            TempData["ErrorMessage"] = "Klasör bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var wordsResult = await _folderService.GetWordsInFolderAsync(id, userId);
        if (!wordsResult.IsSuccess)
        {
            TempData["ErrorMessage"] = wordsResult.ErrorMessage ?? "Klasör kelimeleri getirilemedi.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new FolderDetailViewModel
        {
            FolderId = folderResult.Data!.Id,
            FolderName = folderResult.Data!.Name,
            Words = wordsResult.Data ?? []
        };

        return View(vm);
    }

    [HttpPost("Add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(string name)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Kullanıcı oturumu bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _folderService.AddFolderAsync(name, userId);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "İşlem başarısız.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Klasör eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("AddWord")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddWord(int folderId, int wordId)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Kullanıcı oturumu bulunamadı.";
            return RedirectToAction(nameof(Detail), new { id = folderId });
        }

        var result = await _folderService.AddWordToFolderAsync(folderId, wordId, userId);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "İşlem başarısız.";
            return RedirectToAction(nameof(Detail), new { id = folderId });
        }
        TempData["SuccessMessage"] = "Kelime klasöre eklendi.";
        return RedirectToAction(nameof(Detail), new { id = folderId });
    }

    [HttpPost("RemoveWord")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveWord(int folderId, int wordId)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Kullanıcı oturumu bulunamadı.";
            return RedirectToAction(nameof(Detail), new { id = folderId });
        }

        var result = await _folderService.RemoveWordFromFolderAsync(folderId, wordId, userId);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "İşlem başarısız.";
            return RedirectToAction(nameof(Detail), new { id = folderId });
        }
        TempData["SuccessMessage"] = "Kelime klasörden kaldırıldı.";
        return RedirectToAction(nameof(Detail), new { id = folderId });
    }

    [HttpGet("UserWords")]
    public async Task<IActionResult> GetUserWords(string? term, int take = 20)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { results = Array.Empty<object>() });
        }
        // Fetch all words once (service caches per user); client will filter locally
        var wordsResult = await _folderService.GetUserWordsAsync(userId, null, int.MaxValue);
        if (!wordsResult.IsSuccess || wordsResult.Data == null)
        {
            return Json(new { results = Array.Empty<object>() });
        }
        var results = wordsResult.Data.Select(w => new { id = w.Id, text = w.EnglishWord ?? string.Empty });
        return Json(new { results });
    }
}


