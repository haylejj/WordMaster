using Core.Requests;
using Core.Service;
using Microsoft.AspNetCore.Mvc;
using UserInterface.Extensions;

namespace UserInterface.Controllers;

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

        TempData["SuccessMessage"] = "Kayıt olma işlemi başarıyla tamamlanmıştır";

        return View(nameof(Register));
    }
}
