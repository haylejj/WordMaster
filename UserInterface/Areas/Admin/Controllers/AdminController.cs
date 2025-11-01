using Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UserInterface.Areas.Admin.Controllers;

[Authorize(Roles = "admin")]
[Area("Admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService=adminService;
    }

    public async Task<IActionResult> UserList()
    {
        var users = await _adminService.GetUsersAsync();
        return View(users);
    }
}
