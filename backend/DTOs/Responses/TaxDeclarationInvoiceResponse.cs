public class TaxDeclarationInvoiceResponse
{
    public long TaxDeclarationDetailId { get; set; }
    public long TaxDeclarationId { get; set; }
    public long InvoiceId { get; set; }
    public string InvoiceCode { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public decimal RevenueAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public DateTime InvoiceCreatedAt { get; set; }
    public long ImportId { get; set; }
    public string ImportCode { get; set; } = "";
    public decimal PurchaseAmount { get; set; }
    public decimal PurchaseTaxAmount { get; set; }
    public decimal PurchaseFinalAmount { get; set; }
    public DateTime ImportCreatedAt { get; set; }
}