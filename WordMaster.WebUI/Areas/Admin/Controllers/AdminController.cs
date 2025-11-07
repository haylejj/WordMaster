using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Services.Abstract;

namespace WordMaster.WebUI.Areas.Admin.Controllers;

[Authorize(Roles = "admin")]
[Area("Admin")]
public class AdminController(IUserService userService) : Controller
{
    public async Task<IActionResult> UserList()
    {
        var users = await userService.GetUsersAsync();
        return View(users);
    }
}
