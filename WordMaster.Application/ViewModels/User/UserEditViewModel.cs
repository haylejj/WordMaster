using Microsoft.AspNetCore.Http;
using WordMaster.Domain.Entities;

namespace WordMaster.Application.ViewModels.User;

public class UserEditViewModel
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? City { get; set; }
    public IFormFile? Picture { get; set; }
    public Gender? Gender { get; set; }
}
