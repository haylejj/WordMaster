using WordMaster.Application.ViewModels.Admin;

namespace WordMaster.Application.Services.Abstract;

public interface IAdminDashboardService
{
    Task<AdminDashboardViewModel> GetDashboardAsync();
}

