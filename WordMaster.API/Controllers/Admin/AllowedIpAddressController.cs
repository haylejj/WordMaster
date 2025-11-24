using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.API.Extensions;
using WordMaster.Application.Requests.AllowedIpAddress;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.AllowedIpAddress;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers.Admin;

/// <summary>
/// İzin verilen IP adreslerini yöneten admin controller.
/// </summary>
[Authorize(Roles = "admin")]
[Route("api/admin/allowed-ips")]
public class AllowedIpAddressController(IAllowedIpAddressService allowedIpAddressService) : BaseController
{
    /// <summary>
    /// Tüm izin verilen IP adreslerini listeler.
    /// </summary>
    /// <returns>IP adresleri listesi.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        ServiceResult<List<AllowedIpAddressViewModel>> result = await allowedIpAddressService.GetAllAsync();
        return CreateResult(result);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip IP adresini getirir.
    /// </summary>
    /// <param name="id">IP Adresi ID'si.</param>
    /// <returns>IP adresi detayları.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        ServiceResult<AllowedIpAddressViewModel> result = await allowedIpAddressService.GetByIdAsync(id);
        return CreateResult(result);
    }

    /// <summary>
    /// Yeni bir izin verilen IP adresi ekler.
    /// </summary>
    /// <param name="request">Eklenecek IP adresi bilgileri.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AllowedIpAddressCreateRequest request)
    {
        ServiceResult result = await allowedIpAddressService.CreateAsync(request);
        return CreateResult(result);
    }

    /// <summary>
    /// Mevcut bir IP adresini günceller.
    /// </summary>
    /// <param name="request">Güncellenecek IP adresi bilgileri.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] AllowedIpAddressUpdateRequest request)
    {
        ServiceResult result = await allowedIpAddressService.UpdateAsync(request);
        return CreateResult(result);
    }

    /// <summary>
    /// Belirtilen IP adresini siler.
    /// </summary>
    /// <param name="id">Silinecek IP Adresi ID'si.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        ServiceResult result = await allowedIpAddressService.DeleteAsync(id);
        return CreateResult(result);
    }
}

