using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Security.Claims;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequirePermissionAttribute(string areaName, string controllerName, string actionName, string httpMethod, string Description) : Attribute, IAsyncActionFilter
{
    public string AreaName { get; } = areaName;
    public string ControllerName { get; } = controllerName;
    public string ActionName { get; } = actionName;
    public string HttpMethod { get; } = httpMethod;
    public string Description { get; } = Description;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        HttpContext httpContext = context.HttpContext;
        ClaimsPrincipal user = httpContext.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            ServiceResult result = ServiceResult.Failure("Lütfen giriş yapınız.", HttpStatusCode.Unauthorized);
            context.Result = new ObjectResult(result) { StatusCode = (int)result.StatusCode };
            return;
        }

        IPermissionService permissionService = httpContext.RequestServices.GetRequiredService<IPermissionService>();
        UserManager<AppUser> userManager = httpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();

        string? userId = userManager.GetUserId(user);
        if (string.IsNullOrEmpty(userId))
        {
            ServiceResult result = ServiceResult.Failure("Lütfen giriş yapınız.", HttpStatusCode.Unauthorized);
            context.Result = new ObjectResult(result) { StatusCode = (int)result.StatusCode };
            return;
        }

        string key = $"{AreaName}_{ControllerName}_{ActionName}_{HttpMethod}";

        // Check if user has permission
        ServiceResult<bool> serviceResult = await permissionService.HasPermissionAsync(Guid.Parse(userId), key);

        if (!serviceResult.IsSuccess || !serviceResult.Data)
        {
            ServiceResult result = ServiceResult.Failure("Bu işlemi yapmaya yetkiniz yok.", HttpStatusCode.Forbidden);
            context.Result = new ObjectResult(result) { StatusCode = (int)result.StatusCode };
            return;
        }

        await next();
    }
}
