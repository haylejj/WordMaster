using FluentValidation;
using WordMaster.Application.Requests.Database;

namespace WordMaster.Application.Validation.Database;

public class ResetTableRequestValidator : AbstractValidator<ResetTableRequest>
{
    private static readonly string[] AllowedTables = { "words", "unknows", "favorites", "folders", "folderwords" };

    public ResetTableRequestValidator()
    {
        RuleFor(x => x.TableName)
            .NotEmpty().WithMessage("Tablo adı boş bırakılamaz.")
            .Must(t => AllowedTables.Contains(t?.ToLower()))
            .WithMessage($"Geçersiz tablo adı. İzin verilenler: {string.Join(", ", AllowedTables)}");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre alanı boş bırakılamaz.");

        RuleFor(x => x.TargetUserId)
            .Must(id => string.IsNullOrEmpty(id) || Guid.TryParse(id, out _))
            .WithMessage("Hedef kullanıcı ID'si geçerli bir GUID formatında olmalıdır.");
    }
}
