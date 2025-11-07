using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Dto;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels;
using WordMaster.Domain.Entities;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Controllers;

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

        var viewModel = new UnknowsListViewModel
        {
            Words = result.IsSuccess ? result.Data.Unknows.Where(x => x.Word != null).Select(x => x.Word!).ToList() : new List<Word>(),
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
            return Json(new { success = false, message = result.ErrorMessage ?? "��lem ba�ar�s�z." });
        }
        return Json(new { success = true, isUnknows = result.Data, message = result.Data == true ? "Bilinmeyenlere eklendi." : "Bilinmeyenlerden ��kar�ld�." });
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
    public async Task<IActionResult> UpdateWord(WordDto wordDto)
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

        var updateResult = await wordService.UpdateWordAsync(wordDto.Id, wordDto, userId);

        if (!updateResult.IsSuccess)
        {
            return Json(new { success = false, message = updateResult.ErrorMessage ?? "Kelime g�ncellenirken bir hata olu�tu." });
        }

        return Json(new { success = true, message = "Kelime ba�ar�yla g�ncellendi." });
    }

    [HttpPost("DeleteUnknows")]
    public async Task<IActionResult> DeleteUnknows(int id)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Kullan�c� bilgisi bulunamad�." });
        }

        var result = await unknowsService.DeleteUnknowsAsync(id, userId);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Bilinmeyen kelime silinirken bir hata olu�tu." });
        }

        return Json(new { success = true, message = "Bilinmeyen kelime ba�ar�yla silindi." });
    }
}
