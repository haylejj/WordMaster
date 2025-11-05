using Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserInterface.Extensions;

namespace UserInterface.Controllers;

[Authorize]
[Route("/PracticeFavorite")]
public class PracticeFavoriteController(IFavoriteService favoriteService, IWordService wordService) : Controller
{
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        string newEnglishWord = await favoriteService.GetRandomWordFromFavoritesAsync(userId);
        return View((object)newEnglishWord);
    }

    public async Task<IActionResult> CheckTranslation(string turkishWord, string englishWord)
    {
        var userId = User.GetUserId();
        if (userId == null) return Json(new { isCorrect = false });
        bool isCorrect = await favoriteService.CheckTranslationAndUpdateAsync(userId, turkishWord, englishWord, wordService);
        return Json(new { isCorrect });
    }

    public async Task<IActionResult> GetNewEnglishWord()
    {
        var userId = User.GetUserId();
        if (userId == null) return Content(string.Empty);
        string newEnglishWord = await favoriteService.GetRandomWordFromFavoritesAsync(userId);
        return Content(newEnglishWord);
    }
}
