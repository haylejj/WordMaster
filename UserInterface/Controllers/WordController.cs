using AutoMapper;
using Core.Dto;
using Core.Entity;
using Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserInterface.Extensions;
using X.PagedList;

namespace UserInterface.Controllers;

[Authorize]
public class WordController : Controller
{
    private readonly IWordService _wordService;
    private readonly IMapper _mapper;
    private readonly IUnknowsService _unknowsService;
    private readonly IFavoriteService _favoriteService;

    public WordController(IWordService wordService, IMapper mapper, IFavoriteService favoriteService, IUnknowsService unknowsService)
    {
        _wordService = wordService;
        _mapper = mapper;
        _favoriteService = favoriteService;
        _unknowsService = unknowsService;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var userId = User.GetUserId();
        var words = await _wordService.Where(x => x.UserId == userId).ToListAsync();
        return View(words.ToPagedList(page, 10));
    }
    [HttpGet]
    public IActionResult AddWord()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> AddWord(WordDto wordDto)
    {
        var userId = User.GetUserId();
        var word = _mapper.Map<Word>(wordDto);
        word.UserId = userId;
        word.CreatedTime = DateTime.Now;
        await _wordService.AddAsync(word);
        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    public async Task<IActionResult> DeleteWord(int id)
    {
        var userId = User.GetUserId();
        var word = await _wordService.Where(x => x.Id == id && x.UserId == userId).FirstOrDefaultAsync();
        if (word != null)
        {
            await _wordService.RemoveAsync(word);
        }
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> UpdateWord(int id)
    {
        var userId = User.GetUserId();
        var word = await _wordService.Where(x => x.Id == id && x.UserId == userId).FirstOrDefaultAsync();
        return word == null ? NotFound() : View(word);
    }
    [HttpPost]
    public async Task<IActionResult> UpdateWord(Word word)
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
        return RedirectToAction(nameof(Index));
    }

}
