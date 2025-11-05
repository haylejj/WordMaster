using Core.ViewModels;


namespace Core.Service;

public interface IAdminService
{
    Task<List<UserViewModel>> GetUsersAsync();
}
