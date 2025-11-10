using Microsoft.Extensions.Options;
using System.Net.Mail;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Configuration;

namespace WordMaster.Infrastructure.Services;

public class MailHogEmailService(IOptions<MailHogSettings> settings) : IEmailService
{
    public async Task SendResetPasswordLinkToEmailAsync(string resetEmailLink, string toEmail)
    {
        var smtpClient = new SmtpClient
        {
            Host = settings.Value.Host,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = true,
            Port = settings.Value.Port,
            EnableSsl = settings.Value.EnableSsl
        };

        MailMessage mailMessage = new()
        {
            From = new MailAddress(settings.Value.FromEmail)
        };
        mailMessage.To.Add(toEmail);

        mailMessage.Subject = "Localhost | Şifre sıfırlama linki:";
        mailMessage.Body = $@"
                        <h4> Şifrenizi yenilemek için aşağıdaki linke tıklayınız.</h4>
                        <p><a href='{resetEmailLink}'>Şifre Yenileme Linki</a><p/>";
        mailMessage.IsBodyHtml = true;
        await smtpClient.SendMailAsync(mailMessage);
    }

    public async Task SendPasswordToEmailAsync(string password, string toEmail, string userName)
    {
        var smtpClient = new SmtpClient
        {
            Host = settings.Value.Host,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = true,
            Port = settings.Value.Port,
            EnableSsl = settings.Value.EnableSsl
        };

        MailMessage mailMessage = new()
        {
            From = new MailAddress(settings.Value.FromEmail)
        };
        mailMessage.To.Add(toEmail);

        mailMessage.Subject = "WordMaster | Şifre Sıfırlama";
        mailMessage.Body = $@"
                        <h4>Merhaba {userName},</h4>
                        <p>Hesabınızın şifresi başarıyla sıfırlanmıştır.</p>
                        <p><strong>Yeni Şifreniz:</strong> {password}</p>
                        <p>Güvenliğiniz için lütfen giriş yaptıktan sonra şifrenizi değiştirin.</p>
                        <p>İyi günler dileriz.</p>";
        mailMessage.IsBodyHtml = true;
        await smtpClient.SendMailAsync(mailMessage);
    }
}

