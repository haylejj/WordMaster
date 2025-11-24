using System;
using Microsoft.AspNetCore.Http;
using WordMaster.Domain.Entities;

namespace WordMaster.Application.Responses.User;

public class UserEditResponse
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
    public IFormFile? Picture { get; set; }
    public Gender? Gender { get; set; }
}
