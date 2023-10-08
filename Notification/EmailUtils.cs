using System.Net;
using System.Net.Mail;

namespace Notification;

public class Email
{
    public Email(string addr, string sub, string tex)
    {
        Address = addr;
        Subject = sub;
        Text = tex;
    }

    public string Address;
    public string Subject;
    public string Text;
}

public class EmailSender
{
    public EmailSender(string server, string sendAddr, string emailPw)
    {
        smtpClient = new SmtpClient(server)
        {
            Port = 587,
            Credentials = new NetworkCredential(sendAddr, emailPw),
            EnableSsl = true,
        };
    }

    public void SendEmails(List<Email> emails)
    {
        foreach (Email email in emails)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress("Planes@Overhead.com"),
                Subject = email.Subject,
                Body = email.Text,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email.Address);

            smtpClient.Send(mailMessage);
        }
    }

    public SmtpClient smtpClient;
}
