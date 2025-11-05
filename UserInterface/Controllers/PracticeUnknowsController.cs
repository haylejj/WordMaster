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
        string newEnglishWord = await unknowsService.GetRandomWordFromUnknowsAsync(userId);
        return View((object)newEnglishWord);
    }

    public async Task<IActionResult> CheckTranslation(string turkishWord, string englishWord)
    {
        var userId = User.GetUserId();
        if (userId == null) return Json(new { isCorrect = false });
        bool isCorrect = await unknowsService.CheckTranslationAndUpdateAsync(userId, turkishWord, englishWord);
        return Json(new { isCorrect });
    }

    public async Task<IActionResult> GetNewEnglishWord()
    {
        var userId = User.GetUserId();
        if (userId == null) return Content(string.Empty);
        string newEnglishWord = await unknowsService.GetRandomWordFromUnknowsAsync(userId);
        return Content(newEnglishWord);
    }
}
