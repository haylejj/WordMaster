using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests.Folder;
using WordMaster.Application.Responses.Folder;
using WordMaster.Application.Responses.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Controllers;

[Authorize]
[Route("/Folder")]
public class FolderController(IFolderService folderService, IWordService wordService) : Controller
{

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return RedirectToAction("LogIn", "Login");
        }

        ServiceResult<List<FolderResponse>> foldersResult = await folderService.GetUserFoldersAsync(userId);
        if (!foldersResult.IsSuccess)
        {
            TempData["ErrorMessage"] = foldersResult.ErrorMessage() ?? "Klasörler getirilirken hata oluştu.";
            return View(new List<FolderResponse>());
        }

        return View(foldersResult.Data ?? []);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return RedirectToAction("LogIn", "Login");
        }

        ServiceResult<FolderResponse> folderResult = await folderService.GetUserFolderAsync(id, userId);
        if (!folderResult.IsSuccess)
        {
            TempData["ErrorMessage"] = "Klasör bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        ServiceResult<List<WordResponse>> wordsResult = await folderService.GetWordsInFolderAsync(id, userId);
        if (!wordsResult.IsSuccess)
        {
            TempData["ErrorMessage"] = wordsResult.ErrorMessage() ?? "Klasör kelimeleri getirilemedi.";
            return RedirectToAction(nameof(Index));
        }

        FolderDetailResponse vm = new()
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
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Kullanıcı oturumu bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        ServiceResult result = await folderService.AddFolderAsync(new CreateFolderRequest { Name = name }, userId);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage() ?? "İşlem başarısız.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Klasör eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("AddWord")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddWord(int folderId, int wordId)
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Kullanıcı oturumu bulunamadı.";
            return RedirectToAction(nameof(Detail), new { id = folderId });
        }

        ServiceResult result = await folderService.AddWordToFolderAsync(new AddWordToFolderRequest { FolderId = folderId, WordId = wordId }, userId);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage() ?? "İşlem başarısız.";
            return RedirectToAction(nameof(Detail), new { id = folderId });
        }
        TempData["SuccessMessage"] = "Kelime klasöre eklendi.";
        return RedirectToAction(nameof(Detail), new { id = folderId });
    }

    [HttpPost("RemoveWord")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveWord(int folderId, int wordId)
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Kullanıcı oturumu bulunamadı.";
            return RedirectToAction(nameof(Detail), new { id = folderId });
        }

        ServiceResult result = await folderService.RemoveWordFromFolderAsync(new AddWordToFolderRequest { FolderId = folderId, WordId = wordId }, userId);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage() ?? "İşlem başarısız.";
            return RedirectToAction(nameof(Detail), new { id = folderId });
        }
        TempData["SuccessMessage"] = "Kelime klasörden kaldırıldı.";
        return RedirectToAction(nameof(Detail), new { id = folderId });
    }

    [HttpGet("UserWords")]
    public async Task<IActionResult> GetUserWords()
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Json(new { results = Array.Empty<object>() });
        }
        ServiceResult<List<WordLookupResponse>> wordsResult = await wordService.GetUserWordsAsync(userId);
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
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Json(new { success = false, message = "Kullanıcı oturumu bulunamadı." });
        }

        ServiceResult<FolderResponse> folderResult = await folderService.GetUserFolderAsync(id, userId);
        if (!folderResult.IsSuccess || folderResult.Data == null)
        {
            return Json(new { success = false, message = folderResult.ErrorMessage() ?? "Klasör bulunamadı." });
        }

        return Json(new { success = true, folder = new { id = folderResult.Data.Id, name = folderResult.Data.Name } });
    }

    [HttpPost("Update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, string name)
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Json(new { success = false, message = "Kullanıcı oturumu bulunamadı." });
        }

        ServiceResult result = await folderService.UpdateFolderAsync(id, new UpdateFolderRequest { Name = name }, userId);
        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessage() ?? "Güncelleme başarısız." });
        }

        return Json(new { success = true, message = "Klasör güncellendi." });
    }

    [HttpPost("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Json(new { success = false, message = "Kullanıcı oturumu bulunamadı." });
        }

        ServiceResult result = await folderService.DeleteFolderAsync(id, userId);
        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessage() ?? "Silme başarısız." });
        }

        return Json(new { success = true, message = "Klasör silindi." });
    }
}
