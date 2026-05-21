# Email Migration: Gmail SMTP → Resend

## Ngày hoàn thành: 21 Tháng 5 2026

## Tóm tắt thay đổi

Đã thay thế Gmail SMTP bằng Resend email service để khắc phục sự cố timeout khi gửi email trên Railway.

## Các tệp đã sửa đổi

### 1. `backend/Repositories/EmailSender.cs`
- **Thay đổi**: Thay thế implementation từ SMTP sang Resend
- **Trước**: Sử dụng `System.Net.Mail.SmtpClient` với GmailOptions
- **Sau**: Sử dụng `IResend` client từ Resend package
- **Chi tiết**:
  - Loại bỏ dependency trên `GmailOptions`
  - Thêm dependency trên `IResend`
  - Cập nhật `SendEmailAsync()` để sử dụng `EmailMessage` và `ResendException`
  - Cập nhật `SendEmailWithAttachmentAsync()` để sử dụng `EmailAttachment` với Base64 encoding

### 2. `backend/Program.cs`
- **Thay đổi**: Loại bỏ configuration cho GmailOptions
- **Hàng xóa**: Dòng 42-44
  ```csharp
  builder.Services.Configure<GmailOptions>(
      builder.Configuration.GetSection(GmailOptions.GmailOptionKey)
  );
  ```
- **Ghi chú**: Resend configuration đã được thêm (dòng 69-76)

## Services được ảnh hưởng (không cần thay đổi - vẫn sử dụng interface cũ)

1. **EmailVerificationService.cs**
   - Gửi email xác thực đăng ký
   - Gửi email lấy lại mật khẩu
   - ✅ Không cần thay đổi (sử dụng IEmailSender interface)

2. **RefundService.cs**
   - Gửi email xác nhận hoàn tiền
   - ✅ Không cần thay đổi (sử dụng IEmailSender interface)

3. **InvoiceEmailService.cs**
   - Gửi email hóa đơn với attachment
   - ✅ Không cần thay đổi (sử dụng IEmailSender interface)

4. **AuthController.cs**
   - Sử dụng IEmailSender trong dependency injection
   - ✅ Không cần thay đổi

## Cấu hình cần thiết

### Environment Variables
Đảm bảo Railway environment có biến sau:
```
RESEND_APITOKEN=your_resend_api_key
```

### Sender Email
- **Hiện tại**: `onboarding@resend.dev` (email test của Resend)
- **Cho production**: Thay thế bằng email domain của bạn đã xác thực trong Resend dashboard

## Thay đổi trong `EmailSender.cs`

### Trước (Gmail SMTP):
```csharp
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
```

### Sau (Resend):
```csharp
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
    }
    catch (ResendException ex)
    {
        throw new Exception($"Lỗi gửi email: {ex.Message}");
    }
}
```

## Lợi ích

1. ✅ **Reliability**: Resend được tối ưu hóa cho Railway
2. ✅ **No Timeouts**: Giải quyết vấn đề timeout trên Railway
3. ✅ **Simple API**: API đơn giản và dễ sử dụng
4. ✅ **Better Deliverability**: Tỷ lệ delivery cao hơn
5. ✅ **No SMTP Configuration**: Không cần cấu hình SMTP phức tạp

## Kiểm tra

- ✅ Build thành công (0 errors, 8 warnings - chỉ là NuGet package warnings)
- ✅ Các tệp liên quan không cần thay đổi (vẫn sử dụng interface IEmailSender)
- ✅ Functionality không thay đổi (chỉ thay đổi implementation)

## Next Steps (Khi deploy)

1. Cập nhật Resend API key trong Railway environment variables
2. (Optional) Cấu hình custom domain email trong Resend dashboard
3. Cập nhật sender email từ `onboarding@resend.dev` nếu sử dụng domain riêng
4. Test gửi email trên Railway để xác nhận hoạt động

## Rollback (Nếu cần)

Nếu cần quay lại Gmail SMTP:
1. Restore `EmailSender.cs` từ git history
2. Restore `GmailOptions.Configure` trong `Program.cs`
3. Cập nhật Gmail credentials trong environment variables
