using Resend;

public class EmailSender : IEmailSender
{
    private readonly IResend _resendClient;
    private readonly string _senderEmail;

    public EmailSender(IResend resendClient)
    {
        _resendClient = resendClient;
        _senderEmail = "onboarding@resend.dev"; // Sử dụng email mặc định của Resend hoặc thay thế bằng email đã xác thực
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var emailRequest = new EmailMessage
            {
                From = _senderEmail,
                To = new[] { to },
                Subject = subject,
                HtmlBody = body
            };

            var response = await _resendClient.EmailSendAsync(emailRequest);
            
            // If we reach here, the email was sent successfully (response.Content contains the email ID)
        }
        catch (ResendException ex)
        {
            throw new Exception($"Lỗi gửi email: {ex.Message}");
        }
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

        try
        {
            // Đọc file và chuyển đổi thành base64
            byte[] fileBytes = await File.ReadAllBytesAsync(attachmentPath);
            string fileName = Path.GetFileName(attachmentPath);

            var attachment = new EmailAttachment
            {
                Filename = fileName,
                Content = Convert.ToBase64String(fileBytes),
                ContentType = "application/pdf"
            };

            var emailRequest = new EmailMessage
            {
                From = _senderEmail,
                To = new[] { to },
                Subject = subject,
                HtmlBody = body,
                Attachments = new List<EmailAttachment> { attachment }
            };

            var response = await _resendClient.EmailSendAsync(emailRequest);
            
            // If we reach here, the email was sent successfully (response.Content contains the email ID)
        }
        catch (ResendException ex)
        {
            throw new Exception($"Lỗi gửi email: {ex.Message}");
        }
    }
}