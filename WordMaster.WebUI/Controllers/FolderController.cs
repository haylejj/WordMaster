using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Dto.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.Folder;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Controllers;

[Authorize]
[Route("/Folder")]
public class FolderController(IFolderService folderService) : Controller
{

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        string? userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("LogIn", "Login");
        }

        Result<List<Folder>> foldersResult = await folderService.GetUserFoldersAsync(userId);
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
        string? userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("LogIn", "Login");
        }

        Result<Folder> folderResult = await folderService.GetUserFolderAsync(id, userId);
        if (!folderResult.IsSuccess)
        {
            TempData["ErrorMessage"] = "Klasör bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        Result<List<Word>> wordsResult = await folderService.GetWordsInFolderAsync(id, userId);
        if (!wordsResult.IsSuccess)
        {
            TempData["ErrorMessage"] = wordsResult.ErrorMessage ?? "Klasör kelimeleri getirilemedi.";
            return RedirectToAction(nameof(Index));
        }

        FolderDetailViewModel vm = new()
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
        string? userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Kullanıcı oturumu bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        Result result = await folderService.AddFolderAsync(name, userId);
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
        string? userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Kullanıcı oturumu bulunamadı.";
            return RedirectToAction(nameof(Detail), new { id = folderId });
        }

        Result result = await folderService.AddWordToFolderAsync(folderId, wordId, userId);
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
        string? userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Kullanıcı oturumu bulunamadı.";
            return RedirectToAction(nameof(Detail), new { id = folderId });
        }

        Result result = await folderService.RemoveWordFromFolderAsync(folderId, wordId, userId);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "İşlem başarısız.";
            return RedirectToAction(nameof(Detail), new { id = folderId });
        }
        TempData["SuccessMessage"] = "Kelime klasörden kaldırıldı.";
        return RedirectToAction(nameof(Detail), new { id = folderId });
    }

    [HttpGet("UserWords")]
    public async Task<IActionResult> GetUserWords()
    {
        string? userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { results = Array.Empty<object>() });
        }
        Result<List<WordLookupDto>> wordsResult = await folderService.GetUserWordsAsync(userId);
        if (!wordsResult.IsSuccess || wordsResult.Data == null)
        {
            return Json(new { results = Array.Empty<object>() });
        }
        var results = wordsResult.Data.Select(w => new { id = w.Id, text = w.EnglishWord ?? string.Empty });
        return Json(new { results });
    }

    [HttpGet("GetFolder")]
    public async Task<IActionResult> GetFolder(int id)
    {
        string? userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Kullanıcı oturumu bulunamadı." });
        }

        Result<Folder> folderResult = await folderService.GetUserFolderAsync(id, userId);
        if (!folderResult.IsSuccess || folderResult.Data == null)
        {
            return Json(new { success = false, message = folderResult.ErrorMessage ?? "Klasör bulunamadı." });
        }

        return Json(new { success = true, folder = new { id = folderResult.Data.Id, name = folderResult.Data.Name } });
    }

    [HttpPost("Update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, string name)
    {
        string? userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Kullanıcı oturumu bulunamadı." });
        }

        Result result = await folderService.UpdateFolderAsync(id, name, userId);
        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Güncelleme başarısız." });
        }

        return Json(new { success = true, message = "Klasör güncellendi." });
    }

    [HttpPost("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        string? userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Kullanıcı oturumu bulunamadı." });
        }

        Result result = await folderService.DeleteFolderAsync(id, userId);
        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Silme başarısız." });
        }

        return Json(new { success = true, message = "Klasör silindi." });
    }
}


