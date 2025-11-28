using FluentValidation;
using WordMaster.Application.Requests.Folder;

namespace WordMaster.Application.Validation.Folder;

public class UpdateFolderRequestValidator : AbstractValidator<UpdateFolderRequest>
{
    public UpdateFolderRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Geçersiz klasör ID'si.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Klasör adı boş olamaz.")
            .MaximumLength(50).WithMessage("Klasör adı en fazla 50 karakter olabilir.");
    }
}
