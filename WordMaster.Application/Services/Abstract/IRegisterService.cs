using Microsoft.AspNetCore.Identity;
using WordMaster.Application.Requests.Auth;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IRegisterService
{
    Task<ServiceResult<IEnumerable<IdentityError>>> RegisterAsync(RegisterRequest request);
}
