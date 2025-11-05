using Core.Dto;
using Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserInterface.Extensions;

namespace UserInterface.Controllers;

[Authorize]
[Route("/Word")]
public class WordController(IWordService wordService) : Controller
{
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        var words = await wordService.Where(x => x.UserId == userId)
            .Include(x => x.Favorite)
            .Include(x => x.Unknows)
            .ToListAsync();
        return View(words);
    }
    [HttpGet("AddWord")]
    public IActionResult AddWord()
    {
        return View();
    }
    [HttpPost("AddWord")]
    public async Task<IActionResult> AddWord(WordDto wordDto)
    {
        if (!ModelState.IsValid)
        {
            return View(wordDto);
        }

        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            ModelState.AddModelError("", "Kullanıcı bilgisi bulunamadı.");
            return View(wordDto);
        }

        var (success, errorMessage) = await wordService.AddWordAsync(wordDto, userId);

        if (!success)
        {
            ModelState.AddModelError("EnglishWord", errorMessage ?? "Kelime eklenirken bir hata oluştu.");
            return View(wordDto);
        }

        TempData["SuccessMessage"] = "Kelime başarıyla eklendi!";
        return RedirectToAction(nameof(AddWord));
    }
    [HttpPost("DeleteWord")]
    public async Task<IActionResult> DeleteWord(int id)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Kullanıcı bilgisi bulunamadı." });
        }

        var (success, errorMessage) = await wordService.DeleteWordAsync(id, userId);

        if (!success)
        {
            return Json(new { success = false, message = errorMessage ?? "Kelime silinirken bir hata oluştu." });
        }

        return Json(new { success = true, message = "Kelime başarıyla silindi." });
    }

    [HttpGet("GetWord")]
    public async Task<IActionResult> GetWord(int id)
    {
        var userId = User.GetUserId();
        var word = await wordService.Where(x => x.Id == id && x.UserId == userId).FirstOrDefaultAsync();

        if (word == null)
        {
            return Json(new { success = false, message = "Kelime bulunamadı." });
        }

        return Json(new
        {
            success = true,
            word = new
            {
                id = word.Id,
                englishWord = word.EnglishWord,
                turkishWord = word.TurkishWord
            }
        });
    }

    [HttpPost("UpdateWord")]
    public async Task<IActionResult> UpdateWord(WordDto wordDto)
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

}
