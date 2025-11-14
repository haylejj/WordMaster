using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using WordMaster.Domain.Entities;

namespace WordMaster.WebUI.TagHelpers;

public class UserRoleNameTagHelper(UserManager<AppUser> userManager) : TagHelper
{
    public string UserId { get; set; } = null!;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        AppUser? user = await userManager.FindByIdAsync(UserId);

        IList<string> userRoles = await userManager.GetRolesAsync(user!);

        StringBuilder stringBuilder = new();

        userRoles.ToList().ForEach(x =>
        {
            stringBuilder.Append(@$"
                <span class='badge bg-secondary mx-1'>{x.ToLower()}</span>");
        });
        output.Content.SetHtmlContent(stringBuilder.ToString());
    }
}
