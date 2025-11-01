using Core.Entity;
using Microsoft.AspNetCore.Http;

namespace Core.ViewModels;

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
