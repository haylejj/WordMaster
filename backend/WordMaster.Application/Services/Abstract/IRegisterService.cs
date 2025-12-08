using WordMaster.Application.Requests.Auth;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IRegisterService
{
    Task<ServiceResult> RegisterAsync(RegisterRequest request);
    Task<ServiceResult> ConfirmEmailAsync(string userId, string token);
}
