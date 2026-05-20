public class TaxDeclarationDetail
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


    // Navigation Properties
    public virtual TaxDeclaration? TaxDeclaration { get; set; }

    public virtual Invoice? Invoice { get; set; }
}