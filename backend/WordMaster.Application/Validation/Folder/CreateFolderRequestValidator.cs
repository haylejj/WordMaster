using FluentValidation;
using WordMaster.Application.Requests.Folder;

namespace WordMaster.Application.Validation.Folder;

public class CreateFolderRequestValidator : AbstractValidator<CreateFolderRequest>
{
    public CreateFolderRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Klasör adı boş olamaz.")
            .MaximumLength(50).WithMessage("Klasör adı en fazla 50 karakter olabilir.");
    }
}
