using WordMaster.Application.Requests.AllowedIpAddress;
using WordMaster.Application.ViewModels.AllowedIpAddress;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IAllowedIpAddressService
{
    Task<List<AllowedIpAddressViewModel>> GetAllAsync();
    Task<ServiceResult<AllowedIpAddressViewModel>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<string>>> CreateAsync(AllowedIpAddressCreateRequest request);
    Task<ServiceResult<IEnumerable<string>>> UpdateAsync(AllowedIpAddressUpdateRequest request);
    Task<ServiceResult<IEnumerable<string>>> DeleteAsync(int id);
    Task<bool> IsIpAllowedAsync(string ipAddress);
}

