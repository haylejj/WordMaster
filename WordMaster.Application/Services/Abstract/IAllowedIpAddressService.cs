using WordMaster.Application.Requests.AllowedIpAddress;
using WordMaster.Application.ViewModels.AllowedIpAddress;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IAllowedIpAddressService
{
    Task<List<AllowedIpAddressViewModel>> GetAllAsync();
    Task<Result<AllowedIpAddressViewModel>> GetByIdAsync(int id);
    Task<Result<IEnumerable<string>>> CreateAsync(AllowedIpAddressCreateRequest request);
    Task<Result<IEnumerable<string>>> UpdateAsync(AllowedIpAddressUpdateRequest request);
    Task<Result<IEnumerable<string>>> DeleteAsync(int id);
    Task<bool> IsIpAllowedAsync(string ipAddress);
}

