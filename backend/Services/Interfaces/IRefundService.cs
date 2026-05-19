public interface IRefundService
{
    Task CreateRefundRequestAsync(long userId, long orderId, CancelPaidOrderRequest request);
    Task<List<RefundRequest>> GetAllRefundRequestsAsync();
    Task ConfirmRefundAsync(long refundId);
    Task RejectRefundAsync(long refundId, string reason);
}