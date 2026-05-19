public interface IInvoiceService
{
    Task<Invoice> CreateInvoiceAsync(long orderId);
}