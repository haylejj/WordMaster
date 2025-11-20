using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests.AllowedIpAddress;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.AllowedIpAddress;
using WordMaster.Domain.Results;

namespace WordMaster.WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admin")]
[Route("[area]/[controller]")]
public class AllowedIpAddressController(IAllowedIpAddressService allowedIpAddressService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        List<AllowedIpAddressViewModel> allowedIpAddresses = await allowedIpAddressService.GetAllAsync();
        AllowedIpAddressListViewModel viewModel = new()
        {
            AllowedIpAddresses = allowedIpAddresses
        };
        return View(viewModel);
    }

    [HttpGet("GetAllowedIpAddress")]
    public async Task<IActionResult> GetAllowedIpAddress(int id)
    {
        if (id <= 0)
        {
            return Json(new { success = false, message = "IP adresi ID gerekli." });
        }

        Result<AllowedIpAddressViewModel> result = await allowedIpAddressService.GetByIdAsync(id);

        if (!result.IsSuccess || result.Data == null)
        {
            return Json(new { success = false, message = "IP adresi bulunamadı." });
        }

        AllowedIpAddressViewModel allowedIpAddress = result.Data;

        return Json(new
        {
            success = true,
            allowedIpAddress = new
            {
                id = allowedIpAddress.Id,
                ipAddress = allowedIpAddress.IpAddress,
                description = allowedIpAddress.Description,
                isActive = allowedIpAddress.IsActive
            }
        });
    }

    [HttpPost("UpdateAllowedIpAddress")]
    public async Task<IActionResult> UpdateAllowedIpAddress([FromBody] AllowedIpAddressUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            List<string> errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, message = string.Join(", ", errors) });
        }

        Result<IEnumerable<string>> result = await allowedIpAddressService.UpdateAsync(request);

        if (!result.IsSuccess)
        {
            string errorMessage = result.ErrorMessage ?? "IP adresi güncellenirken bir hata oluştu.";
            if (result.Data != null && result.Data.Any())
            {
                errorMessage = string.Join(", ", result.Data);
            }
            return Json(new { success = false, message = errorMessage });
        }

        return Json(new { success = true, message = "IP adresi başarıyla güncellendi." });
    }

    [HttpPost("DeleteAllowedIpAddress")]
    public async Task<IActionResult> DeleteAllowedIpAddress(int id)
    {
        Result<IEnumerable<string>> result = await allowedIpAddressService.DeleteAsync(id);

        if (!result.IsSuccess)
        {
            string errorMessage = result.ErrorMessage ?? "IP adresi silinirken bir hata oluştu.";
            if (result.Data != null && result.Data.Any())
            {
                errorMessage = string.Join(", ", result.Data);
            }
            return Json(new { success = false, message = errorMessage });
        }

        return Json(new { success = true, message = "IP adresi başarıyla silindi." });
    }

    [HttpPost("CreateAllowedIpAddress")]
    public async Task<IActionResult> CreateAllowedIpAddress([FromBody] AllowedIpAddressCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            List<string> errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, message = string.Join(", ", errors) });
        }

        Result<IEnumerable<string>> result = await allowedIpAddressService.CreateAsync(request);

        if (!result.IsSuccess)
        {
            string errorMessage = result.ErrorMessage ?? "IP adresi oluşturulurken bir hata oluştu.";
            if (result.Data != null && result.Data.Any())
            {
                errorMessage = string.Join(", ", result.Data);
            }
            return Json(new { success = false, message = errorMessage });
        }

        return Json(new { success = true, message = "IP adresi başarıyla oluşturuldu." });
    }
}

