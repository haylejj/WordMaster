using AutoMapper;
using Core.Entity;
using Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserInterface.Extensions;
using X.PagedList;

namespace UserInterface.Controllers
{
    [Authorize]
    public class UnknowsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUnknowsService _unknowsService;
        private readonly IWordService _wordService;
        private readonly IFavoriteService _favoriteService;

        public UnknowsController(IMapper mapper, IUnknowsService unknowsService, IWordService wordService, IFavoriteService favoriteService)
        {
            _mapper = mapper;
            _unknowsService = unknowsService;
            _wordService = wordService;
            _favoriteService = favoriteService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var userId = User.GetUserId();
            var unknows = await _unknowsService.Where(x => x.UserId == userId).Include(x => x.Word).ToListAsync();
            return View(unknows.ToPagedList(page, 5));
        }
        public async Task<IActionResult> AddUnknows(int id)
        {
            var userId = User.GetUserId();
            var word = await _wordService.Where(x => x.Id == id && x.UserId == userId).FirstOrDefaultAsync();
            if (word != null)
            {
                // Aynı kelime zaten unknown'da mı kontrol et
                var existingUnknow = await _unknowsService.Where(x => x.WordId == word.Id && x.UserId == userId).FirstOrDefaultAsync();
                if (existingUnknow == null)
                {
                    var unknow = new Unknows
                    {
                        WordId = word.Id,
                        UserId = userId,
                        CreatedTime = DateTime.Now
                    };
                    await _unknowsService.AddAsync(unknow);
                }
            }
            return RedirectToAction("Index", "Word");
        }
        [HttpGet]
        public async Task<IActionResult> UpdateUnknows(int id)
        {
            var userId = User.GetUserId();
            var unknow = await _unknowsService.Where(x => x.Id == id && x.UserId == userId).Include(x => x.Word).FirstOrDefaultAsync();
            if (unknow == null)
            {
                return NotFound();
            }
            return View(unknow.Word);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateUnknows(Word word)
        {
            var userId = User.GetUserId();
            var existingWord = await _wordService.Where(x => x.Id == word.Id && x.UserId == userId).FirstOrDefaultAsync();
            if (existingWord == null)
            {
                return NotFound();
            }

            existingWord.EnglishWord = word.EnglishWord;
            existingWord.TurkishWord = word.TurkishWord;

            await _wordService.UpdateAsync(existingWord);
            return RedirectToAction("Index", "Word");
        }
        public async Task<IActionResult> DeleteUnknows(int id)
        {
            var userId = User.GetUserId();
            var unknow = await _unknowsService.Where(x => x.Id == id && x.UserId == userId).FirstOrDefaultAsync();
            if (unknow != null)
            {
                await _unknowsService.RemoveAsync(unknow);
            }
            return RedirectToAction("Index");
        }
    }
}
