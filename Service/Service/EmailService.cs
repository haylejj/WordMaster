using Core.OptionsModel;
using Core.Service;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Service.Service;

public class EmailService(IOptions<EmailSettings> settings) : IEmailService
{
    public async Task SendResetPasswordLinkToEmailAsync(string resetEmailLink, string toEmail)
    {
        var smtpClient = new SmtpClient();

        smtpClient.Host=settings.Value.Host!;
        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        smtpClient.UseDefaultCredentials = false;
        smtpClient.Port=587;
        smtpClient.Credentials=new NetworkCredential(settings.Value.Email, settings.Value.Password);
        smtpClient.EnableSsl = true;

        var mailMessage = new MailMessage();

        mailMessage.From=new MailAddress(settings.Value.Email!);
        mailMessage.To.Add(toEmail);

        mailMessage.Subject="Localhost | Şifre sıfırlama linki:";
        mailMessage.Body=$@"
                        <h4> Şifrenizi yenilemek için aşağıdaki linke tıklayınız.</h4>
                        <p><a href='{resetEmailLink}'>Şifre Yenileme Linki</a><p/>";
        mailMessage.IsBodyHtml = true;
        await smtpClient.SendMailAsync(mailMessage);
    }
}
