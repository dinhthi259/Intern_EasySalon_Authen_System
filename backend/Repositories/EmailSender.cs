using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

public class EmailSender : IEmailSender
{
    private readonly GmailOptions _gmailOption;

    public EmailSender(IOptions<GmailOptions> gmailOption)
    {
        _gmailOption = gmailOption.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        using MailMessage mailMessage = new MailMessage
        {
            From = new MailAddress(_gmailOption.Email),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(to);

        using var smtpClient = CreateSmtpClient();

        await smtpClient.SendMailAsync(mailMessage);
    }

    public async Task SendEmailWithAttachmentAsync(
        string to,
        string subject,
        string body,
        string attachmentPath)
    {
        if (!File.Exists(attachmentPath))
        {
            throw new Exception("Không tìm thấy file hóa đơn để gửi email.");
        }

        using MailMessage mailMessage = new MailMessage
        {
            From = new MailAddress(_gmailOption.Email),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(to);

        Attachment attachment = new Attachment(attachmentPath);
        mailMessage.Attachments.Add(attachment);

        using var smtpClient = CreateSmtpClient();

        await smtpClient.SendMailAsync(mailMessage);
    }

    private SmtpClient CreateSmtpClient()
    {
        return new SmtpClient
        {
            Host = _gmailOption.Host,
            Port = _gmailOption.Port,
            Credentials = new NetworkCredential(
                _gmailOption.Email,
                _gmailOption.Password
            ),
            EnableSsl = true
        };
    }
}