using Core.Entity;
using Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserInterface.Extensions;

namespace UserInterface.Controllers;

[Authorize]
[Route("/Favorite")]
public class FavoriteController(IFavoriteService favoriteService, IWordService wordService) : Controller
{
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        var favorities = await favoriteService.Where(x => x.UserId == userId).Include(x => x.Word).ToListAsync();
        return View(favorities);
    }
    [HttpPost("ToggleFavorite")]
    public async Task<IActionResult> ToggleFavorite(int id)
    {
        var userId = User.GetUserId();
        var word = await wordService.Where(x => x.Id == id && x.UserId == userId).FirstOrDefaultAsync();
        if (word == null)
        {
            return Json(new { success = false, message = "Kelime bulunamadı." });
        }

        // Aynı kelime zaten favorite'da mı kontrol et
        var existingFavorite = await favoriteService.Where(x => x.WordId == word.Id && x.UserId == userId).FirstOrDefaultAsync();

        if (existingFavorite == null)
        {
            // Ekle
            var favorite = new Favorite
            {
                WordId = word.Id,
                UserId = userId,
                CreatedTime = DateTime.Now
            };
            await favoriteService.AddAsync(favorite);
            return Json(new { success = true, isFavorite = true, message = "Favorilere eklendi." });
        }
        else
        {
            // Çıkar
            await favoriteService.RemoveAsync(existingFavorite);
            return Json(new { success = true, isFavorite = false, message = "Favorilerden çıkarıldı." });
        }
    }

    [HttpGet("AddFavorite")]
    public async Task<IActionResult> AddFavorite(int id)
    {
        var userId = User.GetUserId();
        var word = await wordService.Where(x => x.Id == id && x.UserId == userId).FirstOrDefaultAsync();
        if (word != null)
        {
            // Aynı kelime zaten favorite'da mı kontrol et
            var existingFavorite = await favoriteService.Where(x => x.WordId == word.Id && x.UserId == userId).FirstOrDefaultAsync();
            if (existingFavorite == null)
            {
                var favorite = new Favorite
                {
                    WordId = word.Id,
                    UserId = userId,
                    CreatedTime = DateTime.Now
                };
                await favoriteService.AddAsync(favorite);
            }
        }

        return RedirectToAction("Index", "Word");
    }
    [HttpGet("UpdateFavorite")]
    public async Task<IActionResult> UpdateFavorite(int id)
    {
        var userId = User.GetUserId();
        var favorite = await favoriteService.Where(x => x.Id == id && x.UserId == userId).Include(x => x.Word).FirstOrDefaultAsync();
        return favorite == null ? NotFound() : View(favorite.Word);
    }
    [HttpGet("GetWord")]
    public async Task<IActionResult> GetWord(int favoriteId)
    {
        var userId = User.GetUserId();
        var favorite = await favoriteService.Where(x => x.Id == favoriteId && x.UserId == userId)
            .Include(x => x.Word)
            .FirstOrDefaultAsync();

        if (favorite?.Word == null)
        {
            return Json(new { success = false, message = "Kelime bulunamadı." });
        }

        return Json(new
        {
            success = true,
            word = new
            {
                id = favorite.Word.Id,
                englishWord = favorite.Word.EnglishWord,
                turkishWord = favorite.Word.TurkishWord
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

    [HttpPost("DeleteFavorite")]
    public async Task<IActionResult> DeleteFavorite(int id)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Kullanıcı bilgisi bulunamadı." });
        }

        var (success, errorMessage) = await favoriteService.DeleteFavoriteAsync(id, userId);

        if (!success)
        {
            return Json(new { success = false, message = errorMessage ?? "Favori silinirken bir hata oluştu." });
        }

        return Json(new { success = true, message = "Favori başarıyla silindi." });
    }
}
