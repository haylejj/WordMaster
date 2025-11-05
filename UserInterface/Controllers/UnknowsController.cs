using Core.Entity;
using Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserInterface.Extensions;

namespace UserInterface.Controllers;

[Authorize]
[Route("/Unknows")]
public class UnknowsController(IUnknowsService unknowsService, IWordService wordService) : Controller
{
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        var unknows = await unknowsService.Where(x => x.UserId == userId).Include(x => x.Word).ToListAsync();
        return View(unknows);
    }
    [HttpPost("ToggleUnknows")]
    public async Task<IActionResult> ToggleUnknows(int id)
    {
        var userId = User.GetUserId();
        var word = await wordService.Where(x => x.Id == id && x.UserId == userId).FirstOrDefaultAsync();
        if (word == null)
        {
            return Json(new { success = false, message = "Kelime bulunamadı." });
        }

        // Aynı kelime zaten unknown'da mı kontrol et
        var existingUnknow = await unknowsService.Where(x => x.WordId == word.Id && x.UserId == userId).FirstOrDefaultAsync();
        
        if (existingUnknow == null)
        {
            // Ekle
            var unknow = new Unknows
            {
                WordId = word.Id,
                UserId = userId,
                CreatedTime = DateTime.Now
            };
            await unknowsService.AddAsync(unknow);
            return Json(new { success = true, isUnknows = true, message = "Bilinmeyenlere eklendi." });
        }
        else
        {
            // Çıkar
            await unknowsService.RemoveAsync(existingUnknow);
            return Json(new { success = true, isUnknows = false, message = "Bilinmeyenlerden çıkarıldı." });
        }
    }

    public async Task<IActionResult> AddUnknows(int id)
    {
        var userId = User.GetUserId();
        var word = await wordService.Where(x => x.Id == id && x.UserId == userId).FirstOrDefaultAsync();
        if (word != null)
        {
            // Aynı kelime zaten unknown'da mı kontrol et
            var existingUnknow = await unknowsService.Where(x => x.WordId == word.Id && x.UserId == userId).FirstOrDefaultAsync();
            if (existingUnknow == null)
            {
                var unknow = new Unknows
                {
                    WordId = word.Id,
                    UserId = userId,
                    CreatedTime = DateTime.Now
                };
                await unknowsService.AddAsync(unknow);
            }
        }
        return RedirectToAction("Index", "Word");
    }
    [HttpGet("UpdateUnknows")]
    public async Task<IActionResult> UpdateUnknows(int id)
    {
        var userId = User.GetUserId();
        var unknow = await unknowsService.Where(x => x.Id == id && x.UserId == userId).Include(x => x.Word).FirstOrDefaultAsync();
        return unknow == null ? NotFound() : View(unknow.Word);
    }
    [HttpGet("GetWord")]
    public async Task<IActionResult> GetWord(int unknowsId)
    {
        var userId = User.GetUserId();
        var unknow = await unknowsService.Where(x => x.Id == unknowsId && x.UserId == userId)
            .Include(x => x.Word)
            .FirstOrDefaultAsync();
        
        if (unknow?.Word == null)
        {
            return Json(new { success = false, message = "Kelime bulunamadı." });
        }

        return Json(new { 
            success = true, 
            word = new { 
                id = unknow.Word.Id, 
                englishWord = unknow.Word.EnglishWord, 
                turkishWord = unknow.Word.TurkishWord 
            } 
        });
    }

    [HttpPost("UpdateWord")]
    public async Task<IActionResult> UpdateWord(Core.Dto.WordDto wordDto)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Geçersiz veri." });
        }

        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Kullanıcı bilgisi bulunamadı." });
        }

        var (success, errorMessage) = await wordService.UpdateWordAsync(wordDto.Id, wordDto, userId);

        if (!success)
        {
            return Json(new { success = false, message = errorMessage ?? "Kelime güncellenirken bir hata oluştu." });
        }

        return Json(new { success = true, message = "Kelime başarıyla güncellendi." });
    }

    [HttpPost("DeleteUnknows")]
    public async Task<IActionResult> DeleteUnknows(int id)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Kullanıcı bilgisi bulunamadı." });
        }

        var (success, errorMessage) = await unknowsService.DeleteUnknowsAsync(id, userId);
        
        if (!success)
        {
            return Json(new { success = false, message = errorMessage ?? "Bilinmeyen kelime silinirken bir hata oluştu." });
        }

        return Json(new { success = true, message = "Bilinmeyen kelime başarıyla silindi." });
    }
}
