using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Areas.Admin.Controllers;

[Authorize(Roles = "admin")]
[Area("Admin")]
public class RoleController(IRoleService roleService) : Controller
{
    public async Task<IActionResult> RoleList()
    {
        var roles = await roleService.GetRoleListAsync();
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

        var result = await roleService.CreateRoleAsync(request);

        if (!result.IsSuccess)
        {
            ModelState.AddModelErrorList(result.Data!.Select(x => x.Description).ToList());
            return View();
        }
        TempData["SuccessMessage"] = "Yeni rol baþarýyla oluþturuldu";
        return RedirectToAction(nameof(RoleList));
    }

    public async Task<IActionResult> RoleUpdate(string roleId)
    {
        var role = await roleService.FindByIdReturnRoleUpdateViewModelAsync(roleId);

        return !role.IsSuccess ? throw new Exception("Güncellenecek rol bulunamamýþtýr.") : (IActionResult)View(role.Data);
    }
    [HttpPost]
    public async Task<IActionResult> RoleUpdate(RoleUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        var result = await roleService.UpdateRoleAsync(request);

        if (!result.IsSuccess)
        {
            ModelState.AddModelErrorList(result.Data!.Select(x => x.Description).ToList());
            return View();
        }
        else { TempData["SuccessMessage"] = "Güncelleme Ýþlemi Baþarýyla Yapýldý"; }
        return RedirectToAction("RoleList", "Role");
    }
    public async Task<IActionResult> RoleDelete(string roleId)
    {
        var result = await roleService.DeleteRoleAsync(roleId);
        if (!result.IsSuccess)
        {
            ModelState.AddModelErrorList(result.Data!.Select(x => x.Description).ToList());
            return View();
        }
        else { TempData["SuccessMessage"] = "Rol baþarýyla silinmiþtir"; }
        return RedirectToAction("RoleList", "Role");
    }

    public async Task<IActionResult> AssignToRole(string id)
    {
        var userRoles = await roleService.GetRoleByIdReturnAssignToRoleAsync(id);
        ViewBag.Id = id;
        return View(userRoles);
    }
    [HttpPost]
    public async Task<IActionResult> AssignToRole(string id, List<AssignToRoleViewModel> requestList)
    {
        await roleService.AssignRoleAsync(id, requestList);
        return RedirectToAction("UserList", "Admin");
    }
}
