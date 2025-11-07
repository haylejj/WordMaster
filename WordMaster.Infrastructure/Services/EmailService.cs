using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Configuration;

namespace WordMaster.Infrastructure.Services;

public class EmailService(IOptions<EmailSettings> settings) : IEmailService
{
    public async Task SendResetPasswordLinkToEmailAsync(string resetEmailLink, string toEmail)
    {
        var smtpClient = new SmtpClient
        {
            Host=settings.Value.Host!,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Port=587,
            Credentials=new NetworkCredential(settings.Value.Email, settings.Value.Password),
            EnableSsl = true
        };

        MailMessage mailMessage = new()
        {
            From=new MailAddress(settings.Value.Email!)
        };
        mailMessage.To.Add(toEmail);

        mailMessage.Subject="Localhost | Þifre sýfýrlama linki:";
        mailMessage.Body=$@"
                        <h4> Þifrenizi yenilemek için aþaðýdaki linke týklayýnýz.</h4>
                        <p><a href='{resetEmailLink}'>Þifre Yenileme Linki</a><p/>";
        mailMessage.IsBodyHtml = true;
        await smtpClient.SendMailAsync(mailMessage);
    }
}
