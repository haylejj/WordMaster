using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Dto.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.Word;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Controllers;

[Authorize]
[Route("/Word")]
public class WordController(IWordService wordService, IExcelService excelService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
    {
        string? userId = User.GetUserId();
        Result<(List<Word> Words, int TotalCount)> pageResult = await wordService.GetPagedWordsAsync(userId!, search, page, pageSize);
        (List<Word>? words, int totalCount) = pageResult.IsSuccess ? pageResult.Data : (new List<Word>(), 0);

        WordListViewModel viewModel = new()
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
    public async Task<IActionResult> AddWord(WordViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        string? userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            ModelState.AddModelError("", "Kullanıcı bilgisi bulunamadı.");
            return View(viewModel);
        }

        WordDto wordDto = new()
        {
            Id = viewModel.Id,
            EnglishWord = viewModel.EnglishWord,
            TurkishWord = viewModel.TurkishWord
        };

        Result result = await wordService.AddWordAsync(wordDto, userId);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("EnglishWord", result.ErrorMessage ?? "Kelime eklenirken bir hata oluştu.");
            return View(viewModel);
        }

        TempData["SuccessMessage"] = "Kelime başarıyla eklendi!";
        return RedirectToAction(nameof(AddWord));
    }
    [HttpPost("DeleteWord")]
    public async Task<IActionResult> DeleteWord(int id)
    {
        string? userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Kullanıcı bilgisi bulunamadı." });
        }

        Result result = await wordService.DeleteWordAsync(id, userId);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Kelime silinirken bir hata oluştu." });
        }

        return Json(new { success = true, message = "Kelime başarıyla silindi." });
    }

    [HttpGet("GetWord")]
    public async Task<IActionResult> GetWord(int id)
    {
        string? userId = User.GetUserId();
        Result<Word> result = await wordService.GetWordForUserAsync(id, userId!);

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

        string? userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Kullanıcı bilgisi bulunamadı." });
        }

        WordDto wordDto = new()
        {
            Id = viewModel.Id,
            EnglishWord = viewModel.EnglishWord,
            TurkishWord = viewModel.TurkishWord
        };

        Result result = await wordService.UpdateWordAsync(wordDto.Id, wordDto, userId);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Kelime güncellenirken bir hata oluştu." });
        }

        return Json(new { success = true, message = "Kelime başarıyla güncellendi." });
    }

    [HttpPost("ImportFromExcel")]
    public async Task<IActionResult> ImportFromExcel(IFormFile file)
    {
        string? userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Json(new { success = false, message = "Kullanıcı bilgisi bulunamadı." });
        }

        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Lütfen bir dosya seçin." });
        }

        if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return Json(new { success = false, message = "Sadece .csv dosyaları kabul edilir." });
        }

        using var stream = file.OpenReadStream();
        Result result = await excelService.ImportWordsAsync(stream, userId);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "İçe aktarma sırasında bir hata oluştu." });
        }

        return Json(new { success = true, message = "Kelimeler başarıyla içe aktarıldı." });
    }

}
