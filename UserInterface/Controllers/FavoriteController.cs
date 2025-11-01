using AutoMapper;
using Core.Entity;
using Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserInterface.Extensions;
using X.PagedList;

namespace UserInterface.Controllers;

[Authorize]
[Route("/Favorite")]
public class FavoriteController(IMapper mapper, IFavoriteService favoriteService, IWordService wordService, IUnknowsService unknowsService) : Controller
{
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(int page = 1)
    {
        var userId = User.GetUserId();
        var favorities = await favoriteService.Where(x => x.UserId == userId).Include(x => x.Word).ToListAsync();
        return View(favorities.ToPagedList(page, 5));
    }
    [HttpGet("AddFavorite")]
    public async Task<IActionResult> AddFavorite(int id)
    {
        var userId = User.GetUserId();
        var word = await wordService.Where(x => x.Id == id && x.UserId == userId).FirstOrDefaultAsync();
        if (word != null)
        {
            // Aynı kelime zaten favorite'da mı kontrol et
            var existingFavorite = await favoriteService.Where(x => x.WordId == word.Id && x.UserId == userId).FirstOrDefaultAsync();
            if (existingFavorite == null)
            {
                var favorite = new Favorite
                {
                    WordId = word.Id,
                    UserId = userId,
                    CreatedTime = DateTime.Now
                };
                await favoriteService.AddAsync(favorite);
            }
        }

        return RedirectToAction("Index", "Word");
    }
    [HttpGet("UpdateFavorite")]
    public async Task<IActionResult> UpdateFavorite(int id)
    {
        var userId = User.GetUserId();
        var favorite = await favoriteService.Where(x => x.Id == id && x.UserId == userId).Include(x => x.Word).FirstOrDefaultAsync();
        return favorite == null ? NotFound() : View(favorite.Word);
    }
    [HttpPost]
    public async Task<IActionResult> UpdateFavorite(Word word)
    {
        var userId = User.GetUserId();
        var existingWord = await wordService.Where(x => x.Id == word.Id && x.UserId == userId).FirstOrDefaultAsync();
        if (existingWord == null)
        {
            return NotFound();
        }

        existingWord.EnglishWord = word.EnglishWord;
        existingWord.TurkishWord = word.TurkishWord;

        await wordService.UpdateAsync(existingWord);
        return RedirectToAction("Index");
    }
    public async Task<IActionResult> DeleteFavorite(int id)
    {
        var userId = User.GetUserId();
        var favorite = await favoriteService.Where(x => x.Id == id && x.UserId == userId).FirstOrDefaultAsync();
        if (favorite != null)
        {
            await favoriteService.RemoveAsync(favorite);
        }
        return RedirectToAction("Index");
    }
}
