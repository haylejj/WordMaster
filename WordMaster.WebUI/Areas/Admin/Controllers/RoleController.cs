using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests.Role;
using WordMaster.Application.Responses.Role;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin")]
[Route("[area]/[controller]")]
public class RoleController(IRoleService roleService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ServiceResult<List<RoleResponse>> result = await roleService.GetRoleListAsync();
        RoleListResponse viewModel = new()
        {
            Roles = result.Data ?? []
        };
        return View(viewModel);
    }

    [HttpGet("GetRole")]
    public async Task<IActionResult> GetRole(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Json(new { success = false, message = "Rol ID gerekli." });
        }

        ServiceResult<RoleUpdateResponse> result = await roleService.FindByIdReturnRoleUpdateViewModelAsync(id);

        if (!result.IsSuccess || result.Data == null)
        {
            return Json(new { success = false, message = "Rol bulunamadı." });
        }

        RoleUpdateResponse role = result.Data;

        return Json(new
        {
            success = true,
            role = new
            {
                id = role.Id,
                name = role.Name
            }
        });
    }

    [HttpPost("UpdateRole")]
    public async Task<IActionResult> UpdateRole([FromBody] RoleUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            List<string> errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, message = string.Join(", ", errors) });
        }

        ServiceResult result = await roleService.UpdateRoleAsync(request);

        if (!result.IsSuccess)
        {
            string errorMessage = result.ErrorMessage() ?? "Rol güncellenirken bir hata oluştu.";
            return Json(new { success = false, message = errorMessage });
        }

        return Json(new { success = true, message = "Rol başarıyla güncellendi." });
    }

    [HttpPost("DeleteRole")]
    public async Task<IActionResult> DeleteRole(string id)
    {
        ServiceResult result = await roleService.DeleteRoleAsync(id);

        if (!result.IsSuccess)
        {
            string errorMessage = result.ErrorMessage() ?? "Rol silinirken bir hata oluştu.";
            return Json(new { success = false, message = errorMessage });
        }

        return Json(new { success = true, message = "Rol başarıyla silindi." });
    }

    [HttpPost("CreateRole")]
    public async Task<IActionResult> CreateRole([FromBody] RoleCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            List<string> errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, message = string.Join(", ", errors) });
        }

        ServiceResult result = await roleService.CreateRoleAsync(request);

        if (!result.IsSuccess)
        {
            string errorMessage = result.ErrorMessage() ?? "Rol oluşturulurken bir hata oluştu.";
            return Json(new { success = false, message = errorMessage });
        }

        return Json(new { success = true, message = "Rol başarıyla oluşturuldu." });
    }
}
