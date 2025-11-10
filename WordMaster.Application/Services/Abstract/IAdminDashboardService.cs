using WordMaster.Application.ViewModels;

namespace WordMaster.Application.Services.Abstract;

public interface IAdminDashboardService
{
    Task<AdminDashboardViewModel> GetDashboardAsync();
}

