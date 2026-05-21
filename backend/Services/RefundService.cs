using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class RefundService : IRefundService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IEmailSender _emailSender;

    public RefundService(AppDbContext context, IHubContext<NotificationHub> hubContext, IEmailSender emailSender)
    {
        _context = context;
        _hubContext = hubContext;
        _emailSender = emailSender;
    }

    public async Task CreateRefundRequestAsync(
        long userId,
        long orderId,
        CancelPaidOrderRequest request)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(x => x.Id == orderId && x.UserId == userId);

        if (order == null)
            throw new Exception("Không tìm thấy đơn hàng");

        if (order.PaymentMethod != "PAYOS")
            throw new Exception("Đơn hàng này không cần hoàn tiền online");

        if (order.PaymentStatus != "Đã thanh toán")
            throw new Exception("Đơn hàng chưa thanh toán");

        if (order.Status == "Đã hủy")
            throw new Exception("Đơn hàng đã bị hủy");

        if (order.Status == "Đang giao" || order.Status == "Hoàn tất")
            throw new Exception("Không thể hủy đơn ở trạng thái hiện tại");

        var existedRefund = await _context.RefundRequests
            .AnyAsync(x => x.OrderId == orderId && x.Status == "Pending");

        if (existedRefund)
            throw new Exception("Đơn hàng đã có yêu cầu hoàn tiền");

        var bankAccount = await _context.UserBankAccounts
            .FirstOrDefaultAsync(x =>
                x.Id == request.BankAccountId &&
                x.UserId == userId);

        if (bankAccount == null)
            throw new Exception("Không tìm thấy tài khoản ngân hàng");

        var refund = new RefundRequest
        {
            OrderId = order.Id,
            UserId = userId,
            Amount = order.TotalAmount,
            Status = "Pending",
            Reason = request.Reason,
            BankName = bankAccount.BankName,
            BankAccountNumber = bankAccount.BankAccountNumber,
            BankAccountName = bankAccount.BankAccountName,
            BankLogo = bankAccount.BankLogo
        };

        order.Status = "Chờ hoàn tiền";
        order.UpdateAt = DateTime.Now;

        _context.RefundRequests.Add(refund);
        var notification = new Notification
        {
            UserId = order.UserId,
            Title = "Gửi yêu cầu hoàn tiền",
            Message = $"Đơn hàng #{order.Id} đã được gửi yêu cầu hoàn tiền. Vui lòng chờ người bán xử lý",
            Type = "CREATE_REFUND",
            IsRead = false,
            CreatedAt = DateTime.Now,
            Link = $"/my-orders"
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        await _hubContext.Clients
            .Group($"user_{order.UserId}")
            .SendAsync("ReceiveNotification", notification);
    }

    public async Task<List<RefundRequest>> GetAllRefundRequestsAsync()
    {
        return await _context.RefundRequests
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }


    public async Task ConfirmRefundAsync(long refundId)
    {
        var refund = await _context.RefundRequests
            .FirstOrDefaultAsync(x => x.Id == refundId);

        if (refund == null)
            throw new Exception("Không tìm thấy yêu cầu hoàn tiền");

        if (refund.Status != "Pending")
            throw new Exception("Yêu cầu hoàn tiền đã được xử lý");

        var order = await _context.Orders
            .Include(x => x.User)
                .ThenInclude(u => u.Profile)
            .FirstOrDefaultAsync(x => x.Id == refund.OrderId);

        if (order == null)
            throw new Exception("Không tìm thấy đơn hàng");

        refund.Status = "Refunded";
        refund.RefundedAt = DateTime.Now;

        order.Status = "Đã hủy";
        order.PaymentStatus = "Đã hoàn tiền";
        order.UpdateAt = DateTime.Now;

        var notification = new Notification
        {
            UserId = order.UserId,
            Title = "Hoàn tiền thành công",
            Message = $"Yêu cầu hoàn tiền cho đơn hàng #{order.Id} đã được xử lý",
            Type = "REFUND_SUCCESS",
            IsRead = false,
            CreatedAt = DateTime.Now,
            Link = $"/my-orders"
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        await _hubContext.Clients
            .Group($"user_{order.UserId}")
            .SendAsync("ReceiveNotification", notification);

        var customerName = order.User.Profile?.FullName ?? "Quý khách";
        var customerEmail = order.User.Email;

        var emailSubject = $"Hoàn tiền thành công cho đơn hàng #{order.Id}";

        var emailBody = $@"
        <h2>Xin chào {customerName},</h2>

        <p>Yêu cầu hoàn tiền cho đơn hàng <strong>#{order.Id}</strong> của bạn đã được xử lý thành công.</p>

        <p><strong>Trạng thái đơn hàng:</strong> Đã hủy</p>
        <p><strong>Trạng thái thanh toán:</strong> Đã hoàn tiền</p>
        <p><strong>Thời gian hoàn tiền:</strong> {refund.RefundedAt:dd/MM/yyyy HH:mm}</p>

        <p>Số tiền hoàn lại sẽ được xử lý theo phương thức thanh toán ban đầu của bạn.</p>

        <br/>
        <p>Trân trọng,<br/>
        Công ty cổ phần Tech AI Việt Nam</p>
    ";

        await _emailSender.SendEmailAsync(customerEmail,emailSubject,emailBody);
    }

    public async Task RejectRefundAsync(long refundId, string reason)
    {
        var refund = await _context.RefundRequests
            .FirstOrDefaultAsync(x => x.Id == refundId);

        if (refund == null)
            throw new Exception("Không tìm thấy yêu cầu hoàn tiền");

        if (refund.Status != "Pending")
            throw new Exception("Yêu cầu hoàn tiền đã được xử lý");

        var order = await _context.Orders
            .FirstOrDefaultAsync(x => x.Id == refund.OrderId);

        refund.Status = "Rejected";
        refund.Reason = reason;

        if (order != null)
        {
            order.Status = "Chờ xác nhận";
            order.PaymentStatus = "Đã thanh toán";
            order.UpdateAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
    }
}