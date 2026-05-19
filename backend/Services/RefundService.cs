using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class RefundService : IRefundService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public RefundService(AppDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
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