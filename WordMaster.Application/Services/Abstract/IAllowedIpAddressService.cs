using WordMaster.Application.Requests.AllowedIpAddress;
using WordMaster.Application.Responses.AllowedIpAddress;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IAllowedIpAddressService
{
    Task<ServiceResult<List<AllowedIpAddressResponse>>> GetAllAsync();
    Task<ServiceResult<AllowedIpAddressResponse>> GetByIdAsync(int id);
    Task<ServiceResult> CreateAsync(AllowedIpAddressCreateRequest request);
    Task<ServiceResult> UpdateAsync(AllowedIpAddressUpdateRequest request);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult<bool>> IsIpAllowedAsync(string ipAddress);
}
