using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;



[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;


    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var result = await _orderService.CreateOrderAsync(request);

        return Ok(new
        {
            message = request.PaymentMethod == "PAYOS"
                ? "Tạo link thanh toán thành công"
                : "Đặt hàng thành công",

            orderId = result.OrderId,
            paymentMethod = result.PaymentMethod,
            paymentStatus = result.PaymentStatus,

            checkoutUrl = result.CheckoutUrl
        });
    }

    [Authorize]
    [HttpPost("{orderId}/pay-again")]
    public async Task<IActionResult> PayAgain(long orderId)
    {
        try
        {
            var result = await _orderService.PayAgainAsync(orderId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] OrderQueryRequest query)
    {
        var userId = GetUserId();

        var result = await _orderService.GetOrdersAsync(userId, query);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(long id)
    {
        var userId = GetUserId();

        await _orderService.CancelOrderAsync(userId, id);
        return Ok();
    }

    [Authorize]
    [HttpPut("{id}/address")]
    public async Task<IActionResult> UpdateAddress(long id, [FromBody] long addressId)
    {
        var userId = GetUserId();

        await _orderService.UpdateAddressAsync(userId, id, addressId);
        return Ok();
    }

    private long GetUserId()
    {
        return long.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("admin")]
    public async Task<IActionResult> GetOrders([FromQuery] AdminOrderQueryRequest query)
    {
        return Ok(await _orderService.GetAllOrdersAsync(query));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(long id)
    {
        return Ok(await _orderService.GetOrderDetailAsync(id));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateStatusOrderRequest request)
    {
        await _orderService.UpdateOrderStatusAsync(id, request.Status);
        return Ok();
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("admin/count")]
    public async Task<IActionResult> AdminCountOrders()
    {
        var result = await _orderService.AdminCountOrdersByStatusAsync();
        return Ok(result);
    }

    [Authorize]
    [HttpGet("count")]
    public async Task<IActionResult> CountOrders()
    {
        var userId = GetUserId();
        var result = await _orderService.CountOrdersByStatusAsync(userId);
        return Ok(result);
    }
}