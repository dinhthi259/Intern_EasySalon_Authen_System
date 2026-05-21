using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PayOS;
using PayOS.Models;
using PayOS.Models.Webhooks;


[ApiController]
[Route("api/payos")]
public class PayOSController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PayOSClient _payOS;
    private readonly IHubContext<NotificationHub> _hubContext;

    public PayOSController(AppDbContext context, PayOSClient payOS, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _payOS = payOS;
        _hubContext = hubContext;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> PayOSWebhook(
    [FromBody] Webhook webhookBody)
    {
        var verifiedData = await _payOS.Webhooks.VerifyAsync(webhookBody);

        var payment = await _context.Payments
            .FirstOrDefaultAsync(x => x.OrderCode == verifiedData.OrderCode);

        if (payment == null)
            return Ok();

        if (payment.Status == "Paid")
            return Ok();

        payment.Status = "Paid";

        var order = await _context.Orders.FindAsync(payment.OrderId);

        if (order != null)
        {
            order.PaymentStatus = "Đã thanh toán";
            order.UpdateAt = DateTime.Now;
            order.Status = "Chờ xác nhận";
        }
        var notification = new Notification
        {
            UserId = order.UserId,
            Title = "Thanh toán thành công",
            Message = $"Đơn hàng #{order.Id} đã được thanh toán thành công",
            Type = "PAYMENT_SUCCESS",
            IsRead = false,
            CreatedAt = DateTime.Now,
            Link = $"/my-orders"
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        await _hubContext.Clients
            .Group($"user_{order.UserId}")
            .SendAsync("ReceiveNotification", notification);

        return Ok();
    }
}