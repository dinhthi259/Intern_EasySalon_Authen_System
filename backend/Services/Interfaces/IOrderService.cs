public interface IOrderService
{
    //User
    Task<CreateOrderResponseDto> CreateOrderAsync(CreateOrderRequest request);
    Task<List<OrderDto>> GetOrdersAsync(long userId, OrderQueryRequest query);
    Task CancelOrderAsync(long userId, long orderId);
    Task UpdateAddressAsync(long userId, long orderId, long newAddressId);
    Task<PayAgainResponseDto> PayAgainAsync(long orderId);

    //Admin
    Task<List<OrderDto>> GetAllOrdersAsync(AdminOrderQueryRequest query);
    Task<OrderDetailDto> GetOrderDetailAsync(long orderId);
    Task UpdateOrderStatusAsync(long orderId, string status);
    Task<List<OrderCountByStatusDto>> AdminCountOrdersByStatusAsync();
    Task<List<OrderCountByStatusDto>> CountOrdersByStatusAsync(long userId);
}