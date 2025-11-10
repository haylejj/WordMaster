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
}

