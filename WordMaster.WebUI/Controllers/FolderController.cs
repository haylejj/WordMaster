using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordMaster.Domain.Entities;
using WordMaster.Infrastructure.EfCore;
using WordMaster.WebUI.Extensions;
using WordMaster.WebUI.Models;

namespace WordMaster.WebUI.Controllers;

[Authorize]
[Route("/Folder")]
public class FolderController(AppDbContext dbContext) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("LogIn", "Login");
        }

        var folders = await dbContext.Folders
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedTime)
            .ToListAsync();

        return View(folders);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("LogIn", "Login");
        }

        var folder = await dbContext.Folders
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
        if (folder == null)
        {
            TempData["ErrorMessage"] = "Klasör bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var wordsInFolder = await dbContext.WordFolders
            .AsNoTracking()
            .Where(wf => wf.FolderId == id)
            .Include(wf => wf.Word)
            .Select(wf => wf.Word)
            .ToListAsync();

        var allUserWords = await dbContext.Words
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.EnglishWord)
            .ToListAsync();

        var vm = new FolderDetailViewModel
        {
            FolderId = folder.Id,
            FolderName = folder.Name,
            Words = wordsInFolder,
            AllWords = allUserWords
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

        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["ErrorMessage"] = "Klasör adı gereklidir.";
            return RedirectToAction(nameof(Index));
        }

        // Aynı isimde klasör var mı kontrolü (isteğe bağlı)
        var exists = await dbContext.Folders.AnyAsync(f => f.UserId == userId && f.Name == name.Trim());
        if (exists)
        {
            TempData["ErrorMessage"] = "Bu isimde bir klasör zaten mevcut.";
            return RedirectToAction(nameof(Index));
        }

        var folder = new Folder
        {
            Name = name.Trim(),
            UserId = userId,
            CreatedTime = DateTime.UtcNow
        };

        dbContext.Folders.Add(folder);
        await dbContext.SaveChangesAsync();

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

        // yetki ve varlık kontrolleri
        var folderExists = await dbContext.Folders.AnyAsync(f => f.Id == folderId && f.UserId == userId);
        var wordExists = await dbContext.Words.AnyAsync(w => w.Id == wordId && w.UserId == userId);
        if (!folderExists || !wordExists)
        {
            TempData["ErrorMessage"] = "Klasör veya kelime bulunamadı.";
            return RedirectToAction(nameof(Detail), new { id = folderId });
        }

        var linkExists = await dbContext.WordFolders.AnyAsync(x => x.FolderId == folderId && x.WordId == wordId);
        if (linkExists)
        {
            TempData["ErrorMessage"] = "Kelime zaten bu klasörde.";
            return RedirectToAction(nameof(Detail), new { id = folderId });
        }

        dbContext.WordFolders.Add(new WordFolder { FolderId = folderId, WordId = wordId });
        await dbContext.SaveChangesAsync();
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

        var folder = await dbContext.Folders.FirstOrDefaultAsync(f => f.Id == folderId && f.UserId == userId);
        if (folder == null)
        {
            TempData["ErrorMessage"] = "Klasör bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var link = await dbContext.WordFolders.FirstOrDefaultAsync(x => x.FolderId == folderId && x.WordId == wordId);
        if (link == null)
        {
            TempData["ErrorMessage"] = "Kelime bu klasörde değil.";
            return RedirectToAction(nameof(Detail), new { id = folderId });
        }

        dbContext.WordFolders.Remove(link);
        await dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Kelime klasörden kaldırıldı.";
        return RedirectToAction(nameof(Detail), new { id = folderId });
    }
}


