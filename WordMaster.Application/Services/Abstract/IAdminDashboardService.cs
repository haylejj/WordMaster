using WordMaster.Application.ViewModels.Admin;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IAdminDashboardService
{
    Task<ServiceResult<AdminDashboardViewModel>> GetDashboardAsync();
}
