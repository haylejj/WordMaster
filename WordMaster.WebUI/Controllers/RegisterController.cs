using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.Auth;
using WordMaster.Domain.Results;
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
    public async Task<IActionResult> Register(RegisterViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }
        RegisterRequest request = new()
        {
            UserName = viewModel.UserName,
            Email = viewModel.Email,
            Password = viewModel.Password,
            PasswordConfirm = viewModel.PasswordConfirm,
            Phone = viewModel.Phone
        };
        Result<IEnumerable<IdentityError>> result = await registerService.RegisterAsync(request);

        if (!result.IsSuccess)
        {
            ModelState.AddModelErrorList(result.Data!.Select(x => x.Description).ToList());
            return View();
        }

        TempData["SuccessMessage"] = "Kay�t olma i�lemi ba�ar�yla tamamlanm��t�r";

        return View(nameof(Register));
    }
}
