using WordMaster.Application.Responses.Admin;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IAdminDashboardService
{
    Task<ServiceResult<AdminDashboardResponse>> GetDashboardAsync();
}
