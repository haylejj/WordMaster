using WordMaster.Application.Requests.AllowedIpAddress;
using WordMaster.Application.ViewModels.AllowedIpAddress;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IAllowedIpAddressService
{
    Task<List<AllowedIpAddressViewModel>> GetAllAsync();
    Task<ServiceResult<AllowedIpAddressViewModel>> GetByIdAsync(int id);
    Task<ServiceResult> CreateAsync(AllowedIpAddressCreateRequest request);
    Task<ServiceResult> UpdateAsync(AllowedIpAddressUpdateRequest request);
    Task<ServiceResult> DeleteAsync(int id);
    Task<bool> IsIpAllowedAsync(string ipAddress);
}
