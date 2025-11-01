using Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserInterface.Extensions;

namespace UserInterface.Controllers;

[Authorize]
public class PracticeController : Controller
{
    private readonly IWordService _wordService;
    private static Random random = new();

    public PracticeController(IWordService wordService)
    {
        _wordService = wordService;
    }

    public async Task<IActionResult> Index()
    {
        string newEnglishWord = await getNewWord();
        return View((object)newEnglishWord);
    }

    private bool IsCorrect(string turkish, string english)
    {
        var userId = User.GetUserId();
        var word = _wordService.Where(x => x.EnglishWord == english && x.UserId == userId).FirstOrDefault();
        return word != null && word.TurkishWord == turkish;
    }

    private async Task<string> getNewWord()
    {
        var userId = User.GetUserId();
        var words = await _wordService.Where(x => x.UserId == userId).ToListAsync();
        if (words == null || words.Count == 0)
        {
            return string.Empty;
        }

        int index = random.Next(0, words.Count);
        return words[index].EnglishWord ?? string.Empty;
    }

    public async Task<IActionResult> CheckTranslation(string turkishWord, string englishWord)
    {
        var userId = User.GetUserId();
        var word = await _wordService.Where(x => x.EnglishWord == englishWord && x.UserId == userId).FirstOrDefaultAsync();

        if (word == null)
        {
            return Json(new { isCorrect = false });
        }

        bool isCorrect = word.TurkishWord?.ToLower().Trim() == turkishWord?.ToLower().Trim();

        // Öğrenme takibini güncelle
        word.IsLastAnswerCorrect = isCorrect;
        word.LastPracticeDate = DateTime.Now;

        if (isCorrect)
        {
            word.ConsecutiveCorrectCount++;
            word.ConsecutiveWrongCount = 0; // Ard arda doğru bildiyse yanlış sayacını sıfırla
            word.TotalCorrectCount++;
        }
        else
        {
            word.ConsecutiveWrongCount++;
            word.ConsecutiveCorrectCount = 0; // Yanlış bildiyse doğru sayacını sıfırla
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
