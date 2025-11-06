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
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
    {
        var userId = User.GetUserId();
        var result = await unknowsService.GetPagedUnknowsAsync(userId!, search, page, pageSize);

        var viewModel = new Core.ViewModels.UnknowsListViewModel
        {
            Words = result.IsSuccess ? result.Data.Unknows.Where(x => x.Word != null).Select(x => x.Word!).ToList() : new List<Core.Entity.Word>(),
            Page = page,
            PageSize = pageSize,
            TotalCount = result.IsSuccess ? result.Data.TotalCount : 0,
            Search = search
        };

        return View(viewModel);
    }
    [HttpPost("ToggleUnknows")]
    public async Task<IActionResult> ToggleUnknows(int id)
    {
        var userId = User.GetUserId();
        var result = await unknowsService.ToggleUnknowsAsync(id, userId!);
        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "İşlem başarısız." });
        }
        return Json(new { success = true, isUnknows = result.Data, message = result.Data == true ? "Bilinmeyenlere eklendi." : "Bilinmeyenlerden çıkarıldı." });
    }

    public async Task<IActionResult> AddUnknows(int id)
    {
        var userId = User.GetUserId();
        var result = await unknowsService.ToggleUnknowsAsync(id, userId!);
        return RedirectToAction("Index", "Word");
    }
    [HttpGet("UpdateUnknows")]
    public async Task<IActionResult> UpdateUnknows(int id)
    {
        var userId = User.GetUserId();
        var result = await unknowsService.GetUnknowsWithWordAsync(id, userId!);
        return !result.IsSuccess || result.Data?.Word == null ? NotFound() : View(result.Data.Word);
    }
    [HttpGet("GetWord")]
    public async Task<IActionResult> GetWord(int unknowsId)
    {
        var userId = User.GetUserId();
        var result = await unknowsService.GetUnknowsWithWordAsync(unknowsId, userId!);

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

    [HttpPost("DeleteUnknows")]
    public async Task<IActionResult> DeleteUnknows(int id)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Kullanıcı bilgisi bulunamadı." });
        }

        var result = await unknowsService.DeleteUnknowsAsync(id, userId);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Bilinmeyen kelime silinirken bir hata oluştu." });
        }

        return Json(new { success = true, message = "Bilinmeyen kelime başarıyla silindi." });
    }
}
