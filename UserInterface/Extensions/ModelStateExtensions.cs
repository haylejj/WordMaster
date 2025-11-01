using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UserInterface.Extensions;

public static class ModelStateExtensions
{
    public static void AddModelErrorList(this ModelStateDictionary modelState, List<string> errors)
    {
        errors.ForEach(x =>
        {
            modelState.AddModelError(string.Empty, x);
        });
    }
}
