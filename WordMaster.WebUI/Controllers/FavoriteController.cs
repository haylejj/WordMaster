using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Dto.Word;
using WordMaster.Application.Dto.Favorite;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.Favorite;
using WordMaster.Application.ViewModels.Word;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Controllers;

[Authorize]
[Route("/Favorite")]
public class FavoriteController(IFavoriteService favoriteService, IWordService wordService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return RedirectToAction("LogIn", "Login");
        }

        ServiceResult<PagedResult<FavoriteWithWordDto>> result = await favoriteService.GetPagedFavoritesAsync(userId, search, page, pageSize);

        FavoriteListViewModel viewModel = new()
        {
            Favorites = result.IsSuccess && result.Data != null ? result.Data.Items : [],
            Page = page,
            PageSize = pageSize,
            TotalCount = result.IsSuccess && result.Data != null ? result.Data.TotalCount : 0,
            Search = search
        };

        return View(viewModel);
    }
    [HttpPost("ToggleFavorite")]
    public async Task<IActionResult> ToggleFavorite(long id)
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Json(new { success = false, message = "Kullanıcı oturumu bulunamadı." });
        }

        ServiceResult<bool> result = await favoriteService.ToggleFavoriteAsync(id, userId);
        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorList?.FirstOrDefault() ?? "İşlem başarısız." });
        }
        return Json(new { success = true, isFavorite = result.Data, message = result.Data == true ? "Favorilere eklendi." : "Favorilerden çıkarıldı." });
    }

    [HttpGet("AddFavorite")]
    public async Task<IActionResult> AddFavorite(long id)
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Kullanıcı oturumu bulunamadı.";
            return Redirect("/Word");
        }

        ServiceResult<bool> result = await favoriteService.ToggleFavoriteAsync(id, userId);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorList?.FirstOrDefault() ?? "Favori işlemi sırasında bir sorun oluştu.";
            return Redirect("/Word");
        }
        TempData["SuccessMessage"] = result.Data == true ? "Favorilere eklendi." : "Favorilerden çıkarıldı.";
        return Redirect("/Word");
    }
    [HttpGet("UpdateFavorite")]
    public async Task<IActionResult> UpdateFavorite(int id)
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return NotFound();
        }

        ServiceResult<FavoriteWithWordDto> result = await favoriteService.GetFavoriteWithWordAsync(id, userId);
        if (!result.IsSuccess || result.Data?.Word == null)
        {
            return NotFound();
        }
        WordViewModel viewModel = new()
        {
            Id = result.Data.Word.Id,
            EnglishWord = result.Data.Word.EnglishWord,
            TurkishWord = result.Data.Word.TurkishWord
        };
        return View(viewModel);
    }
    [HttpGet("GetWord")]
    public async Task<IActionResult> GetWord(long favoriteId)
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Json(new { success = false, message = "Kullanıcı bulunamadı." });
        }

        // favoriteId aslında Word Id olarak gönderiliyor, bu yüzden WordService kullanıyoruz
        ServiceResult<WordDto> result = await wordService.GetWordForUserAsync(favoriteId, userId);

        if (!result.IsSuccess || result.Data == null)
        {
            return Json(new { success = false, message = "Kelime bulunamadı." });
        }

        return Json(new
        {
            success = true,
            word = new
            {
                id = result.Data.Id,
                englishWord = result.Data.EnglishWord,
                turkishWord = result.Data.TurkishWord
            }
        });
    }

    [HttpPost("UpdateWord")]
    public async Task<IActionResult> UpdateWord(WordViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Geçersiz veri." });
        }

        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Json(new { success = false, message = "Kullanıcı bilgisi bulunamadı." });
        }

        WordDto wordDto = new()
        {
            Id = viewModel.Id,
            EnglishWord = viewModel.EnglishWord,
            TurkishWord = viewModel.TurkishWord
        };

        ServiceResult updateResult = await wordService.UpdateWordAsync(wordDto.Id, wordDto, userId);

        if (!updateResult.IsSuccess)
        {
            return Json(new { success = false, message = updateResult.ErrorList?.FirstOrDefault() ?? "Kelime güncellenirken bir hata oluştu." });
        }

        return Json(new { success = true, message = "Kelime başarıyla güncellendi." });
    }

    [HttpPost("DeleteFavorite")]
    public async Task<IActionResult> DeleteFavorite(int id)
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Json(new { success = false, message = "Kullanıcı bilgisi bulunamadı." });
        }

        ServiceResult result = await favoriteService.DeleteFavoriteAsync(id, userId);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorList?.FirstOrDefault() ?? "Favori silinirken bir hata oluştu." });
        }

        return Json(new { success = true, message = "Favori başarıyla silindi." });
    }
}
