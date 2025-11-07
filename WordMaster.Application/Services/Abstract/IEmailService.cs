namespace WordMaster.Application.Services.Abstract;

public interface IEmailService
{
    Task SendResetPasswordLinkToEmailAsync(string resetEmailLink, string toEmail);
}
