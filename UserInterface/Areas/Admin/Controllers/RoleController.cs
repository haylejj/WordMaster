using Core.Requests;
using Core.Service;
using Core.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserInterface.Extensions;

namespace UserInterface.Areas.Admin.Controllers;

[Authorize(Roles = "admin")]
[Area("Admin")]
public class RoleController : Controller
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public async Task<IActionResult> RoleList()
    {
        var roles = await _roleService.GetRoleListAsync();
        return View(roles);
    }
    public IActionResult RoleCreate()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> RoleCreate(RoleCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        var result = await _roleService.CreateRoleAsync(request);

        if (!result.IsSuccess)
        {
            ModelState.AddModelErrorList(result.Data!.Select(x => x.Description).ToList());
            return View();
        }
        TempData["SuccessMessage"] = "Yeni rol başarıyla oluşturuldu";
        return RedirectToAction(nameof(RoleController.RoleList));
    }

    public async Task<IActionResult> RoleUpdate(string roleId)
    {
        var role = await _roleService.FindByIdReturnRoleUpdateViewModelAsync(roleId);

        return !role.IsSuccess ? throw new Exception("Güncellenecek rol bulunamamıştır.") : (IActionResult)View(role.Data);
    }
    [HttpPost]
    public async Task<IActionResult> RoleUpdate(RoleUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        var result = await _roleService.UpdateRoleAsync(request);

        if (!result.IsSuccess)
        {
            ModelState.AddModelErrorList(result.Data!.Select(x => x.Description).ToList());
            return View();
        }
        else { TempData["SuccessMessage"] = "Güncelleme İşlemi Başarıyla Yapıldı"; }
        return RedirectToAction("RoleList", "Role");
    }
    public async Task<IActionResult> RoleDelete(string roleId)
    {
        var result = await _roleService.DeleteRoleAsync(roleId);
        if (!result.IsSuccess)
        {
            ModelState.AddModelErrorList(result.Data!.Select(x => x.Description).ToList());
            return View();
        }
        else { TempData["SuccessMessage"] = "Rol başarıyla silinmiştir"; }
        return RedirectToAction("RoleList", "Role");
    }

    public async Task<IActionResult> AssignToRole(string id)
    {
        var userRoles = await _roleService.GetRoleByIdReturnAssignToRoleAsync(id);
        ViewBag.Id = id;
        return View(userRoles);
    }
    [HttpPost]
    public async Task<IActionResult> AssignToRole(string id, List<AssignToRoleViewModel> requestList)
    {
        await _roleService.AssignRoleAsync(id, requestList);
        return RedirectToAction("UserList", "Admin");
    }
}
