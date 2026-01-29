using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Desktop_Crypto_Portfolio_Tracker.Services;

public class EmailService
{
    private readonly string _smtpHost = "smtp.gmail.com";
    private readonly int _smtpPort = 587;


    private readonly string _smtpUser = "zizzo2006@gmail.com";
    private readonly string _smtpPass = "tkcqufibmspkvgpl";

    public void SendVerificationCode(string toEmail, string code)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Crypto Tracker", _smtpUser));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Verification code";

        message.Body = new TextPart("plain")
        {
            Text = $"Your verification code: {code}\nExpires in 10 minutes."
        };

        using var client = new SmtpClient();
        client.Connect(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
        client.Authenticate(_smtpUser, _smtpPass);
        client.Send(message);
        client.Disconnect(true);
    }
}
