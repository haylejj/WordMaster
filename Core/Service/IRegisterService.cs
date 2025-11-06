using Core.Requests;
using Microsoft.AspNetCore.Identity;
using Core.Results;

namespace Core.Service;

public interface IRegisterService
{
    Task<Result<IEnumerable<IdentityError>>> RegisterAsync(RegisterRequest request);
}
