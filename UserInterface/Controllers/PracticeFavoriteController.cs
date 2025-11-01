using Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Core.Entity;
using UserInterface.Extensions;

namespace UserInterface.Controllers
{
    [Authorize]
    public class PracticeFavoriteController : Controller
    {
        private readonly IFavoriteService _favoriteService;
        private readonly IWordService _wordService;
        private static Random random = new Random();

        public PracticeFavoriteController(IFavoriteService favoriteService, IWordService wordService)
        {
            _favoriteService = favoriteService;
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
            var favorite = _favoriteService.Where(x => x.Word.EnglishWord == english && x.UserId == userId).Include(x => x.Word).FirstOrDefault();
            return favorite != null && favorite.Word?.TurkishWord?.ToLower().Trim() == turkish?.ToLower().Trim();
        }
        private async Task<string> getNewWord()
        {
            var userId = User.GetUserId();
            var favorites = await _favoriteService.Where(x => x.UserId == userId).Include(x => x.Word).ToListAsync();
            if (favorites == null || favorites.Count == 0)
            {
                return string.Empty;
            }

            int index = random.Next(0, favorites.Count);
            return favorites[index].Word?.EnglishWord ?? string.Empty;
        }
        public async Task<IActionResult> CheckTranslation(string turkishWord, string englishWord)
        {
            var userId = User.GetUserId();
            var favorite = await _favoriteService.Where(x => x.Word.EnglishWord == englishWord && x.UserId == userId)
                .Include(x => x.Word)
                .FirstOrDefaultAsync();

            if (favorite?.Word == null)
            {
                return Json(new { isCorrect = false });
            }

            var word = favorite.Word;
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
}
