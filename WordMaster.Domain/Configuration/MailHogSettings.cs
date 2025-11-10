namespace WordMaster.Domain.Configuration;

public class MailHogSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public bool EnableSsl { get; set; } = false;
    public string FromEmail { get; set; } = "dev@localhost";
}

