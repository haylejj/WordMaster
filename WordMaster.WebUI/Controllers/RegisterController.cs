using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests;
using WordMaster.Application.Services.Abstract;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Controllers;

[Route("/Register")]
public class RegisterController(IRegisterService registerService) : Controller
{
    [HttpGet("")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost("")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }
        var result = await registerService.RegisterAsync(request);

        if (!result.IsSuccess)
        {
            ModelState.AddModelErrorList(result.Data!.Select(x => x.Description).ToList());
            return View();
        }

        TempData["SuccessMessage"] = "Kayýt olma iþlemi baþarýyla tamamlanmýþtýr";

        return View(nameof(Register));
    }
}
