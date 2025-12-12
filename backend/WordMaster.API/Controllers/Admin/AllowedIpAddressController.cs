using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Attributes;
using WordMaster.Application.Requests.AllowedIpAddress;
using WordMaster.Application.Responses.AllowedIpAddress;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers.Admin;

/// <summary>
/// İzin verilen IP adreslerini yöneten admin controller.
/// </summary>
[Route("api/v{version:apiVersion}/admin/allowed-ips")]
public class AllowedIpAddressController(IAllowedIpAddressService allowedIpAddressService) : BaseController
{
    /// <summary>
    /// Tüm izin verilen IP adreslerini listeler.
    /// </summary>
    /// <returns>IP adresleri listesi.</returns>
    [HttpGet]
    [RequirePermission("Admin", "AllowedIps", "GetAll", "GET", "İzin verilen IP'leri listele")]
    public async Task<IActionResult> GetAll()
    {
        ServiceResult<List<AllowedIpAddressResponse>> result = await allowedIpAddressService.GetAllAsync();
        return CreateResult(result);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip IP adresini getirir.
    /// </summary>
    /// <param name="id">IP Adresi ID'si.</param>
    /// <returns>IP adresi detayları.</returns>
    [HttpGet("{id}")]
    [RequirePermission("Admin", "AllowedIps", "GetById", "GET", "İzin verilen IP detayını getir")]
    public async Task<IActionResult> GetById(int id)
    {
        ServiceResult<AllowedIpAddressResponse> result = await allowedIpAddressService.GetByIdAsync(id);
        return CreateResult(result);
    }

    /// <summary>
    /// Yeni bir izin verilen IP adresi ekler.
    /// </summary>
    /// <param name="request">Eklenecek IP adresi bilgileri.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpPost]
    [RequirePermission("Admin", "AllowedIps", "Create", "POST", "İzin verilen IP ekle")]
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
    [RequirePermission("Admin", "AllowedIps", "Update", "PUT", "İzin verilen IP güncelle")]
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
    [RequirePermission("Admin", "AllowedIps", "Delete", "DELETE", "İzin verilen IP sil")]
    public async Task<IActionResult> Delete(int id)
    {
        ServiceResult result = await allowedIpAddressService.DeleteAsync(id);
        return CreateResult(result);
    }
}

