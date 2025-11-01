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
[Route("/Unknows")]
public class UnknowsController(IMapper mapper, IUnknowsService unknowsService, IWordService wordService, IFavoriteService favoriteService) : Controller
{
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(int page = 1)
    {
        var userId = User.GetUserId();
        var unknows = await unknowsService.Where(x => x.UserId == userId).Include(x => x.Word).ToListAsync();
        return View(unknows.ToPagedList(page, 5));
    }
    public async Task<IActionResult> AddUnknows(int id)
    {
        var userId = User.GetUserId();
        var word = await wordService.Where(x => x.Id == id && x.UserId == userId).FirstOrDefaultAsync();
        if (word != null)
        {
            // Aynı kelime zaten unknown'da mı kontrol et
            var existingUnknow = await unknowsService.Where(x => x.WordId == word.Id && x.UserId == userId).FirstOrDefaultAsync();
            if (existingUnknow == null)
            {
                var unknow = new Unknows
                {
                    WordId = word.Id,
                    UserId = userId,
                    CreatedTime = DateTime.Now
                };
                await unknowsService.AddAsync(unknow);
            }
        }
        return RedirectToAction("Index", "Word");
    }
    [HttpGet("UpdateUnknows")]
    public async Task<IActionResult> UpdateUnknows(int id)
    {
        var userId = User.GetUserId();
        var unknow = await unknowsService.Where(x => x.Id == id && x.UserId == userId).Include(x => x.Word).FirstOrDefaultAsync();
        return unknow == null ? NotFound() : View(unknow.Word);
    }
    [HttpPost]
    public async Task<IActionResult> UpdateUnknows(Word word)
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
        return RedirectToAction("Index", "Word");
    }
    public async Task<IActionResult> DeleteUnknows(int id)
    {
        var userId = User.GetUserId();
        var unknow = await unknowsService.Where(x => x.Id == id && x.UserId == userId).FirstOrDefaultAsync();
        if (unknow != null)
        {
            await unknowsService.RemoveAsync(unknow);
        }
        return RedirectToAction("Index");
    }
}
