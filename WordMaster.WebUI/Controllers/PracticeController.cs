using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.Practice;
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
        string? userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        Result<string> word = await wordService.GetRandomWordAsync(userId);
        var viewModel = new PracticeViewModel
        {
            EnglishWord = word.IsSuccess && word.Data != null ? word.Data : string.Empty,
            ErrorMessage = word.IsSuccess ? null : word.ErrorMessage
        };

        return View(viewModel);
    }

    [HttpGet("CheckTranslation")]
    public async Task<IActionResult> CheckTranslation(string turkishWord, string englishWord)
    {
        string? userId = User.GetUserId();
        if (userId == null) return Json(new { isCorrect = false });
        Result<bool> result = await wordService.CheckTranslationAndUpdateAsync(userId, turkishWord, englishWord);
        return Json(new { isCorrect = result.Data });
    }

    [HttpGet("GetNewEnglishWord")]
    public async Task<IActionResult> GetNewEnglishWord()
    {
        string? userId = User.GetUserId();
        if (userId == null) return Content(string.Empty);
        Result<string> word = await wordService.GetRandomWordAsync(userId);
        return Content(word.IsSuccess && word.Data != null ? word.Data : string.Empty);
    }
}
