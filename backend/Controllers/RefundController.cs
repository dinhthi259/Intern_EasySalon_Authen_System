using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/refunds")]
public class RefundController : ControllerBase
{
    private readonly IRefundService _refundService;

    public RefundController(IRefundService refundService)
    {
        _refundService = refundService;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    [HttpPost("orders/{orderId}")]
    public async Task<IActionResult> CreateRefundRequest(
        long orderId,
        CancelPaidOrderRequest request)
    {
        try
        {
            await _refundService.CreateRefundRequestAsync(
                GetUserId(),
                orderId,
                request);

            return Ok("Đã gửi yêu cầu hoàn tiền");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _refundService.GetAllRefundRequestsAsync();
        return Ok(result);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("{refundId}/confirm")]
    public async Task<IActionResult> ConfirmRefund(int refundId)
    {
        try
        {
            await _refundService.ConfirmRefundAsync(refundId);
            return Ok("Đã xác nhận hoàn tiền");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("{refundId}/reject")]
    public async Task<IActionResult> RejectRefund(
        int refundId,
        RejectRefundRequest request)
    {
        try
        {
            await _refundService.RejectRefundAsync(refundId, request.Reason);
            return Ok("Đã từ chối yêu cầu hoàn tiền");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}