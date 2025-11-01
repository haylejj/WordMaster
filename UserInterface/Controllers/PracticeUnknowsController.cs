using Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserInterface.Extensions;

namespace UserInterface.Controllers;

[Authorize]
[Route("/PracticeUnknows")]
public class PracticeUnknowsController : Controller
{
    private readonly IUnknowsService _unknowsService;
    private readonly IWordService _wordService;
    private static Random random = new();

    public PracticeUnknowsController(IUnknowsService unknowsService, IWordService wordService)
    {
        _unknowsService = unknowsService;
        _wordService = wordService;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        string newEnglishWord = await getNewWord();
        return View((object)newEnglishWord);
    }

    private bool IsCorrect(string turkish, string english)
    {
        var userId = User.GetUserId();
        var unknow = _unknowsService.Where(x => x.Word.EnglishWord == english && x.UserId == userId).Include(x => x.Word).FirstOrDefault();
        return unknow != null && unknow.Word?.TurkishWord?.ToLower().Trim() == turkish?.ToLower().Trim();
    }
    private async Task<string> getNewWord()
    {
        var userId = User.GetUserId();
        var unknows = await _unknowsService.Where(x => x.UserId == userId).Include(x => x.Word).ToListAsync();
        if (unknows == null || unknows.Count == 0)
        {
            return string.Empty;
        }

        int index = random.Next(0, unknows.Count);
        return unknows[index].Word?.EnglishWord ?? string.Empty;
    }
    public async Task<IActionResult> CheckTranslation(string turkishWord, string englishWord)
    {
        var userId = User.GetUserId();
        var unknow = await _unknowsService.Where(x => x.Word.EnglishWord == englishWord && x.UserId == userId)
            .Include(x => x.Word)
            .FirstOrDefaultAsync();

        if (unknow?.Word == null)
        {
            return Json(new { isCorrect = false });
        }

        var word = unknow.Word;
        bool isCorrect = word.TurkishWord?.ToLower().Trim() == turkishWord?.ToLower().Trim();

        // Öğrenme takibini güncelle
        word.IsLastAnswerCorrect = isCorrect;
        word.LastPracticeDate = DateTime.Now;

        if (isCorrect)
        {
            word.ConsecutiveCorrectCount++;
            word.ConsecutiveWrongCount = 0;
            word.TotalCorrectCount++;
        }
        else
        {
            word.ConsecutiveWrongCount++;
            word.ConsecutiveCorrectCount = 0;
            word.TotalWrongCount++;
        }

        await _wordService.UpdateAsync(word);

        return Json(new { isCorrect });
    }
    public async Task<IActionResult> GetNewEnglishWord()
    {
        string newEnglishWord = await getNewWord();
        return Content(newEnglishWord);
    }
}
