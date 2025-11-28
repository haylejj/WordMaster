using FluentValidation;
using WordMaster.Application.Requests.Folder;

namespace WordMaster.Application.Validation.Folder;

public class AddWordToFolderRequestValidator : AbstractValidator<AddWordToFolderRequest>
{
    public AddWordToFolderRequestValidator()
    {
        RuleFor(x => x.FolderId)
            .GreaterThan(0).WithMessage("Geçersiz klasör ID'si.");

        RuleFor(x => x.WordId)
            .GreaterThan(0).WithMessage("Geçersiz kelime ID'si.");
    }
}
