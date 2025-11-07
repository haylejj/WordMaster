using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Dto;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels;
using WordMaster.Domain.Entities;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Controllers;

[Authorize]
[Route("/Favorite")]
public class FavoriteController(IFavoriteService favoriteService, IWordService wordService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
    {
        var userId = User.GetUserId();
        var result = await favoriteService.GetPagedFavoritesAsync(userId!, search, page, pageSize);

        var viewModel = new FavoriteListViewModel
        {
            Words = result.IsSuccess ? [.. result.Data.Favorites.Where(x => x.Word != null).Select(x => x.Word!)] : [],
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
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Favori işlemi sırasında bir sorun oluştu.";
            return Redirect("/Word");
        }
        TempData["SuccessMessage"] = result.Data == true ? "Favorilere eklendi." : "Favorilerden çıkarıldı.";
        return Redirect("/Word");
    }
    [HttpGet("UpdateFavorite")]
    public async Task<IActionResult> UpdateFavorite(int id)
    {
        var userId = User.GetUserId();
        var result = await favoriteService.GetFavoriteWithWordAsync(id, userId!);
        if (!result.IsSuccess || result.Data?.Word == null)
        {
            return NotFound();
        }
        var viewModel = new WordViewModel
        {
            Id = result.Data.Word.Id,
            EnglishWord = result.Data.Word.EnglishWord,
            TurkishWord = result.Data.Word.TurkishWord
        };
        return View(viewModel);
    }
    [HttpGet("GetWord")]
    public async Task<IActionResult> GetWord(int favoriteId)
    {
        var userId = User.GetUserId();
        var result = await favoriteService.GetFavoriteWithWordAsync(favoriteId, userId!);

        if (!result.IsSuccess || result.Data?.Word == null)
        {
            return Json(new { success = false, message = "Kelime bulunamad�." });
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
    public async Task<IActionResult> UpdateWord(WordViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Ge�ersiz veri." });
        }

        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Kullan�c� bilgisi bulunamad�." });
        }

        var wordDto = new WordDto
        {
            Id = viewModel.Id,
            EnglishWord = viewModel.EnglishWord,
            TurkishWord = viewModel.TurkishWord
        };

        var updateResult = await wordService.UpdateWordAsync(wordDto.Id, wordDto, userId);

        if (!updateResult.IsSuccess)
        {
            return Json(new { success = false, message = updateResult.ErrorMessage ?? "Kelime g�ncellenirken bir hata olu�tu." });
        }

        return Json(new { success = true, message = "Kelime ba�ar�yla g�ncellendi." });
    }

    [HttpPost("DeleteFavorite")]
    public async Task<IActionResult> DeleteFavorite(int id)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Kullan�c� bilgisi bulunamad�." });
        }

        var result = await favoriteService.DeleteFavoriteAsync(id, userId);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Favori silinirken bir hata olu�tu." });
        }

        return Json(new { success = true, message = "Favori ba�ar�yla silindi." });
    }
}
