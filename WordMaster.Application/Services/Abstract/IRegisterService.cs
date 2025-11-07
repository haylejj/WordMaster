using Microsoft.AspNetCore.Identity;
using WordMaster.Application.Requests;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IRegisterService
{
    Task<Result<IEnumerable<IdentityError>>> RegisterAsync(RegisterRequest request);
}
