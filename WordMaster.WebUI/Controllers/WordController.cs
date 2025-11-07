using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Dto;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels;
using WordMaster.Domain.Entities;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Controllers;

[Authorize]
[Route("/Word")]
public class WordController(IWordService wordService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
    {
        var userId = User.GetUserId();
        var pageResult = await wordService.GetPagedWordsAsync(userId!, search, page, pageSize);
        var (words, totalCount) = pageResult.IsSuccess ? pageResult.Data : (new List<Word>(), 0);

        var viewModel = new WordListViewModel
        {
            Words = words,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Search = search
        };

        return View(viewModel);
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
            ModelState.AddModelError("", "Kullan�c� bilgisi bulunamad�.");
            return View(wordDto);
        }

        var result = await wordService.AddWordAsync(wordDto, userId);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("EnglishWord", result.ErrorMessage ?? "Kelime eklenirken bir hata olu�tu.");
            return View(wordDto);
        }

        TempData["SuccessMessage"] = "Kelime ba�ar�yla eklendi!";
        return RedirectToAction(nameof(AddWord));
    }
    [HttpPost("DeleteWord")]
    public async Task<IActionResult> DeleteWord(int id)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Kullan�c� bilgisi bulunamad�." });
        }

        var result = await wordService.DeleteWordAsync(id, userId);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Kelime silinirken bir hata olu�tu." });
        }

        return Json(new { success = true, message = "Kelime ba�ar�yla silindi." });
    }

    [HttpGet("GetWord")]
    public async Task<IActionResult> GetWord(int id)
    {
        var userId = User.GetUserId();
        var result = await wordService.GetWordForUserAsync(id, userId!);

        if (!result.IsSuccess || result.Data == null)
        {
            return Json(new { success = false, message = "Kelime bulunamad�." });
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

        var result = await wordService.UpdateWordAsync(wordDto.Id, wordDto, userId);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Kelime g�ncellenirken bir hata olu�tu." });
        }

        return Json(new { success = true, message = "Kelime ba�ar�yla g�ncellendi." });
    }

}
