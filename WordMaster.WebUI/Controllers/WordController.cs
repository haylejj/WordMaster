using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.Responses.Word;
using WordMaster.Application.Requests.Word;
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
        Guid userId = User.GetUserId();
        ServiceResult<PagedResult<WordResponse>> pageResult = await wordService.GetPagedWordsAsync(userId, search, page, pageSize);

        List<WordResponse> words = pageResult.IsSuccess && pageResult.Data != null ? pageResult.Data.Items : [];
        int totalCount = pageResult.IsSuccess && pageResult.Data != null ? pageResult.Data.TotalCount : 0;

        WordListResponse viewModel = new()
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
    public async Task<IActionResult> AddWord(CreateWordRequest request)
    {
        if (!ModelState.IsValid)
        {
            // View expects WordResponse to show errors or previous values, but we have CreateWordRequest.
            // We should ideally map back or use a different View model. 
            // For now, let's create a dummy response to pass back to view if we want to preserve input.
            // Or better, let the view accept CreateWordRequest? 
            // The view AddWord.cshtml currently has @model WordMaster.Application.Responses.Word.WordResponse
            // This is a bit of a mismatch if we switch controller input to Request.
            // Ideally, Add/Edit views should probably use the Request object as model or a specific ViewModel.
            // However, to satisfy "use Request in controller method", we do this.
            // I'll construct a WordResponse from request to return to View.
            WordResponse response = new()
            {
                EnglishWord = request.EnglishWord,
                TurkishWord = request.TurkishWord
            };
            return View(response);
        }

        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            ModelState.AddModelError("", "Kullanıcı bilgisi bulunamadı.");
            WordResponse response = new()
            {
                EnglishWord = request.EnglishWord,
                TurkishWord = request.TurkishWord
            };
            return View(response);
        }

        ServiceResult result = await wordService.AddWordAsync(request, userId);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("EnglishWord", result.ErrorList?.FirstOrDefault() ?? "Kelime eklenirken bir hata oluştu.");
            WordResponse response = new()
            {
                EnglishWord = request.EnglishWord,
                TurkishWord = request.TurkishWord
            };
            return View(response);
        }

        TempData["SuccessMessage"] = "Kelime başarıyla eklendi!";
        return RedirectToAction(nameof(AddWord));
    }
    [HttpPost("DeleteWord")]
    public async Task<IActionResult> DeleteWord(long id)
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Json(new { success = false, message = "Kullanıcı bilgisi bulunamadı." });
        }

        ServiceResult result = await wordService.DeleteWordAsync(id, userId);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorList?.FirstOrDefault() ?? "Kelime silinirken bir hata oluştu." });
        }

        return Json(new { success = true, message = "Kelime başarıyla silindi." });
    }

    [HttpGet("GetWord")]
    public async Task<IActionResult> GetWord(long id)
    {
        Guid userId = User.GetUserId();
        ServiceResult<WordResponse> result = await wordService.GetWordForUserAsync(id, userId);

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
    public async Task<IActionResult> UpdateWord(UpdateWordRequest request, long id)
    {
        // Note: id is usually passed in URL or form. If it's in form, we might need it in Request or as separate param.
        // UpdateWordRequest usually doesn't have ID if it's strictly for body, but let's check definition. 
        // Assuming ID comes from route or form field named 'Id' which binds to 'id' param.

        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Geçersiz veri." });
        }

        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Json(new { success = false, message = "Kullanıcı bilgisi bulunamadı." });
        }

        ServiceResult result = await wordService.UpdateWordAsync(id, request, userId);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorList?.FirstOrDefault() ?? "Kelime güncellenirken bir hata oluştu." });
        }

        return Json(new { success = true, message = "Kelime başarıyla güncellendi." });
    }

    [HttpPost("ImportFromExcel")]
    public async Task<IActionResult> ImportFromExcel(IFormFile file)
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
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

        using Stream stream = file.OpenReadStream();
        ServiceResult result = await excelService.ImportWordsAsync(stream, userId);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.ErrorList?.FirstOrDefault() ?? "İçe aktarma sırasında bir hata oluştu." });
        }

        return Json(new { success = true, message = "Kelimeler başarıyla içe aktarıldı." });
    }

}
