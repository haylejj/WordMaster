namespace Core.Service;

public interface IEmailService
{
    Task SendResetPasswordLinkToEmailAsync(string resetEmailLink, string toEmail);
}
