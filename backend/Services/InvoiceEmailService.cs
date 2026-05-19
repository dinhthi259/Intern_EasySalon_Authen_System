public class InvoiceEmailService
{
    private readonly AppDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly IWebHostEnvironment _env;

    public InvoiceEmailService(
        AppDbContext context,
        IEmailSender emailSender,
        IWebHostEnvironment env)
    {
        _context = context;
        _emailSender = emailSender;
        _env = env;
    }

    public async Task SendInvoiceEmailAsync(long invoiceId)
    {
        var invoice = await _context.Invoices.FindAsync(invoiceId);

        if (invoice == null)
        {
            throw new Exception("Không tìm thấy hóa đơn.");
        }

        var pdfPath = Path.Combine(
            _env.WebRootPath,
            invoice.PdfUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
        );

        var body = $@"
            <h2>Xin chào {invoice.CustomerName},</h2>
            <p>Cảm ơn bạn đã mua hàng tại hệ thống.</p>
            <p>Hóa đơn của bạn đã được đính kèm trong email này.</p>
            <p><strong>Mã hóa đơn:</strong> {invoice.InvoiceCode}</p>
            <p><strong>Tổng thanh toán:</strong> {invoice.FinalAmount:N0} VNĐ</p>
        ";

        await _emailSender.SendEmailWithAttachmentAsync(
            invoice.CustomerEmail,
            $"Hóa đơn mua hàng {invoice.InvoiceCode}",
            body,
            pdfPath
        );

        invoice.Status = "Sent";
        invoice.SentAt = DateTime.Now;

        await _context.SaveChangesAsync();
    }
}