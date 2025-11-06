using Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserInterface.Extensions;

namespace UserInterface.Controllers;

[Authorize]
[Route("/Favorite")]
public class FavoriteController(IFavoriteService favoriteService, IWordService wordService) : Controller
{
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
    {
        var userId = User.GetUserId();
        var result = await favoriteService.GetPagedFavoritesAsync(userId!, search, page, pageSize);

        var viewModel = new Core.ViewModels.FavoriteListViewModel
        {
            Words = result.IsSuccess ? result.Data.Favorites.Where(x => x.Word != null).Select(x => x.Word!).ToList() : new List<Core.Entity.Word>(),
            Page = page,
            PageSize = pageSize,
            TotalCount = result.IsSuccess ? result.Data.TotalCount : 0,
            Search = search
        };

        return View(viewModel);
    }
    [HttpPost("ToggleFavorite")]
    public async Task<IActionResult> ToggleFavorite(int id)
    {
        var userId = User.GetUserId();
        var result = await favoriteService.ToggleFavoriteAsync(id, userId!);
        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "İşlem başarısız." });
        }
        return Json(new { success = true, isFavorite = result.Data, message = result.Data == true ? "Favorilere eklendi." : "Favorilerden çıkarıldı." });
    }

    [HttpGet("AddFavorite")]
    public async Task<IActionResult> AddFavorite(int id)
    {
        var userId = User.GetUserId();
        var result = await favoriteService.ToggleFavoriteAsync(id, userId!);

        return RedirectToAction("Index", "Word");
    }
    [HttpGet("UpdateFavorite")]
    public async Task<IActionResult> UpdateFavorite(int id)
    {
        var userId = User.GetUserId();
        var result = await favoriteService.GetFavoriteWithWordAsync(id, userId!);
        return !result.IsSuccess || result.Data?.Word == null ? NotFound() : View(result.Data.Word);
    }
    [HttpGet("GetWord")]
    public async Task<IActionResult> GetWord(int favoriteId)
    {
        var userId = User.GetUserId();
        var result = await favoriteService.GetFavoriteWithWordAsync(favoriteId, userId!);

        if (!result.IsSuccess || result.Data?.Word == null)
        {
            return Json(new { success = false, message = "Kelime bulunamadı." });
        }

        return Json(new
        {
            success = true,
            word = new
            {
                id = result.Data.Word.Id,
                englishWord = result.Data.Word.EnglishWord,
                turkishWord = result.Data.Word.TurkishWord
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

        var updateResult = await wordService.UpdateWordAsync(wordDto.Id, wordDto, userId);

        if (!updateResult.IsSuccess)
        {
            return Json(new { success = false, message = updateResult.ErrorMessage ?? "Kelime güncellenirken bir hata oluştu." });
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

        var result = await favoriteService.DeleteFavoriteAsync(id, userId);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Favori silinirken bir hata oluştu." });
        }

        return Json(new { success = true, message = "Favori başarıyla silindi." });
    }
}
