using Core.Requests;
using Microsoft.AspNetCore.Identity;

namespace Core.Service
{
    public interface IRegisterService
    {
        Task<(bool, IEnumerable<IdentityError>?)> RegisterAsync(RegisterRequest request);
    }
}
