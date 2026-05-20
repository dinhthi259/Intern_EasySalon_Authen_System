public class Invoice
{
    public long InvoiceId { get; set; }

    public string InvoiceCode { get; set; } = "";

    public long OrderId { get; set; }

    public string CustomerName { get; set; } = "";

    public string CustomerEmail { get; set; } = "";

    public decimal TotalAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal FinalAmount { get; set; }

    public string? PdfUrl { get; set; }

    public string Status { get; set; } = "Created";

    public bool TaxDeclared { get; set; } = false;

    public long? TaxDeclarationId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? SentAt { get; set; }


    // Navigation Properties
    public virtual Order? Order { get; set; }

    public virtual TaxDeclaration? TaxDeclaration { get; set; }

    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; }
        = new List<InvoiceItem>();

    public virtual ICollection<TaxDeclarationDetail> TaxDeclarationDetails { get; set; }
        = new List<TaxDeclarationDetail>();
}