using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IEmailService
{
    Task<ServiceResult> SendResetPasswordLinkToEmailAsync(string resetEmailLink, string toEmail);
    Task<ServiceResult> SendPasswordToEmailAsync(string password, string toEmail, string userName);
}
