using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.Responses.Practice;
using WordMaster.Domain.Results;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Controllers;

[Authorize]
[Route("/Practice")]
public class PracticeController(IWordService wordService) : Controller
{
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        ServiceResult<string> word = await wordService.GetRandomWordAsync(userId);
        PracticeResponse viewModel = new()
        {
            EnglishWord = word.IsSuccess && word.Data != null ? word.Data : string.Empty,
            ErrorMessage = word.IsSuccess ? null : word.ErrorMessage()
        };

        return View(viewModel);
    }

    [HttpGet("CheckTranslation")]
    public async Task<IActionResult> CheckTranslation(string turkishWord, string englishWord)
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Json(new { isCorrect = false });
        }

        ServiceResult<bool> result = await wordService.CheckTranslationAndUpdateAsync(userId, new CheckTranslationRequest { EnglishWord = englishWord, TurkishWord = turkishWord });
        return Json(new { isCorrect = result.Data });
    }

    [HttpGet("GetNewEnglishWord")]
    public async Task<IActionResult> GetNewEnglishWord()
    {
        Guid userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Content(string.Empty);
        }

        ServiceResult<string> word = await wordService.GetRandomWordAsync(userId);
        return Content(word.IsSuccess && word.Data != null ? word.Data : string.Empty);
    }
}
