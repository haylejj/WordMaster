using Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserInterface.Extensions;

namespace UserInterface.Controllers;

[Authorize]
[Route("/PracticeUnknows")]
public class PracticeUnknowsController(IUnknowsService unknowsService) : Controller
{
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        var word = await unknowsService.GetRandomWordFromUnknowsAsync(userId);
        return View((object)(word.IsSuccess ? word.Data : string.Empty));
    }

    [HttpGet("CheckTranslation")]
    public async Task<IActionResult> CheckTranslation(string turkishWord, string englishWord)
    {
        var userId = User.GetUserId();
        if (userId == null) return Json(new { isCorrect = false });
        var result = await unknowsService.CheckTranslationAndUpdateAsync(userId, turkishWord, englishWord);
        return Json(new { isCorrect = result.Data });
    }

    [HttpGet("GetNewEnglishWord")]
    public async Task<IActionResult> GetNewEnglishWord()
    {
        var userId = User.GetUserId();
        if (userId == null) return Content(string.Empty);
        var word = await unknowsService.GetRandomWordFromUnknowsAsync(userId);
        return Content(word.IsSuccess ? word.Data : string.Empty);
    }
}
